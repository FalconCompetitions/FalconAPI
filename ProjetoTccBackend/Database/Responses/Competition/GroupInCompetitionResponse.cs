using System.Text.Json.Serialization;
using ProjetoTccBackend.Database.Responses.Group;

namespace ProjetoTccBackend.Database.Responses.Competition
{
    /// <summary>
    /// Response model for group registration in a competition.
    /// </summary>
    public class GroupInCompetitionResponse
    {
        /// <summary>
        /// Identifier of the group participating in the competition.
        /// </summary>
        [JsonPropertyName("groupId")]
        public int GroupId { get; set; }

        /// <summary>
        /// Identifier of the competition in which the group is participating.
        /// </summary>
        [JsonPropertyName("competitionId")]
        public int CompetitionId { get; set; }

        /// <summary>
        /// The date and time when the group was added to the competition.
        /// </summary>
        [JsonPropertyName("createdOn")]
        public DateTime CreatedOn { get; set; }

        /// <summary>
        /// Indicates whether the group is blocked from participating in the competition.
        /// </summary>
        [JsonPropertyName("blocked")]
        public bool Blocked { get; set; }

        /// <summary>
        /// Reference to the group entity.
        /// </summary>
        [JsonPropertyName("group")]
        public GroupResponse? Group { get; set; }

        /// <summary>
        /// Reference to the competition entity.
        /// </summary>
        [JsonPropertyName("competition")]
        public CompetitionResponse? Competition { get; set; }
    }
}
