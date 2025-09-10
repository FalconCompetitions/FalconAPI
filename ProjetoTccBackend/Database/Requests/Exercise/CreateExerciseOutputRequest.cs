using ProjetoTccBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ProjetoTccBackend.Database.Requests.Exercise
{
    public class CreateExerciseOutputRequest
    {
        [JsonPropertyName("exerciseId")]
        public int? ExerciseId { get; set; }
        [JsonPropertyName("exerciseInputId")]
        public int? ExerciseInputId { get; set; }

        [Required]
        [JsonPropertyName("orderId")]
        public int OrderId { get; set; }

        [Required]
        [JsonPropertyName("output")]
        public string Output { get; set; }
    }
}
