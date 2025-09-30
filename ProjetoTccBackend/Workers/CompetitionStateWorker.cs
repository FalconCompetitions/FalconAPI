using Microsoft.Extensions.Caching.Memory;
using ProjetoTccBackend.Enums.Competition;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Services.Interfaces;

namespace ProjetoTccBackend.Workers
{
    /// <summary>
    /// Represents a background service that monitors and manages the state of competitions.
    /// </summary>
    /// <remarks>The <see cref="CompetitionStateWorker"/> is responsible for periodically checking the state
    /// of competitions and performing necessary state transitions, such as opening or closing inscriptions and ending
    /// competitions. It uses a combination of caching, scoped services, and a configurable delay mechanism to
    /// efficiently manage competition states. The worker runs continuously until it is stopped via a cancellation
    /// token.</remarks>
    public class CompetitionStateWorker : BackgroundService
    {
        private readonly ILogger<CompetitionStateWorker> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ICompetitionStateService _competitionStateService;
        private readonly TimeSpan _idleTime = TimeSpan.FromMinutes(5);
        private readonly TimeSpan _operationalTime = TimeSpan.FromMinutes(1);
        private const string CompetitionCacheKey = "currentCompetition";

        public CompetitionStateWorker(
            ILogger<CompetitionStateWorker> logger,
            IMemoryCache memoryCache,
            IServiceScopeFactory scopeFactory,
            ICompetitionStateService competitionStateService
        )
        {
            this._logger = logger;
            this._memoryCache = memoryCache;
            this._scopeFactory = scopeFactory;
            this._competitionStateService = competitionStateService;
        }

        /// <summary>
        /// Checks the initial state of open competitions and signals the competition state service if any are found.
        /// </summary>
        /// <remarks>This method retrieves the list of open competitions using the competition service.
        /// If one or more open competitions are detected, it signals the competition state service  to indicate the
        /// presence of a new competition.</remarks>
        /// <returns></returns>
        private async Task CheckInitialStateAsync()
        {
            using var scope = this._scopeFactory.CreateScope();
            var competitionService =
                scope.ServiceProvider.GetRequiredService<ICompetitionService>();
            ICollection<Competition> competitions =
                await competitionService.GetOpenCompetitionsAsync();

            if (competitions.Any())
            {
                this._competitionStateService.SignalNewCompetition();
            }
        }

        /// <summary>
        /// Executes the background worker process that monitors and manages competition states.
        /// </summary>
        /// <remarks>This method initializes the worker, checks the initial state of competitions, and
        /// enters a loop that periodically processes active competitions. The loop continues until the <paramref
        /// name="stoppingToken"/> is triggered. The delay between iterations is determined by whether there are active
        /// competitions.</remarks>
        /// <param name="stoppingToken">A <see cref="CancellationToken"/> that is triggered when the worker process should stop.</param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this._logger.LogInformation("Competition State Worker Initialized...");

            await this.CheckInitialStateAsync();

            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.UtcNow;

                using (var scope = this._scopeFactory.CreateScope())
                {
                    bool hasActiveCompetitions =
                        this._competitionStateService.HasActiveCompetitions;
                    var delay = (hasActiveCompetitions) ? this._operationalTime : this._idleTime;

                    if (hasActiveCompetitions)
                    {
                        await this.ProcessCompetitionsAsync();
                    }

                    await Task.Delay(delay, stoppingToken);
                }
            }
        }

        /// <summary>
        /// Processes all open competitions asynchronously.
        /// </summary>
        /// <remarks>This method retrieves a list of open competitions and processes each one
        /// individually.  If no open competitions are found, it signals that there are no active
        /// competitions.</remarks>
        /// <returns></returns>
        private async Task ProcessCompetitionsAsync()
        {
            using (var scope = this._scopeFactory.CreateScope())
            {
                ICompetitionService competitionService =
                    scope.ServiceProvider.GetRequiredService<ICompetitionService>();
                var competitions = await competitionService.GetOpenCompetitionsAsync();

                if (!competitions.Any())
                {
                    this._competitionStateService.SignalNoActiveCompetitions();
                    return;
                }

                DateTime now = DateTime.UtcNow;

                foreach (var competition in competitions)
                {
                    await this.ProcessCompetitionAsync(competition, competitionService, now);
                }
            }
        }

        /// <summary>
        /// Processes the current state of a competition and transitions it to the appropriate next state based on the
        /// current time and its status.
        /// </summary>
        /// <remarks>This method evaluates the competition's current status and performs the necessary
        /// state transition: <list type="bullet"> <item> <description>If the competition is in the <see
        /// cref="CompetitionStatus.Pending"/> state and the current time is within the inscription period, inscriptions
        /// are opened.</description> </item> <item> <description>If the competition is in the <see
        /// cref="CompetitionStatus.OpenInscriptions"/> state and the inscription period has ended, inscriptions are
        /// closed.</description> </item> <item> <description>If the competition is in the <see
        /// cref="CompetitionStatus.Ongoing"/> state and the competition's end time has passed, the competition is
        /// ended.</description> </item> </list> No action is taken if the competition's state does not meet the
        /// conditions for a transition.</remarks>
        /// <param name="competition">The competition to process. Must not be null.</param>
        /// <param name="competitionService">The service responsible for handling competition state transitions. Must not be null.</param>
        /// <param name="now">The current date and time used to evaluate the competition's state.</param>
        /// <returns></returns>
        private async Task ProcessCompetitionAsync(
            Competition competition,
            ICompetitionService competitionService,
            DateTime now
        )
        {
            if (competition.Status.Equals(CompetitionStatus.Pending))
            {
                if (competition.StartInscriptions < now && competition.EndInscriptions > now)
                {
                    await competitionService.OpenCompetitionInscriptionsAsync(competition);
                }
                return;
            }

            if (competition.Status.Equals(CompetitionStatus.OpenInscriptions))
            {
                if (competition.EndInscriptions < now)
                {
                    await competitionService.CloseCompetitionInscriptionsAsync(competition);
                }
                return;
            }

            if (competition.Status.Equals(CompetitionStatus.Ongoing))
            {
                if (competition.EndTime < now)
                {
                    await competitionService.EndCompetitionAsync(competition);
                }
                return;
            }
        }
    }
}
