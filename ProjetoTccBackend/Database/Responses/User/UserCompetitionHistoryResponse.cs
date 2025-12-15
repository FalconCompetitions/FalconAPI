using System.Text.Json.Serialization;

namespace ProjetoTccBackend.Database.Responses.User
{
    /// <summary>
    /// Represents the competition history for a user.
    /// </summary>
    public class UserCompetitionHistoryResponse
    {
        /// <summary>
        /// The year of the competition.
        /// </summary>
        [JsonPropertyName("year")]
        public int Year { get; set; }

        /// <summary>
        /// The name of the group that participated.
        /// </summary>
        [JsonPropertyName("groupName")]
        public string GroupName { get; set; }

        /// <summary>
        /// The number of questions solved in the format "solved/total".
        /// </summary>
        [JsonPropertyName("questions")]
        public string Questions { get; set; }

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
    }
}
