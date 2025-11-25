using ProjetoTccBackend.Enums.Log;
using System.ComponentModel.DataAnnotations;

namespace ProjetoTccBackend.Models
{
    /// <summary>
    /// Represents a log entry in the system for tracking user actions.
    /// </summary>
    public class Log
    {
        /// <summary>
        /// Gets or sets the unique identifier of the log entry.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the type of action logged.
        /// </summary>
        [Required]
        public LogType ActionType { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the action occurred.
        /// </summary>
        [Required]
        public DateTime ActionTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the IP address from which the action was performed.
        /// </summary>
        [Required]
        [StringLength(128)]
        public string IpAddress { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the user who performed the action.
        /// </summary>
        public string? UserId { get; set; } = null;

        /// <summary>
        /// Gets or sets the user who performed the action.
        /// </summary>
        public User? User { get; set; } = null;

        /// <summary>
        /// Gets or sets the identifier of the group associated with the action.
        /// </summary>
        public int? GroupId { get; set; } = null;

        /// <summary>
        /// Gets or sets the group associated with the action.
        /// </summary>
        public Group? Group { get; set; } = null;

        /// <summary>
        /// Gets or sets the identifier of the competition associated with the action.
        /// </summary>
        public int? CompetitionId { get; set; } = null;

        /// <summary>
        /// Gets or sets the competition associated with the action.
        /// </summary>
        public Competition? Competition { get; set; } = null;
    }
}
