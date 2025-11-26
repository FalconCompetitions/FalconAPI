using ProjetoTccBackend.Enums.Exercise;
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

        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("languageId")]
        public LanguageType LanguageId { get; set; }

        [JsonPropertyName("submittedAt")]
        public DateTime SubmittedAt { get; set; }

        [JsonPropertyName("executionTime")]
        public int ExecutionTime { get; set; }

        [JsonPropertyName("memoryUsed")]
        public int MemoryUsed { get; set; }

        [JsonPropertyName("score")]
        public int Score { get; set; }

        [JsonPropertyName("points")]
        public int Points { get; set; }

        [JsonPropertyName("penalty")]
        public int Penalty { get; set; }
    }
}
