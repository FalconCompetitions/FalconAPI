using System.Text.Json.Serialization;

namespace ProjetoTccBackend.Database.Requests.Judge
{
    /// <summary>
    /// Represents a request to submit a solution for a problem to the judge system.
    /// </summary>
    public class JudgeSubmissionRequest
    {
        /// <summary>
        /// Gets or sets the identifier of the problem to be judged.
        /// </summary>
        [JsonPropertyName("problem_id")]
        public required string ProblemId { get; set; }

        /// <summary>
        /// Gets or sets the programming language type of the submitted solution.
        /// </summary>
        [JsonPropertyName("language_type")]
        public required string LanguageType { get; set; }

        /// <summary>
        /// Gets or sets the content of the solution to be judged.
        /// </summary>
        [JsonPropertyName("content")]
        public required string Content { get; set; }
    }
}
