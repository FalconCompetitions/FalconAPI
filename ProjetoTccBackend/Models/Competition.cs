using System.ComponentModel.DataAnnotations;

namespace ProjetoTccBackend.Models
{
    /// <summary>
    /// Represents a competition event, including its schedule, duration, and related entities such as groups, exercises, and rankings.
    /// </summary>
    public class Competition
    {
        /// <summary>
        /// Unique identifier for the competition.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// The start date and time of the competition.
        /// </summary>
        [Required]
        public DateTime StartTime { get; set; }

        /// <summary>
        /// The end date and time of the competition.
        /// </summary>
        [Required]
        public DateTime EndTime { get; set; }

        /// <summary>
        /// The total duration of the competition.
        /// </summary>
        [Required]
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// The date and time when the ranking will be stopped.
        /// </summary>
        [Required]
        public DateTime StopRanking { get; set; }

        /// <summary>
        /// The date and time after which submissions are blocked.
        /// </summary>
        public DateTime BlockSubmissions { get; set; }

        /// <summary>
        /// The penalty of the submission if rejected.
        /// </summary>
        [Required]
        public TimeSpan SubmissionPenalty { get; set; }

        /// <summary>
        /// The collection of groups participating in the competition.
        /// </summary>
        public ICollection<Group> Groups { get; } = [];

        /// <summary>
        /// The collection of group-competition relationships.
        /// </summary>
        public ICollection<GroupInCompetition> GroupInCompetitions { get; } = [];

        /// <summary>
        /// The collection of exercises included in the competition.
        /// </summary>
        public ICollection<Exercise> Exercices { get; } = [];

        /// <summary>
        /// The collection of exercise-competition relationships.
        /// </summary>
        public ICollection<ExerciseInCompetition> ExercisesInCompetition { get; } = [];

        /// <summary>
        /// The collection of group exercise attempts in the competition.
        /// </summary>
        public ICollection<GroupExerciseAttempt> GroupExerciseAttempts { get; } = [];

        /// <summary>
        /// The collection of questions associated with the competition.
        /// </summary>
        public ICollection<Question> Questions { get; } = [];

        /// <summary>
        /// The ranking information for the competition.
        /// </summary>
        public ICollection<CompetitionRanking> CompetitionRankings { get; set; } = [];

        public ICollection<Log> Logs { get; } = [];
    }
}
