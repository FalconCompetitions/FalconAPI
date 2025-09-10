using ProjetoTccBackend.Enums.Judge;
using System.Text.Json.Serialization;

namespace ProjetoTccBackend.Database.Responses.Exercise
{
    public class ExerciseSubmissionResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("exerciseId")]
        public int ExerciseId { get; set; }

        [JsonPropertyName("groupId")]
        public int GroupId { get; set; }

        [JsonPropertyName("accepted")]
        public bool Accepted { get; set; }

        [JsonPropertyName("judgeResponse")]
        public JudgeSubmissionResponse JudgeResponse { get; set; }

    }
}
