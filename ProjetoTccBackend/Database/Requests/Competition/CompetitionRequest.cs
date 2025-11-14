using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ProjetoTccBackend.Database.Requests.Competition
{
    public class CompetitionRequest
    {
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
        [JsonPropertyName("startInscriptions")]
        public DateTime StartInscriptions { get; set; }

        [Required]
        [JsonPropertyName("endInscriptions")]
        public DateTime EndInscriptions { get; set; }


        [JsonPropertyName("duration")]
        public TimeSpan? Duration { get; set; }

        /// <summary>
        /// The date and time when the ranking will be stopped.
        /// </summary>
        [JsonPropertyName("stopRanking")]
        public TimeSpan? StopRanking { get; set; }

        /// <summary>
        /// The date and time after which submissions are blocked.
        /// </summary>
        [JsonPropertyName("blockSubmissions")]
        public TimeSpan? BlockSubmissions { get; set; }

        /// <summary>
        /// The penalty of the submission if rejected.
        /// </summary>
        [JsonPropertyName("submissionPenalty")]
        public TimeSpan SubmissionPenalty { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of exercises allowed.
        /// </summary>
        [JsonPropertyName("maxExercises")]
        public int? MaxExercises { get; set; }


        /// <summary>
        /// Gets or sets the maximum allowed size, in kb, for a submission.
        /// </summary>
        [Required]
        [JsonPropertyName("maxSubmissionSize")]
        public int MaxSubmissionSize { get; set; }

        [Required]
        [JsonPropertyName("exerciseIds")]
        public ICollection<int> ExerciseIds { get; set; }

    }
}
