using System.ComponentModel.DataAnnotations;
using ProjetoTccBackend.Database.Requests.Competition;
using ProjetoTccBackend.Enums.Competition;

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
        /// Name of the Competition
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description associated with the object.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of exercises allowed.
        /// </summary>
        public int? MaxExercises { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of members allowed in the group.
        /// </summary>
        public int MaxMembers { get; set; }

        /// <summary>
        /// Gets or sets the maximum allowed size, in kb, for a submission.
        /// </summary>
        public int? MaxSubmissionSize { get; set; }

        /// <summary>
        /// Gets or sets the date and time when inscriptions start.
        /// </summary>
        public DateTime StartInscriptions { get; set; }

        /// <summary>
        /// Gets or sets the date and time when inscriptions end.
        /// </summary>
        public DateTime EndInscriptions { get; set; }

        /// <summary>
        /// Gets or sets the current status of the competition.
        /// </summary>
        /// <remarks>This property is required and must be set to a valid <see cref="CompetitionStatus"/>
        /// value.</remarks>
        [Required]
        public CompetitionStatus Status { get; set; } = CompetitionStatus.ModelTemplate;

        /// <summary>
        /// The start date and time of the competition.
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// The end date and time of the competition.
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// The total duration of the competition.
        /// </summary>
        [Required]
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// The date and time when the ranking will be stopped.
        /// </summary>
        public DateTime? StopRanking { get; set; }

        /// <summary>
        /// The date and time after which submissions are blocked.
        /// </summary>
        public DateTime? BlockSubmissions { get; set; }

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

        /// <summary>
        /// Gets the collection of logs associated with the current instance.
        /// </summary>
        public ICollection<Log> Logs { get; } = [];

        /// <summary>
        /// Updates the status of the competition to the specified value.
        /// </summary>
        /// <remarks>Use this method to change the current state of the competition. Ensure that the
        /// provided <paramref name="status"/> is a valid <see cref="CompetitionStatus"/> value.</remarks>
        /// <param name="status">The new status to assign to the competition.</param>
        public void ChangeCompetitionStatus(CompetitionStatus status)
        {
            this.Status = status;
        }

        /// <summary>
        /// Updates the competition's configuration and timing details based on the provided data.
        /// </summary>
        /// <remarks>This method calculates and updates the competition's end time, stop ranking date,
        /// and block submissions date based on the start time and respective durations provided  in the <paramref
        /// name="newData"/> object. It also updates other competition settings  such as the maximum number of
        /// exercises, maximum submission size, competition name, submission penalty, and Duration in minutes.</remarks>
        /// <param name="newData">An instance of <see cref="CompetitionRequest"/> containing the new competition settings,  including start
        /// time, duration, and other configuration parameters.</param>
        public void ProcessCompetitionData(CompetitionRequest newData, bool isNew, bool forceInscriptionsUpdate)
        {
            DateTime? newEndTime = (isNew) ? null : newData.StartTime.Add(newData.Duration!.Value);
            DateTime? newStopRankingDate =
                (isNew) ? null : newData.StartTime.Add(newData.StopRanking!.Value);
            DateTime? newBlockSubmissionsDate =
                (isNew) ? null : newData.StartTime.Add(newData.BlockSubmissions!.Value);

            this.StartTime = newData.StartTime;
            this.EndTime = newEndTime;
            this.StopRanking = newStopRankingDate;
            this.BlockSubmissions = newBlockSubmissionsDate;
            this.MaxExercises = (isNew) ? null : newData.MaxExercises;
            this.MaxMembers = newData.MaxMembers;
            this.MaxSubmissionSize = (isNew) ? null : newData.MaxSubmissionSize;
            this.StartInscriptions = (forceInscriptionsUpdate) ? newData.StartInscriptions : this.StartInscriptions;
            this.EndInscriptions = (forceInscriptionsUpdate) ?  newData.EndInscriptions : this.EndInscriptions;
            this.Name = newData.Name;
            this.Description = newData.Description;
            this.SubmissionPenalty = newData.SubmissionPenalty;
            this.Duration = newData.Duration!.Value;
        }
    }
}
