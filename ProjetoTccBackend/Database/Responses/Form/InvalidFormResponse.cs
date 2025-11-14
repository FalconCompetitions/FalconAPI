using System.Text.Json.Serialization;

namespace ProjetoTccBackend.Database.Responses.Form
{
    public class InvalidFormResponse
    {
        [JsonPropertyName("target")]
        public string Target { get; set; }

        [JsonPropertyName("error")]
        public string Error { get; set; }
    }
}
