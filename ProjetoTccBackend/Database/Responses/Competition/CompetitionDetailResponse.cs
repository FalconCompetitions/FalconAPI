using System.Text.Json.Serialization;
using ProjetoTccBackend.Database.Responses.Exercise;
using ProjetoTccBackend.Database.Responses.Group;
using ProjetoTccBackend.Database.Responses.User;
using ProjetoTccBackend.Enums.Competition;
using ProjetoTccBackend.Enums.Exercise;
using ProjetoTccBackend.Enums.Judge;
using ProjetoTccBackend.Enums.Log;
using ProjetoTccBackend.Enums.Question;

namespace ProjetoTccBackend.Database.Responses.Competition
{
    /// <summary>
    /// Detailed response for a competition, including all related entities without circular references.
    /// Used for archived/history competition views.
    /// </summary>
    public class CompetitionDetailResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("maxExercises")]
        public int? MaxExercises { get; set; }

        [JsonPropertyName("maxMembers")]
        public int MaxMembers { get; set; }

        [JsonPropertyName("maxSubmissionSize")]
        public int? MaxSubmissionSize { get; set; }

        [JsonPropertyName("startInscriptions")]
        public DateTime StartInscriptions { get; set; }

        [JsonPropertyName("endInscriptions")]
        public DateTime EndInscriptions { get; set; }

        [JsonPropertyName("status")]
        public CompetitionStatus Status { get; set; }

        [JsonPropertyName("startTime")]
        public DateTime StartTime { get; set; }

        [JsonPropertyName("endTime")]
        public DateTime? EndTime { get; set; }

        [JsonPropertyName("duration")]
        public TimeSpan Duration { get; set; }

        [JsonPropertyName("stopRanking")]
        public DateTime? StopRanking { get; set; }

        [JsonPropertyName("blockSubmissions")]
        public DateTime? BlockSubmissions { get; set; }

        [JsonPropertyName("submissionPenalty")]
        public TimeSpan SubmissionPenalty { get; set; }

        [JsonPropertyName("groups")]
        public ICollection<GroupDetailResponse>? Groups { get; set; }

        [JsonPropertyName("exercises")]
        public ICollection<ExerciseDetailResponse>? Exercises { get; set; }

        [JsonPropertyName("groupExerciseAttempts")]
        public ICollection<GroupExerciseAttemptDetailResponse>? GroupExerciseAttempts { get; set; }

        [JsonPropertyName("questions")]
        public ICollection<QuestionDetailResponse>? Questions { get; set; }

        [JsonPropertyName("competitionRankings")]
        public ICollection<CompetitionRankingDetailResponse>? CompetitionRankings { get; set; }

        [JsonPropertyName("logs")]
        public ICollection<LogDetailResponse>? Logs { get; set; }
    }

    /// <summary>
    /// Detailed group response without circular references to Competition.
    /// </summary>
    public class GroupDetailResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("leaderId")]
        public string LeaderId { get; set; } = string.Empty;

        [JsonPropertyName("users")]
        public ICollection<UserSimpleResponse>? Users { get; set; }
    }

    /// <summary>
    /// Simple user response without circular references.
    /// </summary>
    public class UserSimpleResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("ra")]
        public string? Ra { get; set; }

        [JsonPropertyName("joinYear")]
        public int? JoinYear { get; set; }

        [JsonPropertyName("department")]
        public string? Department { get; set; }
    }

    /// <summary>
    /// Detailed exercise response without circular references to Competition.
    /// </summary>
    public class ExerciseDetailResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("exerciseTypeId")]
        public int ExerciseTypeId { get; set; }

        [JsonPropertyName("estimatedTime")]
        public TimeSpan EstimatedTime { get; set; }

        [JsonPropertyName("judgeUuid")]
        public string? JudgeUuid { get; set; }

        [JsonPropertyName("attachedFileId")]
        public int? AttachedFileId { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("inputs")]
        public ICollection<ExerciseInputResponse>? Inputs { get; set; }

        [JsonPropertyName("outputs")]
        public ICollection<ExerciseOutputResponse>? Outputs { get; set; }
    }

    /// <summary>
    /// Detailed group exercise attempt response without circular references.
    /// </summary>
    public class GroupExerciseAttemptDetailResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("exerciseId")]
        public int ExerciseId { get; set; }

        [JsonPropertyName("exerciseTitle")]
        public string? ExerciseTitle { get; set; }

        [JsonPropertyName("groupId")]
        public int GroupId { get; set; }

        [JsonPropertyName("groupName")]
        public string? GroupName { get; set; }

        [JsonPropertyName("competitionId")]
        public int CompetitionId { get; set; }

        [JsonPropertyName("time")]
        public TimeSpan Time { get; set; }

        [JsonPropertyName("submissionTime")]
        public DateTime SubmissionTime { get; set; }

        [JsonPropertyName("language")]
        public LanguageType Language { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("accepted")]
        public bool Accepted { get; set; }

        [JsonPropertyName("judgeResponse")]
        public JudgeSubmissionResponse JudgeResponse { get; set; }
    }

    /// <summary>
    /// Detailed question response without circular references.
    /// </summary>
    public class QuestionDetailResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("competitionId")]
        public int CompetitionId { get; set; }

        [JsonPropertyName("exerciseId")]
        public int? ExerciseId { get; set; }

        [JsonPropertyName("exerciseTitle")]
        public string? ExerciseTitle { get; set; }

        [JsonPropertyName("userId")]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName("userName")]
        public string? UserName { get; set; }

        [JsonPropertyName("userGroupName")]
        public string? UserGroupName { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;

        [JsonPropertyName("questionType")]
        public QuestionType QuestionType { get; set; }

        [JsonPropertyName("answer")]
        public AnswerDetailResponse? Answer { get; set; }
    }

    /// <summary>
    /// Detailed answer response without circular references.
    /// </summary>
    public class AnswerDetailResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;

        [JsonPropertyName("userId")]
        public string? UserId { get; set; }

        [JsonPropertyName("userName")]
        public string? UserName { get; set; }
    }

    /// <summary>
    /// Detailed competition ranking response without circular references.
    /// </summary>
    public class CompetitionRankingDetailResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("groupId")]
        public int GroupId { get; set; }

        [JsonPropertyName("group")]
        public GroupDetailResponse? Group { get; set; }

        [JsonPropertyName("points")]
        public double Points { get; set; }

        [JsonPropertyName("penalty")]
        public double Penalty { get; set; }

        [JsonPropertyName("rankOrder")]
        public int RankOrder { get; set; }

        [JsonPropertyName("exerciseAttempts")]
        public ICollection<GroupExerciseAttemptResponse>? ExerciseAttempts { get; set; }
    }

    /// <summary>
    /// Detailed log response without circular references.
    /// </summary>
    public class LogDetailResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("actionType")]
        public LogType ActionType { get; set; }

        [JsonPropertyName("actionTime")]
        public DateTime ActionTime { get; set; }

        [JsonPropertyName("ipAddress")]
        public string IpAddress { get; set; } = string.Empty;

        [JsonPropertyName("userId")]
        public string? UserId { get; set; }

        [JsonPropertyName("userName")]
        public string? UserName { get; set; }

        [JsonPropertyName("groupId")]
        public int? GroupId { get; set; }

        [JsonPropertyName("groupName")]
        public string? GroupName { get; set; }

        [JsonPropertyName("competitionId")]
        public int? CompetitionId { get; set; }
    }
}
