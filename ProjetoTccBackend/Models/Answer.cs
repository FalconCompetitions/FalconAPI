using System.ComponentModel.DataAnnotations;

namespace ProjetoTccBackend.Models
{
    /// <summary>
    /// Represents an answer to a question in the system.
    /// </summary>
    public class Answer
    {
        /// <summary>
        /// Gets or sets the unique identifier of the answer.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the textual content of the answer.
        /// </summary>
        [DataType(DataType.MultilineText)]
        public required string Content { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the user who provided the answer.
        /// </summary>
        public required string UserId { get; set; }

        /// <summary>
        /// Gets or sets the user who provided the answer.
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// Gets or sets the question this answer is associated with.
        /// </summary>
        public Question Question { get; set; }
    }
}
