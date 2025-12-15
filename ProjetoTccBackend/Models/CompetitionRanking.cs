using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetoTccBackend.Models
{
    /// <summary>
    /// Represents the ranking information of a group within a specific competition,
    /// including points earned and the order in the ranking.
    /// </summary>
    public class CompetitionRanking
    {
        /// <summary>
        /// Unique identifier for the competition ranking entry.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Identifier of the competition to which this ranking belongs.
        /// </summary>
        [Required]
        public int CompetitionId { get; set; }

        /// <summary>
        /// Reference to the related competition.
        /// </summary>
        public Competition Competition { get; set; }

        /// <summary>
        /// Identifier of the group associated with this ranking.
        /// </summary>
        [Required]
        public int GroupId { get; set; }

        /// <summary>
        /// Reference to the related group.
        /// </summary>
        public Group Group { get; set; }

        /// <summary>
        /// Total points earned by the group in the competition.
        /// </summary>
        [DefaultValue(0)]
        public double Points { get; set; }

        /// <summary>
        /// Penalty points applied to the group's points in the competition.
        /// </summary>
        [DefaultValue(0)]
        public double Penalty { get; set; } = 0;

        /// <summary>
        /// The order or position of the group in the competition ranking.
        /// </summary>
        public int RankOrder { get; set; }
    }
}
