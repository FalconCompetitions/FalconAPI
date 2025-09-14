using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetoTccBackend.Models
{
    public class User : IdentityUser
    {
        [StringLength(255)]
        public string Name { get; set; }
        public int? JoinYear { get; set; }

        [StringLength(15)]
        public string RA { get; set; }

        public int? GroupId { get; set; }
        public Group? Group { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastLoggedAt { get; set; }

        public ICollection<Question> Questions { get; } = [];
        public ICollection<Answer> Answers { get; } = [];

        public ICollection<Log> Logs = [];
    }
}
