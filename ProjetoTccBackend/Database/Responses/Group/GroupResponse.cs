using System.Text.Json.Serialization;
using ProjetoTccBackend.Database.Responses.Competition;
using ProjetoTccBackend.Database.Responses.User;

namespace ProjetoTccBackend.Database.Responses.Group
{
    public class GroupResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("leaderId")]
        public string LeaderId { get; set; }

        [JsonPropertyName("groupInvitations")]
        public ICollection<GroupInvitationResponse> GroupInvitations { get; set; }

        [JsonPropertyName("users")]
        public ICollection<GenericUserInfoResponse> Users { get; set; } = [];

        [JsonPropertyName("lastCompetitionDate")]
        public DateTime? LastCompetitionDate { get; set; }
    }
}
