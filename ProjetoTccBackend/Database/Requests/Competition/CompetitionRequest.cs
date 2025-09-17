using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ProjetoTccBackend.Database.Requests.Competition
{
    public class CompetitionRequest
    {
        [Required]
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [Required]
        [JsonPropertyName("startTime")]
        public DateTime StartTime { get; set; }


        [Required]
        [JsonPropertyName("duration")]
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// The date and time when the ranking will be stopped.
        /// </summary>
        [Required]
        [JsonPropertyName("stopRanking")]
        public TimeSpan StopRanking { get; set; }

        /// <summary>
        /// The date and time after which submissions are blocked.
        /// </summary>
        [JsonPropertyName("blockSubmissions")]
        public TimeSpan BlockSubmissions { get; set; }

        /// <summary>
        /// The penalty of the submission if rejected.
        /// </summary>
        [Required]
        [JsonPropertyName("submissionPenalty")]
        public TimeSpan SubmissionPenalty { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of exercises allowed.
        /// </summary>
        [Required]
        [JsonPropertyName("maxExercises")]
        public int MaxExercises { get; set; }


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
