using System.Threading.Channels;
using ProjetoTccBackend.Database;
using ProjetoTccBackend.Database.Requests.Competition;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Repositories.Interfaces;

namespace ProjetoTccBackend.Workers.Queues
{
    /// <summary>
    /// Represents a thread-safe queue for managing group exercise attempt requests, with fail-safe persistence in the database.
    /// </summary>
    /// <remarks>
    /// This class provides asynchronous methods to enqueue and dequeue <see cref="ExerciseSubmissionQueueItem"/> objects. Each item is persisted in the database when enqueued and removed when dequeued, ensuring fail-safe processing even if the server restarts or crashes. An unbounded channel is used internally for efficient concurrent operations.
    /// </remarks>
    public class ExerciseSubmissionQueue
    {
        private readonly Channel<ExerciseSubmissionQueueItem> _queue =
            Channel.CreateUnbounded<ExerciseSubmissionQueueItem>();
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExerciseSubmissionQueue"/> class.
        /// </summary>
        /// <param name="serviceProvider">Service provider for creating scoped services.</param>
        public ExerciseSubmissionQueue(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Asynchronously enqueues an <see cref="ExerciseSubmissionQueueItem"/> into the processing queue and persists it in the database.
        /// </summary>
        /// <remarks>
        /// This method creates a new service scope, saves the item to the database for fail-safe persistence, and then writes the item to the in-memory channel for processing.
        /// </remarks>
        /// <param name="request">The queue item to be added. This parameter cannot be <see langword="null"/>.</param>
        /// <returns>A task representing the asynchronous enqueue operation.</returns>
        public async Task EnqueueAsync(ExerciseSubmissionQueueItem request)
        {
            using (var scope = this._serviceProvider.CreateScope())
            {
                TccDbContext dbContext = scope.ServiceProvider.GetRequiredService<TccDbContext>();
                IExerciseSubmissionQueueItemRepository submissionRepository =
                    scope.ServiceProvider.GetRequiredService<IExerciseSubmissionQueueItemRepository>();

                submissionRepository.Add(request);
                await dbContext.SaveChangesAsync();
            }

            await this._queue.Writer.WriteAsync(request);
        }

        /// <summary>
        /// Asynchronously retrieves and removes the next item from the queue, also removing it from the database.
        /// </summary>
        /// <remarks>
        /// This method waits asynchronously until an item is available in the queue. After reading the item, it creates a new service scope and removes the item from the database to ensure it is not processed again.
        /// </remarks>
        /// <param name="cancellationToken">A token to monitor for cancellation requests. If the operation is canceled, the task will be canceled.</param>
        /// <returns>A task representing the asynchronous operation. The result contains the next <see cref="ExerciseSubmissionQueueItem"/> from the queue.</returns>
        public async Task<ExerciseSubmissionQueueItem> DequeueAsync(
            CancellationToken cancellationToken
        )
        {
            ExerciseSubmissionQueueItem queueItem = await this._queue.Reader.ReadAsync(
                cancellationToken
            );

            using (var scope = this._serviceProvider.CreateScope())
            {
                TccDbContext dbContext = scope.ServiceProvider.GetRequiredService<TccDbContext>();
                IExerciseSubmissionQueueItemRepository submissionRepository =
                    scope.ServiceProvider.GetRequiredService<IExerciseSubmissionQueueItemRepository>();

                submissionRepository.Remove(queueItem);
                await dbContext.SaveChangesAsync();
            }

            return queueItem;
        }

        /// <summary>
        /// Gets the current number of items available in the queue.
        /// </summary>
        /// <remarks>
        /// Returns the total number of items currently present in the in-memory channel. This does not include items that may be persisted in the database but not yet loaded into the channel.
        /// </remarks>
        /// <returns>The total number of items currently available in the queue.</returns>
        public int GetQueueSize()
        {
            return this._queue.Reader.Count;
        }
    }
}
