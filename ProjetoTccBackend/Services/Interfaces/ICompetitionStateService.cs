namespace ProjetoTccBackend.Services.Interfaces
{
    public interface ICompetitionStateService
    {
        bool HasActiveCompetitions { get; }

        void SignalNewCompetition();

        void SignalNoActiveCompetitions();

    }
}
