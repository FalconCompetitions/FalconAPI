using ProjetoTccBackend.Database.Responses.User;
using System.Text.Json.Serialization;

namespace ProjetoTccBackend.Database.Responses.Question
{
    public class AnswerResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }

        [JsonPropertyName("userId")]
        public string UserId { get; set; }
    }

    public class QuestionResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("content")]
        public int CompetitionId { get; set; }

        [JsonPropertyName("exerciseId")]
        public int? ExerciseId { get; set; }

        [JsonPropertyName("userId")]
        public string UserId { get; set; }

        [JsonPropertyName("user")]
        public GenericUserInfoResponse User { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }

        [JsonPropertyName("answerId")]
        public int? AnswerId { get; set; }

        [JsonPropertyName("answer")]
        public AnswerResponse? Answer { get; set; }

        [JsonPropertyName("questionType")]
        public int QuestionType { get; set; }
    }
}
