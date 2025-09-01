using System.ComponentModel.DataAnnotations;

namespace ProjetoTccBackend.Database.Requests.User
{
    public class UpdateUserRequest
    {
        [Required]
        public string Name { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public int? JoinYear { get; set; }
    }
}
