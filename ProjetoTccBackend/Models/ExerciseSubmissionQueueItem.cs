using ProjetoTccBackend.Database.Requests.Competition;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetoTccBackend.Models
{
    /// <summary>
    /// Represents an item in the exercise submission queue, containing the request details for a group exercise attempt
    /// and the associated connection identifier.
    /// </summary>
    /// <remarks>This class is used to encapsulate the details of a group exercise attempt request and the
    /// connection ID associated with the submission. It is typically used in scenarios where exercise submissions are
    /// queued for processing.</remarks>
    public class ExerciseSubmissionQueueItem
    {
        /// <summary>
        /// Gets or sets the unique identifier for the entity.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the request details for a group exercise attempt.
        /// </summary>
        public GroupExerciseAttemptWorkerRequest Request { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the connection.
        /// </summary>
        public string ConnectionId { get; set; }

        /// <summary>
        /// Represents an item in the exercise submission queue, containing the request details and the associated
        /// connection ID.
        /// </summary>
        /// <param name="groupExerciseAttemptRequest">The request object containing details about the group exercise attempt. This parameter cannot be null.</param>
        /// <param name="connectionId">The unique identifier for the connection associated with this submission. This parameter cannot be null or
        /// empty.</param>
        public ExerciseSubmissionQueueItem(GroupExerciseAttemptWorkerRequest groupExerciseAttemptRequest, string connectionId)
        {
            this.Request = groupExerciseAttemptRequest;
            this.ConnectionId = connectionId;
        }

        /// <summary>
        /// Represents an item in the exercise submission queue, containing details about a group exercise attempt
        /// request and its associated connection.
        /// </summary>
        /// <param name="id">The unique identifier for the queue item.</param>
        /// <param name="groupExerciseAttemptRequest">The request object containing details about the group exercise attempt. Cannot be null.</param>
        /// <param name="connectionId">The connection identifier associated with the request. Cannot be null or empty.</param>
        public ExerciseSubmissionQueueItem(Guid id, GroupExerciseAttemptWorkerRequest groupExerciseAttemptRequest, string connectionId)
        {
            this.Id = id;
            this.Request = groupExerciseAttemptRequest;
            this.ConnectionId = connectionId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExerciseSubmissionQueueItem"/> class.
        /// </summary>
        public ExerciseSubmissionQueueItem()
        {

        }
    }
}
