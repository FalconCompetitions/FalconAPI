using System.Text.Json.Serialization;
using ProjetoTccBackend.Database.Responses.Group;

namespace ProjetoTccBackend.Database.Responses.Competition
{
    /// <summary>
    /// Represents the ranking information of a group in a competition.
    /// </summary>
    public class CompetitionRankingResponse
    {
        /// <summary>
        /// Unique identifier for the competition ranking entry.
        /// </summary>
        [JsonPropertyName("id")]
        public int Id { get; set; }

        /// <summary>
        /// Reference to the related group.
        /// </summary>
        [JsonPropertyName("group")]
        public GroupResponse Group { get; set; }

        /// <summary>
        /// Total points earned by the group in the competition.
        /// </summary>
        [JsonPropertyName("points")]
        public double Points { get; set; }

        /// <summary>
        /// Penalty points applied to the group's points in the competition.
        /// </summary>
        [JsonPropertyName("penalty")]
        public double Penalty { get; set; } = 0;

        /// <summary>
        /// The order or position of the group in the competition ranking.
        /// </summary>
        [JsonPropertyName("rankOrder")]
        public int RankOrder { get; set; }

        /// <summary>
        /// Collection of exercise attempts made by the group in the competition.
        /// </summary>
        [JsonPropertyName("exerciseAttempts")]
        public ICollection<GroupExerciseAttemptResponse> ExerciseAttempts { get; set; }
    }
}
