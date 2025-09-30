using System.ComponentModel.DataAnnotations;

namespace ProjetoTccBackend.Models
{
    public class GroupInvite
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int GroupId { get; set; }

        [Required]
        [StringLength(36, MinimumLength = 36)]
        public string UserId { get; set; }

        public User User { get; }

        public Group Group { get; }

        [Required]
        public bool Accepted { get; set; }
    }
}
