using ProjetoTccBackend.Database.Responses.Group;
using System.Text.Json.Serialization;

namespace ProjetoTccBackend.Database.Responses.User
{
    public class GenericUserInfoResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }


        [JsonPropertyName("ra")]
        public string Ra { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("group")]
        public GroupResponse Group { get; set; }

        [JsonPropertyName("exercisesCreated")]
        public int? ExercisesCreated { get; set; }

        [JsonPropertyName("joinYear")]
        public int? JoinYear { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("lastLoggedAt")]
        public DateTime? LastLoggedAt { get; set; }
    }
}
