using ProjetoTccBackend.Enums.Question;
using ProjetoTccBackend.Models;
using System.ComponentModel.DataAnnotations;

namespace ProjetoTccBackend.Database.Requests.Competition
{
    public class CreateGroupQuestionRequest
    {
        /// <summary>  
        /// Target question identifier, if any.  
        /// </summary>  
        public int? TargetQuestionId { get; set; } = null;

        /// <summary>  
        /// Identifier of the exercise associated with the question, if any.  
        /// </summary>  
        public int? ExerciseId { get; set; } = null;

        /// <summary>  
        /// Textual content of the question.  
        /// </summary>  
        [DataType(DataType.MultilineText)]
        public required string Content { get; set; }

        /// <summary>  
        /// Type of the question, indicating its nature.  
        /// </summary>  
        public QuestionType QuestionType { get; set; }
    }
}
