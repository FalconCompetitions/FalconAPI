namespace ProjetoTccBackend.Models
{
    /// <summary>
    /// Represents the association between a group and a competition,
    /// including the date and time when the group was added to the competition.
    /// </summary>
    public class GroupInCompetition
    {
        /// <summary>
        /// Identifier of the group participating in the competition.
        /// </summary>
        public int GroupId { get; set; }

        /// <summary>
        /// Identifier of the competition in which the group is participating.
        /// </summary>
        public int CompetitionId { get; set; }

        /// <summary>
        /// The date and time when the group was added to the competition.
        /// </summary>
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Reference to the group entity.
        /// </summary>
        public Group Group { get; set; }

        /// <summary>
        /// Reference to the competition entity.
        /// </summary>
        public Competition Competition { get; set; }
    }
}
