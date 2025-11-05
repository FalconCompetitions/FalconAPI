using ProjetoTccBackend.Database.Responses.User;
using ProjetoTccBackend.Enums.Question;

namespace ProjetoTccBackend.Database.Responses.Competition
{
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

        public AnswerResponse? Answer { get; set; } = null;

        /// <summary>
        /// Reference to the user who created the question.
        /// </summary>
        public GenericUserInfoResponse User { get; set; }

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
