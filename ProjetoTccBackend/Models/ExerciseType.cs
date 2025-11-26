using System.ComponentModel.DataAnnotations;

namespace ProjetoTccBackend.Models
{
    /// <summary>
    /// Represents a type/category of exercise.
    /// </summary>
    public class ExerciseType
    {
        /// <summary>
        /// Gets or sets the unique identifier of the exercise type.
        /// </summary>
        [Key]
        public required int Id { get; set; }

        /// <summary>
        /// Gets or sets the label/name of the exercise type.
        /// </summary>
        [StringLength(100)]
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the collection of exercises of this type.
        /// </summary>
        public ICollection<Exercise> Exercises { get; set; } = [];

        /// <summary>
        /// Explicit conversion operator from nullable integer to ExerciseType.
        /// </summary>
        /// <param name="v">The nullable integer value to convert.</param>
        /// <returns>Throws NotImplementedException.</returns>
        /// <exception cref="NotImplementedException">Always thrown as this conversion is not implemented.</exception>
        public static explicit operator ExerciseType(int? v)
        {
            throw new NotImplementedException();
        }
    }
}
