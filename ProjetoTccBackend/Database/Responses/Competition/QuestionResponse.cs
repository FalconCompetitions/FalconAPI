using ProjetoTccBackend.Database.Responses.Group;
using ProjetoTccBackend.Database.Responses.User;
using ProjetoTccBackend.Enums.Question;

namespace ProjetoTccBackend.Database.Responses.Competition
{
    /// <summary>
    /// Response containing question information for a competition.
    /// </summary>
    public class QuestionResponse
    {
        /// <summary>
        /// Unique identifier of the question.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Identifier of the competition to which this question belongs.
        /// </summary>
        public int CompetitionId { get; set; }

        /// <summary>
        /// Answer to this question if it has been answered.
        /// </summary>
        public AnswerResponse? Answer { get; set; } = null;

        /// <summary>
        /// Reference to the user who created the question.
        /// </summary>
        public required GenericUserInfoResponse User { get; set; }

        /// <summary>
        /// Group information of the user who asked the question.
        /// </summary>
        public GroupResponse? Group { get; set; } = null;

        /// <summary>
        /// Textual content of the question.
        /// </summary>
        public required string Content { get; set; }

        /// <summary>
        /// Type of the question, indicating its nature.
        /// </summary>
        public QuestionType QuestionType { get; set; }
    }
}
