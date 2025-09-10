using ProjetoTccBackend.Enums.Log;
using System.ComponentModel.DataAnnotations;

namespace ProjetoTccBackend.Models
{
    public class Log
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public LogType ActionType { get; set; }

        [Required]
        public DateTime ActionTime { get; set; } = DateTime.Now;

        [Required]
        [StringLength(128)]
        public string IpAddress { get; set; }

        public string? UserId { get; set; } = null;

        public User? User { get; set; } = null;

        public int? GroupId { get; set; } = null;
        public Group? Group { get; set; } = null;

        public int? CompetitionId { get; set; } = null;

        public Competition? Competition { get; set; } = null;
    }
}
