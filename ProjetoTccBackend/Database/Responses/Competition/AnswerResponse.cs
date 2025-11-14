using ProjetoTccBackend.Database.Responses.User;

namespace ProjetoTccBackend.Database.Responses.Competition
{
    /// <summary>
    /// Response containing answer information for a question.
    /// </summary>
    public class AnswerResponse
    {
        /// <summary>
        /// Unique identifier for the answer.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Textual content of the answer.
        /// </summary>
        public required string Content { get; set; }

        /// <summary>
        /// Reference to the user who created the answer.
        /// </summary>
        public required GenericUserInfoResponse User { get; set; }

        /// <summary>
        /// Identifier of the question being answered (optional for reference).
        /// </summary>
        public int? QuestionId { get; set; }
    }
}
