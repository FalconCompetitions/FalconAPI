using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ProjetoTccBackend.Database.Requests.Exercise
{
    public class UpdateExerciseInputRequest
    {
        [JsonPropertyName("id")]
        public int? Id { get; set; }

        [JsonPropertyName("exerciseId")]
        public int? ExerciseId { get; set; }

        [Required]
        [JsonPropertyName("orderId")]
        public int OrderId { get; set; }

        [Required]
        [JsonPropertyName("input")]
        public string Input { get; set; }
    }
}
