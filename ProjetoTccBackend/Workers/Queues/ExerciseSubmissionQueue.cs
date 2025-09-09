using System.Threading.Channels;
using ProjetoTccBackend.Database.Requests.Competition;

namespace ProjetoTccBackend.Workers.Queues
{
    /// <summary>
    /// Represents a queue for managing group exercise attempt requests in a thread-safe manner.
    /// </summary>
    /// <remarks>This class provides asynchronous methods to enqueue and dequeue <see
    /// cref="ExerciseSubmissionQueueItem"/> objects. It uses an unbounded channel internally to ensure thread-safe
    /// operations and efficient handling of concurrent requests.</remarks>
    public class ExerciseSubmissionQueue
    {
        private readonly Channel<ExerciseSubmissionQueueItem> _queue =
            Channel.CreateUnbounded<ExerciseSubmissionQueueItem>();

        /// <summary>
        /// Asynchronously enqueues a <see cref="GroupExerciseAttemptRequest"/> into the processing queue.
        /// </summary>
        /// <param name="request">The request to be added to the queue. This parameter cannot be <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous enqueue operation.</returns>
        public async Task EnqueueAsync(ExerciseSubmissionQueueItem request)
        {
            await this._queue.Writer.WriteAsync(request);
        }

        /// <summary>
        /// Asynchronously retrieves and removes the next item from the queue.
        /// </summary>
        /// <remarks>This method waits asynchronously until an item is available in the queue. If the
        /// queue is empty,  the operation will block until an item is added or the operation is canceled via the
        /// <paramref name="cancellationToken"/>.</remarks>
        /// <param name="cancellationToken">A token to monitor for cancellation requests. If the operation is canceled, the task will be canceled.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the next  <see
        /// cref="ExerciseSubmissionQueueItem"/> item from the queue.</returns>
        public async Task<ExerciseSubmissionQueueItem> DequeueAsync(
            CancellationToken cancellationToken
        )
        {
            return await this._queue.Reader.ReadAsync(cancellationToken);
        }


        /// <summary>
        /// Gets the current number of items in the queue.
        /// </summary>
        /// <returns>The total number of items currently available in the queue.</returns>
        public int GetQueueSize()
        {
            return this._queue.Reader.Count;
        }
    }
}
