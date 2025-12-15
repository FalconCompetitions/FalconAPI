using System.ComponentModel.DataAnnotations;

namespace ProjetoTccBackend.Models
{
    /// <summary>
    /// Represents an invitation to join a group.
    /// </summary>
    public class GroupInvite
    {
        /// <summary>
        /// Gets or sets the unique identifier of the group invite.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the group the invitation is for.
        /// </summary>
        [Required]
        public int GroupId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the user being invited.
        /// </summary>
        [Required]
        public string UserId { get; set; }

        /// <summary>
        /// Gets the user being invited.
        /// </summary>
        public User User { get; }

        /// <summary>
        /// Gets the group the invitation is for.
        /// </summary>
        public Group Group { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the invitation has been accepted.
        /// </summary>
        [Required]
        public bool Accepted { get; set; }
    }
}
