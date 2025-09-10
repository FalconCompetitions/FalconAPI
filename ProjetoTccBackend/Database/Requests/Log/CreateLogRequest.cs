using ProjetoTccBackend.Enums.Log;
using System.ComponentModel.DataAnnotations;

namespace ProjetoTccBackend.Database.Requests.Log
{
    public class CreateLogRequest
    {
        [Required]
        public LogType ActionType { get; set; }
        [Required]
        public string IpAddress { get; set; }
        public string? UserId { get; set; }
        public int? GroupId { get; set; }
        public int? CompetitionId { get; set; }
    }
}
