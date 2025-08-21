using ProjetoTccBackend.Enums.Judge;
using System.Text.Json.Serialization;

namespace ProjetoTccBackend.Database.Requests.Competition
{
    public class RevokeGroupSubmissionRequest
    {
        [JsonPropertyName("submissionId")]
        public int SubmissionId { get; set; }

        [JsonPropertyName("newJudgeResponse")]
        public JudgeSubmissionResponse NewJudgeResponse { get; set; }
    }
}