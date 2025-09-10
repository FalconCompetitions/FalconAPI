using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ProjetoTccBackend.Database.Requests.Exercise
{
    public class UpdateExerciseRequest
    {
        [Required]
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [Required]
        [JsonPropertyName("exerciseTypeId")]
        public int ExerciseTypeId { get; set; }

        [Required]
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [Required]
        [JsonPropertyName("description")]
        public string Description { get; set; }

        [Required]
        [JsonPropertyName("estimatedTime")]
        public TimeSpan EstimatedTime { get; set; }

        [JsonPropertyName("inputs")]
        public ICollection<UpdateExerciseInputRequest> Inputs { get; set; }

        [JsonPropertyName("outputs")]
        public ICollection<UpdateExerciseOutputRequest> Outputs { get; set; }
    }
}
