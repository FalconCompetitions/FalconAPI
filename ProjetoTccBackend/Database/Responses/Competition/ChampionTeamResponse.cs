using System.Text.Json.Serialization;

namespace ProjetoTccBackend.Database.Responses.Competition
{
    /// <summary>
    /// Represents a champion team from a competition.
    /// </summary>
    public class ChampionTeamResponse
    {
        /// <summary>
        /// The year of the competition.
        /// </summary>
        [JsonPropertyName("year")]
        public int Year { get; set; }

        /// <summary>
        /// The name of the champion team.
        /// </summary>
        [JsonPropertyName("teamName")]
        public string TeamName { get; set; }

        /// <summary>
        /// The competition ID.
        /// </summary>
        [JsonPropertyName("competitionId")]
        public int CompetitionId { get; set; }

        /// <summary>
        /// The competition name.
        /// </summary>
        [JsonPropertyName("competitionName")]
        public string CompetitionName { get; set; }

        /// <summary>
        /// The total points scored by the team.
        /// </summary>
        [JsonPropertyName("points")]
        public double Points { get; set; }
    }
}
