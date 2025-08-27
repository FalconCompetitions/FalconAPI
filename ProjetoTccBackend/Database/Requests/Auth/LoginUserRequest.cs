using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ProjetoTccBackend.Database.Requests.Auth
{
    public class LoginUserRequest
    {
        [Required(ErrorMessage = "Campo obrigatório")]
        [MinLength(6, ErrorMessage = "RA deve ter no mínimo 6 digitos")]
        [MaxLength(7, ErrorMessage = "RA deve ter no máximo 7 digitos")]
        [JsonPropertyName("ra")]
        public required string Ra { get; set; }

        [Required(ErrorMessage = "Campo obrigatório")]
        [JsonPropertyName("password")]
        public required string Password { get; set; }
    }
}
