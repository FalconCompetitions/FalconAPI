using System.Text.Json.Serialization;

namespace ProjetoTccBackend.Database.Requests.Competition
{
    /// <summary>
    /// Request to answer a question in a competition.
    /// </summary>
    public class AnswerGroupQuestionRequest
    {
        /// <summary>
        /// Identifier of the question to answer.
        /// </summary>
        [JsonPropertyName("questionId")]
        public int QuestionId { get; set; }

        /// <summary>
        /// The answer content.
        /// </summary>
        [JsonPropertyName("answer")]
        public required string Answer { get; set; }
    }
}
