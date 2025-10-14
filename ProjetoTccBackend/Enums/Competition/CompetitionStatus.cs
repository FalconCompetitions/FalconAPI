namespace ProjetoTccBackend.Enums.Competition
{
    /// <summary>
    /// Represents the various statuses that a competition can have during its lifecycle.
    /// </summary>
    /// <remarks>The <see cref="CompetitionStatus"/> enumeration defines the possible states of a competition,
    /// from its initial creation to its completion. Each status indicates a specific phase in the  competition's
    /// lifecycle, such as whether inscriptions are open, the competition is ongoing,  or it has finished.</remarks>
    public enum CompetitionStatus
    {
        /// <summary>
        /// Represents a state where the operation is pending and has not yet completed.
        /// </summary>
        Pending = 0,
        /// <summary>
        /// Represents the state where inscriptions are open for submission.
        /// </summary>
        OpenInscriptions = 1,
        /// <summary>
        /// Represents a state where inscriptions are closed and no further inscriptions are allowed.
        /// </summary>
        ClosedInscriptions = 2,
        /// <summary>
        /// Represents a status indicating that the process or operation is currently ongoing.
        /// </summary>
        Ongoing = 3,
        /// <summary>
        /// Indicates that the process or operation has completed successfully.
        /// </summary>
        Finished = 4,
        /// <summary>
        /// Represents a competition model not yet ready to process.
        /// </summary>
        ModelTemplate = 5,
    }
}
