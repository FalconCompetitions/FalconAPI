using ProjetoTccBackend.Database.Responses.Group;

namespace ProjetoTccBackend.Database.Responses.Competition
{
    public class CompetitionRankingResponse
    {
        /// <summary>
        /// Unique identifier for the competition ranking entry.
        /// </summary>
        public int Id { get; set; }


        /// <summary>
        /// Reference to the related group.
        /// </summary>
        public GroupResponse Group { get; set; }

        /// <summary>
        /// Total points earned by the group in the competition.
        /// </summary>
        public double Points { get; set; }

        /// <summary>
        /// Penalty points applied to the group's points in the competition.
        /// </summary>
        public double Penalty { get; set; } = 0;

        /// <summary>
        /// The order or position of the group in the competition ranking.
        /// </summary>
        public int RankOrder { get; set; }
    }
}
