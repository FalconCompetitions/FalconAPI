using ProjetoTccBackend.Database.Responses.Competition;
using ProjetoTccBackend.Database.Responses.Group;
using System.Text.Json.Serialization;

namespace ProjetoTccBackend.Database.Responses.Auth
{
    public class UserResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("ra")]
        public string RA { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("emailConfirmed")]
        public bool EmailConfirmed { get; set; }

        [JsonPropertyName("joinYear")]
        public int? JoinYear { get; set; }

        [JsonPropertyName("phoneNumber")]
        public string? PhoneNumber { get; set; }

        [JsonPropertyName("phoneNumberConfirmed")]
        public bool PhoneNumberConfirmed { get; set; }

        [JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonPropertyName("group")]
        public GroupResponse? Group { get; set; } = null;

        [JsonPropertyName("groupInvitations")]
        public ICollection<GroupInvitationResponse> GroupInvitations { get; set; }
    }
}
