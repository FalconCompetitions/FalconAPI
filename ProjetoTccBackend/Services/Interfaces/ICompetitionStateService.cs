namespace ProjetoTccBackend.Services.Interfaces
{
    /// <summary>
    /// Interface for the competition state management service.
    /// </summary>
    /// <remarks>
    /// Allows signaling the existence or absence of active competitions in the system.
    /// Used by workers and other components to control the competition lifecycle,
    /// facilitating state transitions and periodic operations.
    /// </remarks>
    public interface ICompetitionStateService
    {
        /// <summary>
        /// Indicates whether there are active competitions in the system.
        /// </summary>
        /// <remarks>
        /// Used by workers and services to determine if operations related to competitions should be executed.
        /// </remarks>
        bool HasActiveCompetitions { get; }

        /// <summary>
        /// Signals that a new competition has been detected and is active.
        /// </summary>
        /// <remarks>
        /// Should be called when a competition is opened or identified as active,
        /// allowing the system to perform operations related to ongoing competitions.
        /// </remarks>
        void SignalNewCompetition();

        /// <summary>
        /// Signals that there are no more active competitions in the system.
        /// </summary>
        /// <remarks>
        /// Should be called when all competitions have ended or there are no open competitions,
        /// allowing the system to enter idle mode or perform finalization operations.
        /// </remarks>
        void SignalNoActiveCompetitions();
    }
}
