using System.Text.Json.Serialization;

namespace ProjetoTccBackend.Database.Responses.Competition
{
    /// <summary>
    /// Information about a group's exercise attempts in a competition.
    /// </summary>
    public class GroupExerciseAttemptResponse
    {
        /// <summary>
        /// Identifier of the group.
        /// </summary>
        [JsonPropertyName("groupId")]
        public int GroupId { get; set; }

        /// <summary>
        /// Identifier of the exercise.
        /// </summary>
        [JsonPropertyName("exerciseId")]
        public int ExerciseId { get; set; }

        /// <summary>
        /// Number of attempts made by the group for the exercise.
        /// </summary>
        [JsonPropertyName("attempts")]
        public int Attempts { get; set; }

        /// <summary>
        /// Indicates whether the exercise was accepted (solved correctly).
        /// </summary>
        [JsonPropertyName("accepted")]
        public bool Accepted { get; set; }
    }
}
