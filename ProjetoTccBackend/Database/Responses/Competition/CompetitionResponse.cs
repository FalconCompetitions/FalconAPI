using ProjetoTccBackend.Database.Requests.Competition;
using ProjetoTccBackend.Database.Responses.Exercise;
using ProjetoTccBackend.Database.Responses.Group;
using ProjetoTccBackend.Enums.Competition;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ProjetoTccBackend.Database.Responses.Competition
{
    public class CompetitionResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [Required]
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }


        [JsonPropertyName("maxMembers")]
        public int MaxMembers { get; set; }

        [JsonPropertyName("startTime")]
        public DateTime? StartTime { get; set; }

        [JsonPropertyName("endTime")]
        public DateTime? EndTime { get; set; }


        public DateTime? StartInscriptions { get; set; }

        public DateTime? EndInscriptions { get; set; }

        [Required]
        [JsonPropertyName("duration")]
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// The date and time when the ranking will be stopped.
        /// </summary>
        [JsonPropertyName("stopRanking")]
        public DateTime? StopRanking { get; set; }

        /// <summary>
        /// The date and time after which submissions are blocked.
        /// </summary>
        [JsonPropertyName("blockSubmissions")]
        public DateTime? BlockSubmissions { get; set; }

        /// <summary>
        /// The penalty of the submission if rejected.
        /// </summary>
        [JsonPropertyName("submissionPenalty")]
        public TimeSpan? SubmissionPenalty { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of exercises allowed.
        /// </summary>
        [JsonPropertyName("maxExercises")]
        public int? MaxExercises { get; set; }

        /// <summary>
        /// Gets or sets the current status of the competition.
        /// </summary>
        /// <remarks>This property is required and must be set to a valid <see cref="CompetitionStatus"/>
        /// value.</remarks>
        [Required]
        [JsonPropertyName("status")]
        public CompetitionStatus Status { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the logged-in group is inscribed in the competition.
        /// </summary>
        [JsonPropertyName("isLoggedGroupInscribed")]
        public bool? IsLoggedGroupInscribed { get; set; }


        /// <summary>
        /// Gets or sets the maximum allowed size, in kb, for a submission.
        /// </summary>
        [JsonPropertyName("maxSubmissionSize")]
        public int? MaxSubmissionSize { get; set; }

        [Required]
        [JsonPropertyName("exerciseIds")]
        public ICollection<int> ExerciseIds { get; set; }

        [JsonPropertyName("exercises")]
        public ICollection<ExerciseResponse>? Exercises { get; set; }


        [JsonPropertyName("competitionRankings")]
        public ICollection<CompetitionRankingResponse>? CompetitionRankings { get; set; }
    }
}
