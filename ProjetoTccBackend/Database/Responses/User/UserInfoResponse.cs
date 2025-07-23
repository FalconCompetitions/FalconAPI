using ProjetoTccBackend.Database.Responses.Auth;
using System.Text.Json.Serialization;

namespace ProjetoTccBackend.Database.Responses.User
{
    public class UserInfoResponse : UserResponse
    {
        [JsonPropertyName("ra")]
        public string RA { get; set; }

    }
}
