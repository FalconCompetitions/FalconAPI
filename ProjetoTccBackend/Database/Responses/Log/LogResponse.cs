using ProjetoTccBackend.Enums.Log;
using System.Text.Json.Serialization;

namespace ProjetoTccBackend.Database.Responses.Log
{
    public class LogResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("actionType")]
        public LogType ActionType { get; set; }
        [JsonPropertyName("actionTime")]
        public DateTime ActionTime { get; set; }
        [JsonPropertyName("ipAddress")]
        public string IpAddress { get; set; }
        [JsonPropertyName("userId")]
        public string? UserId { get; set; }
        [JsonPropertyName("groupId")]
        public int? GroupId { get; set; }
        [JsonPropertyName("competitionId")]
        public int? CompetitionId { get; set; }
    }
}
