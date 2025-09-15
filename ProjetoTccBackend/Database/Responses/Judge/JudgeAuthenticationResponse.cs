using System.Text.Json.Serialization;

namespace ProjetoTccBackend.Database.Responses.Judge
{
    public class JudgeAuthenticationResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }
    }
}
