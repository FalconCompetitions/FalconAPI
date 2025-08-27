using ProjetoTccBackend.Validation;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ProjetoTccBackend.Database.Requests.Auth
{
    /// <summary>
    /// Represents a request to register a new user in the system.
    /// </summary>
    public class RegisterUserRequest
    {
        /// <summary>
        /// Gets or sets the RA (student/employee registration number).
        /// </summary>
        [Required]
        [MaxLength(6, ErrorMessage = "RA inválido!")]
        [MinLength(7, ErrorMessage = "RA inválido!")]
        [JsonPropertyName("ra")]
        public string RA { get; set; }

        /// <summary>
        /// Gets or sets the user's full name.
        /// </summary>
        [Required]
        [MaxLength(90, ErrorMessage = "Nome maior que 90 caracteres")]
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the user's email address.
        /// </summary>
        [Required]
        [EmailAddress(ErrorMessage = "Email inválido!")]
        [JsonPropertyName("email")]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the user's role (e.g., Student, Teacher).
        /// </summary>
        [JsonPropertyName("role")]
        public required string Role { get; set; }

        /// <summary>
        /// Gets or sets the access code, required if the user is a Teacher.
        /// </summary>
        [JsonPropertyName("accessCode")]
        [RequiredIfAttribute(nameof(Role), "Teacher", ErrorMessage = "Código de acesso é obrigatório")]
        public string? AccessCode { get; set; }

        /// <summary>
        /// Gets or sets the year the student joined the course, required if the user is a Student.
        /// </summary>
        [JsonPropertyName("joinYear")]
        [RequiredIfAttribute(nameof(Role), "Student", ErrorMessage = "Ano de inscrição do curso é obrigatório")]
        public int? JoinYear { get; set; }

        /// <summary>
        /// Gets or sets the user's password.
        /// </summary>
        [Required]
        [JsonPropertyName("password")]
        public required string Password { get; set; }
    }
}
