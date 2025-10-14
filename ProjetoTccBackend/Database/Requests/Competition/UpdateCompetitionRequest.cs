using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ProjetoTccBackend.Database.Requests.Competition
{
    public class UpdateCompetitionRequest
    {
        [Required]
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [Required]
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Número de membros é obrigatório")]
        [JsonPropertyName("maxMembers")]
        public int MaxMembers { get; set; }

        [Required]
        [JsonPropertyName("startTime")]
        public DateTime StartTime { get; set; }

        [Required]
        [JsonPropertyName("duration")]
        public TimeSpan Duration { get; set; }

        [Required]
        [JsonPropertyName("stopRanking")]
        public TimeSpan StopRanking { get; set; }

        [JsonPropertyName("blockSubmissions")]
        public TimeSpan BlockSubmissions { get; set; }

        [Required]
        [JsonPropertyName("submissionPenalty")]
        public TimeSpan SubmissionPenalty { get; set; }

        [Required]
        [JsonPropertyName("maxExercises")]
        public int MaxExercises { get; set; }

        [Required]
        [JsonPropertyName("maxSubmissionSize")]
        public int MaxSubmissionSize { get; set; }

        [Required]
        [JsonPropertyName("exerciseIds")]
        public ICollection<int> ExerciseIds { get; set; }
    }
}
