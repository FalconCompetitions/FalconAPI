using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProjetoTccBackend.Database.Requests.Group
{
    public class UpdateGroupRequest
    {
        [Required]
        public string Name { get; set; }
        public List<string> UserIds { get; set; } = new();
    }
}
