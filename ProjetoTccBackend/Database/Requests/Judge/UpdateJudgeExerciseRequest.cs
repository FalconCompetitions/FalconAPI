using System.Text.Json.Serialization;

namespace ProjetoTccBackend.Database.Requests.Judge
{
    /// <summary>
    /// Represents a request to update a judge exercise.
    /// </summary>
    public class UpdateJudgeExerciseRequest
    {
        /// <summary>
        /// Gets or sets the problem ID.
        /// </summary>
        [JsonPropertyName("problem_id")]
        public required string ProblemId { get; set; }

        /// <summary>
        /// Gets or sets the name of the exercise.
        /// </summary>
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the exercise.
        /// </summary>
        [JsonPropertyName("description")]
        public required string Description { get; set; }

        /// <summary>
        /// Gets or sets the data entries for the exercise.
        /// </summary>
        [JsonPropertyName("data_entry")]
        public required ICollection<string> DataEntry { get; set; }

        /// <summary>
        /// Gets or sets the data outputs for the exercise.
        /// </summary>
        [JsonPropertyName("data_output")]
        public required ICollection<string> DataOutput { get; set; }
    }
}
