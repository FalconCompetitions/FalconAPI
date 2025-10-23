using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ProjetoTccBackend.Models
{
    public class User : IdentityUser
    {
        [JsonIgnore]
        public override string? PasswordHash { get => base.PasswordHash; set => base.PasswordHash = value; }

        [JsonIgnore]
        public override string? NormalizedEmail { get => base.NormalizedEmail; set => base.NormalizedEmail = value; }

        [JsonIgnore]
        public override string? NormalizedUserName { get => base.NormalizedUserName; set => base.NormalizedUserName = value; }

        [JsonIgnore]
        public override string? ConcurrencyStamp { get => base.ConcurrencyStamp; set => base.ConcurrencyStamp = value; }

        [JsonIgnore]
        public override bool LockoutEnabled { get => base.LockoutEnabled; set => base.LockoutEnabled = value; }

        [JsonIgnore]
        public override bool EmailConfirmed { get => base.EmailConfirmed; set => base.EmailConfirmed = value; }

        [JsonIgnore]
        public override string? SecurityStamp { get => base.SecurityStamp; set => base.SecurityStamp = value; }

        [JsonIgnore]
        public override string? UserName { get => base.UserName; set => base.UserName = value; }

        [JsonIgnore]
        public override int AccessFailedCount { get => base.AccessFailedCount; set => base.AccessFailedCount = value; }

        [JsonIgnore]
        public override bool TwoFactorEnabled { get => base.TwoFactorEnabled; set => base.TwoFactorEnabled = value; }

        [JsonIgnore]
        public override DateTimeOffset? LockoutEnd { get => base.LockoutEnd; set => base.LockoutEnd = value; }

        [StringLength(255)]
        public string Name { get; set; }
        public int? JoinYear { get; set; }

        public string? Department { get; set; }

        [StringLength(15)]
        public string RA { get; set; }

        public int? GroupId { get; set; }
        public virtual Group? Group { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastLoggedAt { get; set; }


        public ICollection<GroupInvite> GroupInvites { get; } = [];

        [JsonIgnore]
        public ICollection<Question> Questions { get; } = [];

        [JsonIgnore]
        public ICollection<Answer> Answers { get; } = [];

        [JsonIgnore]
        public ICollection<Log> Logs = [];
    }
}
