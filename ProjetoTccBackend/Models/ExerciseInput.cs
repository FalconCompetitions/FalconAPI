using System.ComponentModel.DataAnnotations;

namespace ProjetoTccBackend.Models
{
    /// <summary>
    /// Represents an input example for an exercise.
    /// </summary>
    public class ExerciseInput
    {
        /// <summary>
        /// Gets or sets the unique identifier of the exercise input.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the exercise this input belongs to.
        /// </summary>
        public int ExerciseId { get; set; }

        /// <summary>
        /// Gets or sets the UUID used by the judge system to identify the input.
        /// </summary>
        [StringLength(36, ErrorMessage = "Invalid exercise UUID")]
        public string JudgeUuid { get; set; }

        /// <summary>
        /// Gets or sets the exercise this input belongs to.
        /// </summary>
        public Exercise Exercise { get; set; }

        /// <summary>
        /// Gets or sets the input content.
        /// </summary>
        [DataType(DataType.MultilineText)]
        public string Input { get; set; }

        /// <summary>
        /// Gets or sets the expected output for this input.
        /// </summary>
        public ExerciseOutput ExerciseOutput { get; set; }
    }
}
