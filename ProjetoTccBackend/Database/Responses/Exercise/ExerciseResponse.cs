using System.Text.Json.Serialization;

namespace ProjetoTccBackend.Database.Responses.Exercise
{
    public class ExerciseResponse
    {
        public int Id { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }

        [JsonPropertyName("exerciseTypeId")]
        public int ExerciseTypeId { get; set; }

        public ICollection<ExerciseInputResponse> Inputs { get; set; }
        public ICollection<ExerciseOutputResponse> Outputs { get; set; }
    }
}
