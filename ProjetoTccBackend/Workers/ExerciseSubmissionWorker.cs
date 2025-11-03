using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using ProjetoTccBackend.Database.Responses.Exercise;
using ProjetoTccBackend.Enums;
using ProjetoTccBackend.Hubs;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Services.Interfaces;
using ProjetoTccBackend.Workers.Queues;

namespace ProjetoTccBackend.Workers
{
    /// <summary>
    /// A background service that processes exercise submissions from a queue and broadcasts results to connected
    /// clients.
    /// </summary>
    /// <remarks>The <see cref="ExerciseSubmissionWorker"/> continuously dequeues exercise submissions,
    /// processes them using the provided services, and sends the results to relevant client groups via SignalR. It
    /// utilizes caching to optimize performance when retrieving competition data and ensures proper dependency
    /// resolution through scoped service providers.</remarks>
    public class ExerciseSubmissionWorker : BackgroundService
    {
        private readonly ExerciseSubmissionQueue _queue;
        private readonly IHubContext<CompetitionHub> _hubContext;
        private readonly IServiceProvider _serviceProvider;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<ExerciseSubmissionWorker> _logger;

        /// <summary>
        /// Represents the maximum number of worker threads that can be utilized.
        /// </summary>
        /// <remarks>This constant defines an upper limit for worker thread allocation, ensuring that no
        /// more than the specified number of threads are used in operations. It is intended for internal use
        /// only.</remarks>
        private const int MAX_WORKER_COUNT = 8;

        private const string CompetitionCacheKey = "currentCompetition";

        /// <summary>
        /// Initializes a new instance of the <see cref="ExerciseSubmissionWorker"/> class.
        /// </summary>
        /// <param name="queue">The queue used to manage exercise submissions for processing.</param>
        /// <param name="hubContext">The SignalR hub context for broadcasting updates to connected clients.</param>
        /// <param name="serviceProvider">The service provider used to resolve dependencies during processing.</param>
        /// <param name="memoryCache">The memory cache used for storing temporary data during processing.</param>
        /// <param name="logger">The logger used to record information, warnings, and errors during the processing of exercise submissions.</param>
        public ExerciseSubmissionWorker(
            ExerciseSubmissionQueue queue,
            IHubContext<CompetitionHub> hubContext,
            IServiceProvider serviceProvider,
            IMemoryCache memoryCache,
            ILogger<ExerciseSubmissionWorker> logger
        )
        {
            this._queue = queue;
            this._hubContext = hubContext;
            this._serviceProvider = serviceProvider;
            this._memoryCache = memoryCache;
            this._logger = logger;
        }

        /// <summary>
        /// Asynchronously retrieves the current competition, utilizing caching to improve performance.
        /// </summary>
        /// <remarks>This method first attempts to retrieve the competition from an in-memory cache. If
        /// the competition is not found in the cache, it fetches the current competition from the underlying service
        /// and stores it in the cache with an expiration time based on the competition's end time.</remarks>
        /// <param name="serviceScope">The <see cref="IServiceScope"/> used to resolve the required services for retrieving the competition.</param>
        /// <returns>The current <see cref="Competition"/> if one is available; otherwise, <see langword="null"/>.</returns>
        private async Task<Competition?> FetchCurrentCompetitionAsync(IServiceScope serviceScope)
        {
            if (_memoryCache.TryGetValue(CompetitionCacheKey, out Competition? competition))
            {
                return competition;
            }

            ICompetitionService competitionService =
                serviceScope.ServiceProvider.GetRequiredService<ICompetitionService>();

            competition = await competitionService.GetCurrentCompetition();

            if (competition is not null)
            {
                var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(
                    competition.EndTime!.Value
                );

                this._memoryCache.Set(CompetitionCacheKey, competition, cacheEntryOptions);
            }

            return competition;
        }

        /// <summary>
        /// Starts parallel processing of items from the exercise submission queue.
        /// </summary>
        /// <remarks>
        /// This method dynamically determines the number of worker tasks to create, based on the greater value between the number of processor cores and the current queue size.
        /// Each worker continuously reads items from the <see cref="ExerciseSubmissionQueue"/> and processes exercise submission attempts within isolated service scopes.
        /// Processing continues until the <paramref name="cancellationToken"/> is signaled.
        /// If a null item is returned from the queue, a critical log entry is recorded and that cycle is skipped.
        /// At the end, the method awaits the completion of all processing tasks.
        /// </remarks>
        /// <param name="cancellationToken">Cancellation token that interrupts the processing of workers.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous execution of the background service.</returns>
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            this._logger.LogInformation("ExerciseSubmissionWorkerInitialized...");

            int queueSize = this._queue.GetQueueSize();

            if (queueSize == 0)
            {
                return;
            }

            int workerCount = Environment.ProcessorCount;
            workerCount = (queueSize < workerCount) ? queueSize : workerCount;

            workerCount = (workerCount > MAX_WORKER_COUNT) ? MAX_WORKER_COUNT : workerCount;

            List<Task> tasks = new List<Task>();

            for (int i = 0; i < workerCount; i++)
            {
                tasks.Add(
                    Task.Run(
                        async () =>
                        {
                            while (!cancellationToken.IsCancellationRequested)
                            {
                                var queueItem = await this._queue.DequeueAsync(cancellationToken);

                                if (queueItem is null)
                                {
                                    this._logger.LogCritical(
                                        $"null item received from queue submission."
                                    );
                                    continue;
                                }

                                using (var scope = this._serviceProvider.CreateScope())
                                {
                                    try
                                    {
                                        await this.ProcessExerciseAttempt(scope, queueItem);
                                    }
                                    catch (Exception ex)
                                    {
                                        this._logger.LogCritical(
                                            eventId: (int)LoggerEvent.ExerciseSubmissionQueue,
                                            exception: ex,
                                            message: $"The processing of ExerciseId: {queueItem.Request.ExerciseId}, from: {queueItem.ConnectionId} failed"
                                        );
                                    }
                                }
                            }
                        },
                        cancellationToken
                    )
                );
            }

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Processes an exercise attempt submitted by a user and sends the result to relevant clients.
        /// </summary>
        /// <remarks>This method retrieves the current competition context, processes the exercise
        /// submission using the group attempt service, and sends the resulting response to multiple client groups,
        /// including administrators, teachers, and the submitting client. Additionally, it broadcasts the updated
        /// ranking to all connected clients in the Students, Teachers, and Admins groups.</remarks>
        /// <param name="serviceScope">The <see cref="IServiceScope"/> used to resolve scoped services for the operation.</param>
        /// <param name="queueItem">The <see cref="ExerciseSubmissionQueueItem"/> containing the details of the exercise submission, including
        /// the request data and the connection ID of the submitting client.</param>
        /// <returns></returns>
        private async Task ProcessExerciseAttempt(
            IServiceScope serviceScope,
            ExerciseSubmissionQueueItem queueItem
        )
        {
            var currentCompetition = await this.FetchCurrentCompetitionAsync(serviceScope);
            
            if (currentCompetition is null)
            {
                this._logger.LogWarning("No active competition found for exercise submission processing");
                return;
            }

            var groupAttemptService =
                serviceScope.ServiceProvider.GetRequiredService<IGroupAttemptService>();

            var (submissionResponse, rankingResponse) = await groupAttemptService.SubmitExerciseAttempt(
                currentCompetition,
                queueItem.Request
            );

            // Send submission response to admins and teachers
            await this
                ._hubContext.Clients.Group("Admins")
                .SendAsync("ReceiveExerciseAttempt", submissionResponse);
            await this
                ._hubContext.Clients.Group("Teachers")
                .SendAsync("ReceiveExerciseAttempt", submissionResponse);
            
            // Send submission response to the submitting client
            await this
                ._hubContext.Clients.Client(queueItem.ConnectionId)
                .SendAsync("ReceiveExerciseAttemptResponse", submissionResponse);

            // Broadcast updated ranking to all participants
            await this
                ._hubContext.Clients.Group("Students")
                .SendAsync("ReceiveRankingUpdate", rankingResponse);
            await this
                ._hubContext.Clients.Group("Teachers")
                .SendAsync("ReceiveRankingUpdate", rankingResponse);
            await this
                ._hubContext.Clients.Group("Admins")
                .SendAsync("ReceiveRankingUpdate", rankingResponse);
        }
    }
}
