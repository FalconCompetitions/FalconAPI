using System.Text.Json.Serialization;

namespace ProjetoTccBackend.Database.Requests.Judge
{
    /// <summary>
    /// Represents a request to create a new judge exercise, including its name, description, input and output data, and their descriptions.
    /// </summary>
    public class CreateJudgeExerciseRequest
    {
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
        /// Gets or sets the collection of input data samples for the exercise.
        /// </summary>
        [JsonPropertyName("data_entries")]
        public required ICollection<string> DataEntry { get; set; }

        /// <summary>
        /// Gets or sets the description of the input data.
        /// </summary>
        [JsonPropertyName("entry_description")]
        public string EntryDescription { get; set; }

        /// <summary>
        /// Gets or sets the collection of expected output data samples for the exercise.
        /// </summary>
        [JsonPropertyName("data_output")]
        public required ICollection<string> DataOutput { get; set; }

        /// <summary>
        /// Gets or sets the description of the output data.
        /// </summary>
        [JsonPropertyName("output_description")]
        public string OutputDescription { get; set; }
    }
}
