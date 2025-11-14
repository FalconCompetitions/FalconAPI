using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ProjetoTccBackend.Database.Requests.Competition
{
    public class CreateCompetitionSubscription
    {
        /// <summary>
        /// Gets or sets the date and time when inscriptions start.
        /// </summary>
        [Required]
        [JsonPropertyName("startInscriptions")]
        public DateTime StartInscriptions { get; set; }

        /// <summary>
        /// Gets or sets the date and time when inscriptions end.
        /// </summary>
        [Required]
        [JsonPropertyName("endInscriptions")]
        public DateTime EndInscriptions { get; set; }
    }
}
