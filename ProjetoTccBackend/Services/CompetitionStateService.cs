using ProjetoTccBackend.Services.Interfaces;

namespace ProjetoTccBackend.Services
{
    public class CompetitionStateService : ICompetitionStateService
    {
        private bool _hasActiveCompetitions = false;

        public bool HasActiveCompetitions => this._hasActiveCompetitions;

        public void SignalNewCompetition()
        {
            this._hasActiveCompetitions = true;
        }

        public void SignalNoActiveCompetitions()
        {
            this._hasActiveCompetitions = false;
        }
    }
}
