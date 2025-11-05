using ProjetoTccBackend.Enums.Question;
using System.ComponentModel.DataAnnotations;

namespace ProjetoTccBackend.Models
{
    /// <summary>  
    /// Represents a question that can be associated with an exercise or another question.  
    /// </summary>  
    public class Question
    {
        /// <summary>  
        /// Unique identifier of the question.  
        /// </summary>  
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Identifier of the competition to which this question belongs.
        /// </summary>
        public required int CompetitionId { get; set; }

        /// <summary>
        /// Reference to the competition to which this question belongs.
        /// </summary>
        public Competition Competition { get; }

        public int? AnswerId { get; set; } = null;
        public Answer? Answer { get; set; } = null;

        /// <summary>  
        /// Identifier of the exercise associated with the question, if any.  
        /// </summary>  
        public int? ExerciseId { get; set; } = null;

        /// <summary>  
        /// Reference to the exercise associated with the question, if any.  
        /// </summary>  
        public Exercise? Exercise { get; set; } = null;

        /// <summary>
        /// Identifier of the user who created the question.
        /// </summary>
        public required string UserId { get; set; }

        /// <summary>
        /// Reference to the user who created the question.
        /// </summary>
        public User User { get; }

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
