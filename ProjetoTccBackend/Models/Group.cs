using System.ComponentModel.DataAnnotations;

namespace ProjetoTccBackend.Models
{
    /// <summary>
    /// Represents a group in the system.
    /// </summary>
    public class Group
    {
        /// <summary>
        /// Gets or sets the unique identifier of the group.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the group.
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the leader.
        /// </summary>
        [Required]
        public string LeaderId { get; set; }

        /// <summary>
        /// Gets or sets the collection of users associated with the group.
        /// </summary>
        public virtual ICollection<User> Users { get; set; }

        /// <summary>
        /// Gets or sets the collection of competitions associated with the group.
        /// </summary>
        public virtual ICollection<Competition> Competitions { get; set; }

        /// <summary>
        /// Gets the collection of group-competition associations.
        /// </summary>
        public virtual ICollection<GroupInCompetition> GroupInCompetitions { get; } = [];

        /// <summary>
        /// Gets the collection of competition rankings associated with the group.
        /// </summary>
        public virtual ICollection<CompetitionRanking> CompetitionRankings { get; } = [];

        /// <summary>
        /// Gets or sets the collection of exercise attempts made by the group.
        /// </summary>
        public virtual ICollection<GroupExerciseAttempt> GroupExerciseAttempts { get; set; } = [];

        /// <summary>
        /// Gets the collection of logs associated with the group.
        /// </summary>
        public virtual ICollection<Log> Logs { get; } = [];

        /// <summary>
        /// Gets or sets the collection of group invites associated with the group.
        /// </summary>
        public virtual ICollection<GroupInvite> GroupInvites { get; set; }

        /// <summary>
        /// Gets or sets the row version for optimistic concurrency control.
        /// </summary>
        [Timestamp]
        public byte[]? RowVersion { get; set; }
    }
}
