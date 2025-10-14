using System.Text.Json.Serialization;

namespace ProjetoTccBackend.Database.Responses.Exercise
{
    public class ExerciseResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("exerciseTypeId")]
        public int ExerciseTypeId { get; set; }

        [JsonPropertyName("attachedFileId")]
        public int AttachedFileId { get; set; }

        [JsonPropertyName("inputs")]
        public ICollection<ExerciseInputResponse> Inputs { get; set; }

        [JsonPropertyName("outputs")]
        public ICollection<ExerciseOutputResponse> Outputs { get; set; }
    }
}
