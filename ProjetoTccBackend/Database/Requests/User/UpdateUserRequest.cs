using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ProjetoTccBackend.Database.Requests.User
{
    public class UpdateUserRequest
    {
        [JsonPropertyName("name")]
        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(255, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 255 caracteres")]
        public string Name { get; set; }

        [JsonPropertyName("department")]
        [StringLength(200, ErrorMessage = "O departamento deve ter no máximo 200 caracteres")]
        public string? Department { get; set; }

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
