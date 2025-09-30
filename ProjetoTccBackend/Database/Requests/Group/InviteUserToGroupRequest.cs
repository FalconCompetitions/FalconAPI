using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ProjetoTccBackend.Database.Requests.Group
{
    public class InviteUserToGroupRequest
    {
        [Required]
        [JsonPropertyName("groupId")]
        public int GroupId { get; set; }

        [Required]
        [JsonPropertyName("userId")]
        public string UserId { get; set; }
    }
}
