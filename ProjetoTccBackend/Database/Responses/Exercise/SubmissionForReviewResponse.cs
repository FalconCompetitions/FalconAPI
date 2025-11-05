using ProjetoTccBackend.Database.Responses.Group;
using ProjetoTccBackend.Enums.Exercise;
using ProjetoTccBackend.Enums.Judge;
using System.Text.Json.Serialization;

namespace ProjetoTccBackend.Database.Responses.Exercise
{
    /// <summary>
    /// Response DTO for exercise submissions that need manual review.
    /// </summary>
    public class SubmissionForReviewResponse
    {
        /// <summary>
        /// Unique identifier for the submission.
        /// </summary>
        [JsonPropertyName("id")]
        public int Id { get; set; }

        /// <summary>
        /// Identifier of the exercise.
        /// </summary>
        [JsonPropertyName("exerciseId")]
        public int ExerciseId { get; set; }

        /// <summary>
        /// Name of the exercise.
        /// </summary>
        [JsonPropertyName("exerciseName")]
        public string? ExerciseName { get; set; }

        /// <summary>
        /// Identifier of the group that made the submission.
        /// </summary>
        [JsonPropertyName("groupId")]
        public int GroupId { get; set; }

        /// <summary>
        /// Group information.
        /// </summary>
        [JsonPropertyName("group")]
        public GroupResponse? Group { get; set; }

        /// <summary>
        /// Date and time when the attempt was submitted.
        /// </summary>
        [JsonPropertyName("submissionTime")]
        public DateTime SubmissionTime { get; set; }

        /// <summary>
        /// Programming language used for the submission.
        /// </summary>
        [JsonPropertyName("language")]
        public LanguageType Language { get; set; }

        /// <summary>
        /// Indicates whether the attempt was accepted (correct solution).
        /// </summary>
        [JsonPropertyName("accepted")]
        public bool Accepted { get; set; }

        /// <summary>
        /// The response from the judge system for the submission.
        /// </summary>
        [JsonPropertyName("judgeResponse")]
        public JudgeSubmissionResponse JudgeResponse { get; set; }

        /// <summary>
        /// The code submitted by the group.
        /// </summary>
        [JsonPropertyName("code")]
        public string? Code { get; set; }
    }
}
