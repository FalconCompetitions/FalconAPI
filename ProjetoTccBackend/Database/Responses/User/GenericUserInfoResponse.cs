using System.Text.Json.Serialization;

namespace ProjetoTccBackend.Database.Responses.User
{
    public class GenericUserInfoResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }


        [JsonPropertyName("joinYear")]
        public int JoinYear { get; set; }
    }
}
