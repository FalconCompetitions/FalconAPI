using System.ComponentModel.DataAnnotations;

namespace ProjetoTccBackend.Models
{
    public class ExerciseType
    {
        [Key]
        public required int Id { get; set; }

        [StringLength(100)]
        public string Label { get; set; }


        public ICollection<Exercise> Exercises { get; set; } = [];
    }
}
