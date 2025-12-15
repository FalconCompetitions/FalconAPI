using ProjetoTccBackend.Enums.Log;
using System.Text.Json.Serialization;

namespace ProjetoTccBackend.Database.Responses.Log
{
    /// <summary>
    /// Response DTO for log entries with enriched user and group information.
    /// </summary>
    public class LogResponse
    {
        /// <summary>
        /// Unique identifier of the log entry.
        /// </summary>
        [JsonPropertyName("id")]
        public int Id { get; set; }

        /// <summary>
        /// Type of action logged.
        /// </summary>
        [JsonPropertyName("actionType")]
        public LogType ActionType { get; set; }

        /// <summary>
        /// Human-readable description of the action.
        /// </summary>
        [JsonPropertyName("actionDescription")]
        public string ActionDescription { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp when the action occurred.
        /// </summary>
        [JsonPropertyName("actionTime")]
        public DateTime ActionTime { get; set; }

        /// <summary>
        /// IP address from which the action was performed.
        /// </summary>
        [JsonPropertyName("ipAddress")]
        public string IpAddress { get; set; } = string.Empty;

        /// <summary>
        /// User ID who performed the action.
        /// </summary>
        [JsonPropertyName("userId")]
        public string? UserId { get; set; }

        /// <summary>
        /// Name of the user who performed the action.
        /// </summary>
        [JsonPropertyName("userName")]
        public string? UserName { get; set; }

        /// <summary>
        /// Group ID associated with the action.
        /// </summary>
        [JsonPropertyName("groupId")]
        public int? GroupId { get; set; }

        /// <summary>
        /// Name of the group associated with the action.
        /// </summary>
        [JsonPropertyName("groupName")]
        public string? GroupName { get; set; }

        /// <summary>
        /// Competition ID associated with the action.
        /// </summary>
        [JsonPropertyName("competitionId")]
        public int? CompetitionId { get; set; }
    }
}
