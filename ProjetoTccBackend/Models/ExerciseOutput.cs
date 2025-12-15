using System.ComponentModel.DataAnnotations;

namespace ProjetoTccBackend.Models
{
    /// <summary>
    /// Represents an expected output for an exercise input.
    /// </summary>
    public class ExerciseOutput
    {
        /// <summary>
        /// Gets or sets the unique identifier of the exercise output.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the exercise this output belongs to.
        /// </summary>
        public int ExerciseId { get; set; }

        /// <summary>
        /// Gets or sets the UUID used by the judge system to identify the output.
        /// </summary>
        [StringLength(36, ErrorMessage = "Invalid exercise UUID")]
        public string? JudgeUuid { get; set; }

        /// <summary>
        /// Gets or sets the exercise this output belongs to.
        /// </summary>
        public Exercise Exercise { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the corresponding exercise input.
        /// </summary>
        public int ExerciseInputId { get; set; }

        /// <summary>
        /// Gets or sets the corresponding exercise input.
        /// </summary>
        public ExerciseInput ExerciseInput { get; set; }

        /// <summary>
        /// Gets or sets the expected output content.
        /// </summary>
        [DataType(DataType.Text)]
        public string Output { get; set; }
    }
}
