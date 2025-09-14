using ProjetoTccBackend.Database.Responses.Auth;
using System.Text.Json.Serialization;

namespace ProjetoTccBackend.Database.Responses.User
{
    public class UserInfoResponse : UserResponse
    {
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("lastLoggedAt")]
        public DateTime? LastLoggedAt { get; set; }

    }
}
