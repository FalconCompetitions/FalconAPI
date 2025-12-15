using ProjetoTccBackend.Services.Interfaces;

namespace ProjetoTccBackend.Services
{
    /// <summary>
    /// Service responsible for managing competition state and notifications.
    /// </summary>
    public class CompetitionStateService : ICompetitionStateService
    {
        private bool _hasActiveCompetitions = false;

        /// <inheritdoc />
        public bool HasActiveCompetitions => this._hasActiveCompetitions;

        /// <inheritdoc />
        public void SignalNewCompetition()
        {
            this._hasActiveCompetitions = true;
        }

        /// <inheritdoc />
        public void SignalNoActiveCompetitions()
        {
            this._hasActiveCompetitions = false;
        }
    }
}
