using ProjetoTccBackend.Enums.Exercise;
using ProjetoTccBackend.Enums.Judge;
using System.ComponentModel.DataAnnotations;

namespace ProjetoTccBackend.Models
{
    /// <summary>
    /// Represents an attempt by a group to solve an exercise within a competition.
    /// Stores information about the exercise, the group, the competition, the code submitted,
    /// the time taken, the submission time, the programming language used, and the judge's response.
    /// </summary>
    public class GroupExerciseAttempt
    {
        /// <summary>
        /// Unique identifier for the group exercise attempt.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Identifier of the exercise being attempted.
        /// </summary>
        public int ExerciseId { get; set; }

        /// <summary>
        /// Reference to the exercise being attempted.
        /// </summary>
        public Exercise Exercise { get; }

        /// <summary>
        /// Time taken by the group to solve the exercise.
        /// </summary>
        public TimeSpan Time { get; set; }

        /// <summary>
        /// Date and time when the attempt was submitted.
        /// </summary>
        public DateTime SubmissionTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Programming language used for the submission.
        /// </summary>
        public LanguageType Language { get; set; }

        /// <summary>
        /// Identifier of the competition in which the attempt was made.
        /// </summary>
        public int CompetitionId { get; set; }

        /// <summary>
        /// Reference to the competition in which the attempt was made.
        /// </summary>
        public Competition Competition { get; }

        /// <summary>
        /// Identifier of the group making the attempt.
        /// </summary>
        public int GroupId { get; set; }

        /// <summary>
        /// Reference to the group making the attempt.
        /// </summary>
        public Group Group { get; }

        /// <summary>
        /// The code submitted by the group for the exercise.
        /// </summary>
        [DataType(DataType.Text)]
        public required string Code { get; set; }

        /// <summary>
        /// Indicates whether the attempt was accepted (correct solution).
        /// </summary>
        public bool Accepted { get; set; }

        /// <summary>
        /// The response from the judge system for the submission.
        /// </summary>
        public JudgeSubmissionResponse JudgeResponse { get; set; }
    }
}
