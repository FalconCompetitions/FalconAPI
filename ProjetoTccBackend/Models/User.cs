using Microsoft.AspNetCore.Identity;

namespace ProjetoTccBackend.Models
{
    public class User : IdentityUser
    {
        public int? JoinYear { get; set; }
        public string RA { get; set; }

        public int? GroupId { get; set; }
        public Group? Group { get; set; }

        public ICollection<Question> Questions { get; } = [];

        public ICollection<Log> Logs = [];
    }
}
