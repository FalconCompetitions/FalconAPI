using System.ComponentModel.DataAnnotations;

namespace ProjetoTccBackend.Models
{
    public class ExerciseType
    {
        [Key]
        public required int Id { get; set; }

        [Required]
        [StringLength(100)]
        public required string Label { get; set; }


        public ICollection<Exercise> Exercises { get; set; } = [];
    }
}
