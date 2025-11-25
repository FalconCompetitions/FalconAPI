namespace ProjetoTccBackend.Models
{
    /// <summary>
    /// Represents the association between an exercise and a competition.
    /// </summary>
    public class ExerciseInCompetition
    {
        /// <summary>
        /// Gets or sets the identifier of the exercise.
        /// </summary>
        public int ExerciseId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the competition.
        /// </summary>
        public int CompetitionId { get; set; }

        /// <summary>
        /// Gets or sets the exercise entity.
        /// </summary>
        public Exercise Exercise { get; set; } = null;

        /// <summary>
        /// Gets or sets the competition entity.
        /// </summary>
        public Competition Competition { get; set; } = null;
    }
}
