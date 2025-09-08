using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using ProjetoTccBackend.Database.Responses.Exercise;
using ProjetoTccBackend.Database.Responses.Judge;
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
        private readonly IGroupAttemptService _groupAttemptService;
        private readonly IHubContext<CompetitionHub> _hubContext;
        private readonly IServiceProvider _serviceProvider;
        private readonly IMemoryCache _memoryCache;

        private const string CompetitionCacheKey = "currentCompetition";

        /// <summary>
        /// Initializes a new instance of the <see cref="ExerciseSubmissionWorker"/> class.
        /// </summary>
        /// <param name="queue">The queue used to manage exercise submissions for processing.</param>
        /// <param name="groupAttemptService">The service used to handle group attempt operations.</param>
        /// <param name="hubContext">The SignalR hub context for broadcasting updates to connected clients.</param>
        /// <param name="serviceProvider">The service provider used to resolve dependencies during processing.</param>
        /// <param name="memoryCache">The memory cache used for storing temporary data during processing.</param>
        public ExerciseSubmissionWorker(
            ExerciseSubmissionQueue queue,
            IGroupAttemptService groupAttemptService,
            IHubContext<CompetitionHub> hubContext,
            IServiceProvider serviceProvider,
            IMemoryCache memoryCache
        )
        {
            this._queue = queue;
            this._groupAttemptService = groupAttemptService;
            this._hubContext = hubContext;
            this._serviceProvider = serviceProvider;
            this._memoryCache = memoryCache;
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
                    competition.EndTime
                );

                this._memoryCache.Set(CompetitionCacheKey, competition, cacheEntryOptions);
            }

            return competition;
        }

        /// <summary>
        /// Executes the background task, processing items from the queue until cancellation is requested.
        /// </summary>
        /// <remarks>This method continuously dequeues items from the queue and processes them within a
        /// scoped service provider context. It runs until the provided <paramref name="cancellationToken"/> is
        /// triggered, at which point the method exits gracefully.</remarks>
        /// <param name="cancellationToken">A token that signals the request to cancel the operation. The method will stop processing when the token is
        /// triggered.</param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var queueItem = await this._queue.DequeueAsync(cancellationToken);

                using (var scope = this._serviceProvider.CreateScope())
                {
                    await this.ProcessExerciseAttempt(scope, queueItem);
                }
            }
        }

        /// <summary>
        /// Processes an exercise attempt submitted by a user and sends the result to relevant clients.
        /// </summary>
        /// <remarks>This method retrieves the current competition context, processes the exercise
        /// submission using the group attempt service, and sends the resulting response to multiple client groups,
        /// including administrators, teachers, and the submitting client.</remarks>
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

            ExerciseSubmissionResponse response =
                await this._groupAttemptService.SubmitExerciseAttempt(
                    currentCompetition,
                    queueItem.Request
                );

            await this
                ._hubContext.Clients.Group("Admins")
                .SendAsync("ReceiveExerciseAttempt", response);
            await this
                ._hubContext.Clients.Group("Teachers")
                .SendAsync("ReceiveExerciseAttempt", response);
            await this
                ._hubContext.Clients.Client(queueItem.ConnectionId)
                .SendAsync("ReceiveExerciseAttemptResponse", response);
        }
    }
}
