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
        private readonly TimeSpan _idleTime;
        private readonly TimeSpan _operationalTime;
        private const string CompetitionCacheKey = "currentCompetition";

        /// <summary>
        /// Initializes a new instance of the <see cref="CompetitionStateWorker"/> class.
        /// </summary>
        /// <param name="configuration">Configuration for worker settings.</param>
        /// <param name="logger">Logger for registering information and errors.</param>
        /// <param name="memoryCache">Memory cache for storing competition data.</param>
        /// <param name="scopeFactory">Factory for creating service scopes.</param>
        /// <param name="competitionStateService">Service for managing competition state operations.</param>
        public CompetitionStateWorker(
            IConfiguration configuration,
            ILogger<CompetitionStateWorker> logger,
            IMemoryCache memoryCache,
            IServiceScopeFactory scopeFactory,
            ICompetitionStateService competitionStateService
        )
        {
            int idleSeconds = configuration.GetValue<int>("CompetitionWorker:IdleSeconds");
            int operationalSeconds = configuration.GetValue<int>(
                "CompetitionWorker:OperationalSeconds"
            );
            this._logger = logger;
            this._memoryCache = memoryCache;
            this._scopeFactory = scopeFactory;
            this._competitionStateService = competitionStateService;
            this._idleTime = TimeSpan.FromSeconds(idleSeconds);
            this._operationalTime = TimeSpan.FromSeconds(operationalSeconds);
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
        /// <remarks> If an error occurs during processing, it is logged, and the worker waits for a minute before
        /// retrying. The worker also handles graceful shutdown when the cancellation token is triggered.</remarks>
        /// <param name="stoppingToken">A <see cref="CancellationToken"/> that is triggered when the worker process should stop.</param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this._logger.LogInformation("Competition State Worker Initialized...");
            try
            {
                await this.CheckInitialStateAsync();
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Error initializing Competition State Worker");
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = this._scopeFactory.CreateScope())
                    {
                        bool hasActiveCompetitions =
                            this._competitionStateService.HasActiveCompetitions;
                        var delay =
                            (hasActiveCompetitions) ? this._operationalTime : this._idleTime;

                        if (delay.TotalSeconds < 1)
                        {
                            this._logger.LogWarning(
                                "Competition State Worker delay is set to less than 1 second. Adjusting to 1 second."
                            );
                            delay = TimeSpan.FromSeconds(10);
                        }

                        this._logger.LogCritical($"HasActiveCompetitions: {hasActiveCompetitions}");

                        if (hasActiveCompetitions)
                        {
                            await this.ProcessCompetitionsAsync();
                        }

                        await Task.Delay(delay, stoppingToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Graceful shutdown
                    break;
                }
                catch (Exception ex)
                {
                    this._logger.LogError(ex, "Error in Competition State Worker loop");

                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
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
        /// <remarks>
        /// This method evaluates the competition's current status and performs the necessary state transition:
        /// <list type="bullet">
        ///   <item>
        ///     <description>If the competition is already invalid for some reason, closes the competition</description>
        ///   </item>
        ///   <item>
        ///     <description>If the competition is in the <see cref="CompetitionStatus.Pending"/> state and the current time is within the inscription period, inscriptions are opened.</description>
        ///   </item>
        ///   <item>
        ///     <description>If the competition is in the <see cref="CompetitionStatus.OpenInscriptions"/> state and the inscription period has ended, inscriptions are closed.</description>
        ///   </item>
        ///   <item>
        ///     <description>If the competition is in the <see cref="CompetitionStatus.ClosedInscriptions"/> state and the competition's start time has passed, the competition is started and transitions to <see cref="CompetitionStatus.Ongoing"/>.</description>
        ///   </item>
        ///   <item>
        ///     <description>If the competition is in the <see cref="CompetitionStatus.Ongoing"/> state and the competition's end time has passed, the competition is ended.</description>
        ///   </item>
        /// </list>
        /// No action is taken if the competition's state does not meet the conditions for a transition.
        /// </remarks>
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
            if (competition.EndTime <= now)
            {
                await competitionService.EndCompetitionAsync(competition);
                return;
            }

            if (competition.Status == CompetitionStatus.Pending)
            {
                if (competition.StartInscriptions < now && competition.EndInscriptions > now)
                {
                    await competitionService.OpenCompetitionInscriptionsAsync(competition);
                }
                return;
            }

            if (competition.Status == CompetitionStatus.OpenInscriptions)
            {
                if (competition.EndInscriptions < now)
                {
                    await competitionService.CloseCompetitionInscriptionsAsync(competition);
                }
                return;
            }

            if (competition.Status == CompetitionStatus.ClosedInscriptions)
            {
                if (competition.StartTime < now)
                {
                    await competitionService.StartCompetitionAsync(competition);
                }
                return;
            }

            if (competition.Status == CompetitionStatus.Ongoing)
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
