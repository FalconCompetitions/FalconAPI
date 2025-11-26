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
        /// Gets or sets the unique identifier of the question.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the competition to which this question belongs.
        /// </summary>
        public required int CompetitionId { get; set; }

        /// <summary>
        /// Gets the reference to the competition to which this question belongs.
        /// </summary>
        public Competition Competition { get; }

        /// <summary>
        /// Gets or sets the identifier of the answer, if any.
        /// </summary>
        public int? AnswerId { get; set; } = null;

        /// <summary>
        /// Gets or sets the answer to this question, if any.
        /// </summary>
        public Answer? Answer { get; set; } = null;

        /// <summary>
        /// Gets or sets the identifier of the exercise associated with the question, if any.
        /// </summary>
        public int? ExerciseId { get; set; } = null;

        /// <summary>
        /// Gets or sets the reference to the exercise associated with the question, if any.
        /// </summary>
        public Exercise? Exercise { get; set; } = null;

        /// <summary>
        /// Gets or sets the identifier of the user who created the question.
        /// </summary>
        public required string UserId { get; set; }

        /// <summary>
        /// Gets the reference to the user who created the question.
        /// </summary>
        public User User { get; }

        /// <summary>
        /// Gets or sets the textual content of the question.
        /// </summary>
        [DataType(DataType.MultilineText)]
        public required string Content { get; set; }

        /// <summary>
        /// Gets or sets the type of the question, indicating its nature.
        /// </summary>
        public QuestionType QuestionType { get; set; }
    }
}
