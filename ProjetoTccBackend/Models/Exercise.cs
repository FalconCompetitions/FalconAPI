using System.ComponentModel.DataAnnotations;

namespace ProjetoTccBackend.Models
{
    /// <summary>
    /// Represents a programming exercise, including its metadata, description, estimated time, and related entities.
    /// </summary>
    public class Exercise
    {
        /// <summary>
        /// Unique identifier for the exercise.
        /// </summary>
        [Key]
        public int Id { get; set; }

        [Required]
        public int ExerciseTypeId { get; set; }

        public ExerciseType ExerciseType { get; set; }

        /// <summary>
        /// Title of the exercise.
        /// </summary>
        [StringLength(200)]
        public required string Title { get; set; }

        /// <summary>
        /// Detailed description of the exercise.
        /// </summary>
        [DataType(DataType.MultilineText)]
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the attached file.
        /// </summary>
        public int? AttachedFileId { get; set; }

        /// <summary>
        /// Gets the file attached to the current entity.
        /// </summary>
        /// <remarks>The <see cref="AttachedFile"/> property provides access to the file associated with
        /// the entity.  If no file is attached, this property may return <see langword="null"/>.</remarks>
        public AttachedFile? AttachedFile { get; } = null;

        /// <summary>
        /// Estimated time to solve the exercise.
        /// </summary>
        public TimeSpan EstimatedTime { get; set; }

        /// <summary>
        /// UUID used by the judge system to identify the exercise.
        /// </summary>
        [StringLength(36, ErrorMessage = "UUID do exercício inválido")]
        public string? JudgeUuid { get; set; }

        /// <summary>
        /// Date and time when the exercise was created.
        /// </summary>
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Collection of input examples for the exercise.
        /// </summary>
        public ICollection<ExerciseInput> ExerciseInputs { get; set; } = [];

        /// <summary>
        /// Collection of output examples for the exercise.
        /// </summary>
        public ICollection<ExerciseOutput> ExerciseOutputs { get; set; } = [];

        /// <summary>
        /// Competitions in which this exercise is included.
        /// </summary>
        public ICollection<Competition> Competitions { get; set; } = [];

        /// <summary>
        /// Collection of associations between this exercise and competitions.
        /// </summary>
        public ICollection<ExerciseInCompetition> ExerciseInCompetions { get; set; } = [];

        /// <summary>
        /// Attempts made by groups to solve this exercise.
        /// </summary>
        public ICollection<GroupExerciseAttempt> GroupExerciseAttempts { get; set; } = [];

        /// <summary>
        /// Questions related to this exercise.
        /// </summary>
        public ICollection<Question> Questions { get; set; } = [];




        /// <summary>
        /// Updates the exercise data with a new title, description, and estimated time.
        /// </summary>
        /// <param name="title">The new title of the exercise.</param>
        /// <param name="description">The new description of the exercise.</param>
        /// <param name="estimatedTime">The new estimated time to solve the exercise.</param>
        public void UpdateExerciseData(string title, string description, TimeSpan estimatedTime)
        {
            this.Title = title;
            this.Description = description;
            this.EstimatedTime = estimatedTime;
        }

        /// <summary>
        /// Sets the UUID used by the judge system to identify the exercise.
        /// </summary>
        /// <param name="uuid">The UUID to be set for the exercise.</param>
        public void SetJudgeUuid(string uuid)
        {
            this.JudgeUuid = uuid;
        }
    }
}
