using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ProjetoTccBackend.Database.Requests.Exercise
{
    public class CreateExerciseRequest
    {
        [Required]
        [JsonPropertyName("exerciseTypeId")]
        public int ExerciseTypeId { get; set; }

        [Required]
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }


        [JsonPropertyName("inputs")]
        public ICollection<CreateExerciseInputRequest> Inputs { get; set; }

        [JsonPropertyName("outputs")]
        public ICollection<CreateExerciseOutputRequest> Outputs { get; set; }

    }
}
