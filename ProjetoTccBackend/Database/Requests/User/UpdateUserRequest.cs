using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ProjetoTccBackend.Database.Requests.User
{
    public class UpdateUserRequest
    {
        [JsonPropertyName("name")]
        [Required]
        public string Name { get; set; }
        
        [JsonPropertyName("department")]
        public string Department { get; set; }

        [Required]
        [EmailAddress]
        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("phoneNumber")]
        public string? PhoneNumber { get; set; }

        [JsonPropertyName("joinYear")]
        public int? JoinYear { get; set; }
    }
}
