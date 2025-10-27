using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ProjetoTccBackend.Database.Requests.Group
{
    public class UpdateGroupRequest
    {
        [Required]
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("membersToRemove")]
        public ICollection<string> MembersToRemove { get; set; }
    }
}
