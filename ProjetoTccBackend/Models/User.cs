using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ProjetoTccBackend.Models
{
    /// <summary>
    /// Represents a user in the system, extending the Identity framework's user class.
    /// </summary>
    public class User : IdentityUser
    {
        /// <summary>
        /// Gets or sets the password hash for the user. This property is hidden from JSON serialization.
        /// </summary>
        [JsonIgnore]
        public override string? PasswordHash { get => base.PasswordHash; set => base.PasswordHash = value; }

        /// <summary>
        /// Gets or sets the normalized email for the user. This property is hidden from JSON serialization.
        /// </summary>
        [JsonIgnore]
        public override string? NormalizedEmail { get => base.NormalizedEmail; set => base.NormalizedEmail = value; }

        /// <summary>
        /// Gets or sets the normalized username for the user. This property is hidden from JSON serialization.
        /// </summary>
        [JsonIgnore]
        public override string? NormalizedUserName { get => base.NormalizedUserName; set => base.NormalizedUserName = value; }

        /// <summary>
        /// Gets or sets the concurrency stamp for the user. This property is hidden from JSON serialization.
        /// </summary>
        [JsonIgnore]
        public override string? ConcurrencyStamp { get => base.ConcurrencyStamp; set => base.ConcurrencyStamp = value; }

        /// <summary>
        /// Gets or sets a value indicating whether lockout is enabled for the user. This property is hidden from JSON serialization.
        /// </summary>
        [JsonIgnore]
        public override bool LockoutEnabled { get => base.LockoutEnabled; set => base.LockoutEnabled = value; }

        /// <summary>
        /// Gets or sets a value indicating whether the user's email is confirmed. This property is hidden from JSON serialization.
        /// </summary>
        [JsonIgnore]
        public override bool EmailConfirmed { get => base.EmailConfirmed; set => base.EmailConfirmed = value; }

        /// <summary>
        /// Gets or sets the security stamp for the user. This property is hidden from JSON serialization.
        /// </summary>
        [JsonIgnore]
        public override string? SecurityStamp { get => base.SecurityStamp; set => base.SecurityStamp = value; }

        /// <summary>
        /// Gets or sets the username for the user. This property is hidden from JSON serialization.
        /// </summary>
        [JsonIgnore]
        public override string? UserName { get => base.UserName; set => base.UserName = value; }

        /// <summary>
        /// Gets or sets the number of failed access attempts for the user. This property is hidden from JSON serialization.
        /// </summary>
        [JsonIgnore]
        public override int AccessFailedCount { get => base.AccessFailedCount; set => base.AccessFailedCount = value; }

        /// <summary>
        /// Gets or sets a value indicating whether two-factor authentication is enabled. This property is hidden from JSON serialization.
        /// </summary>
        [JsonIgnore]
        public override bool TwoFactorEnabled { get => base.TwoFactorEnabled; set => base.TwoFactorEnabled = value; }

        /// <summary>
        /// Gets or sets the date and time when the user's lockout ends. This property is hidden from JSON serialization.
        /// </summary>
        [JsonIgnore]
        public override DateTimeOffset? LockoutEnd { get => base.LockoutEnd; set => base.LockoutEnd = value; }

        /// <summary>
        /// Gets or sets the full name of the user.
        /// </summary>
        [StringLength(255)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the year the user joined the institution.
        /// </summary>
        public int? JoinYear { get; set; }

        /// <summary>
        /// Gets or sets the department the user belongs to.
        /// </summary>
        public string? Department { get; set; }

        /// <summary>
        /// Gets or sets the academic registration number (RA) of the user.
        /// </summary>
        [StringLength(15)]
        public string RA { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the group the user belongs to.
        /// </summary>
        public int? GroupId { get; set; }

        /// <summary>
        /// Gets or sets the group the user belongs to.
        /// </summary>
        public virtual Group? Group { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the user was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the date and time of the user's last login.
        /// </summary>
        public DateTime? LastLoggedAt { get; set; }

        /// <summary>
        /// Gets the collection of group invites sent to the user.
        /// </summary>
        public ICollection<GroupInvite> GroupInvites { get; } = [];

        /// <summary>
        /// Gets the collection of questions created by the user. This property is hidden from JSON serialization.
        /// </summary>
        [JsonIgnore]
        public ICollection<Question> Questions { get; } = [];

        /// <summary>
        /// Gets the collection of answers provided by the user. This property is hidden from JSON serialization.
        /// </summary>
        [JsonIgnore]
        public ICollection<Answer> Answers { get; } = [];

        /// <summary>
        /// Gets the collection of logs associated with the user. This property is hidden from JSON serialization.
        /// </summary>
        [JsonIgnore]
        public ICollection<Log> Logs = [];
    }
}
