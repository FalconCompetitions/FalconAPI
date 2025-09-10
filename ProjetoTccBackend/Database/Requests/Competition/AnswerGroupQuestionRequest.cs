using System.Text.Json.Serialization;

namespace ProjetoTccBackend.Database.Requests.Competition
{
    public class AnswerGroupQuestionRequest
    {
        [JsonPropertyName("groupId")]
        public int GroupId { get; set; }

        [JsonPropertyName("questionId")]
        public int QuestionId { get; set; }

        [JsonPropertyName("answer")]
        public string Answer { get; set; }
    }
}
