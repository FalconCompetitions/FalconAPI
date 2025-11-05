using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using ProjetoTccBackend.Database.Requests.Competition;
using ProjetoTccBackend.Database.Requests.Group;
using ProjetoTccBackend.Database.Requests.Log;
using ProjetoTccBackend.Database.Responses.Competition;
using ProjetoTccBackend.Database.Responses.Exercise;
using ProjetoTccBackend.Database.Responses.Group;
using ProjetoTccBackend.Database.Responses.User;
using ProjetoTccBackend.Enums.Log;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Services.Interfaces;
using ProjetoTccBackend.Workers.Queues;

namespace ProjetoTccBackend.Hubs
{
    [Authorize]
    public class CompetitionHub : Hub
    {
        private readonly IGroupAttemptService _groupAttemptService;
        private readonly ICompetitionService _competitionService;
        private readonly ILogService _logService;
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAcessor;
        private readonly IMemoryCache _memoryCache;
        private readonly ExerciseSubmissionQueue _exerciseSubmissionQueue;
        private readonly ILogger<CompetitionHub> _logger;
        private readonly IGroupInCompetitionService _groupInCompetitionService;
        private const string CompetitionCacheKey = "currentCompetition";

        public CompetitionHub(
            IGroupAttemptService groupAttemptService,
            IGroupInCompetitionService groupInCompetitionService,
            ICompetitionService competitionService,
            IUserService userService,
            ILogService logService,
            IHttpContextAccessor httpContextAcessor,
            IMemoryCache memoryCache,
            ExerciseSubmissionQueue exerciseSubmissionQueue,
            ILogger<CompetitionHub> logger
        )
        {
            this._groupAttemptService = groupAttemptService;
            this._groupInCompetitionService = groupInCompetitionService;
            this._competitionService = competitionService;
            this._userService = userService;
            this._logService = logService;
            this._httpContextAcessor = httpContextAcessor;
            this._memoryCache = memoryCache;
            this._exerciseSubmissionQueue = exerciseSubmissionQueue;
            this._logger = logger;
        }

        /// <summary>
        /// Asynchronously retrieves the current competition, either from the cache or by querying the competition
        /// service.
        /// </summary>
        /// <remarks>If the competition is retrieved from the service, it is cached with an expiration
        /// time based on the competition's end time.</remarks>
        /// <returns>The current <see cref="Competition"/> if one is available; otherwise, <see langword="null"/>.</returns>
        private async Task<Competition?> FetchCurrentCompetitionAsync()
        {
            if (_memoryCache.TryGetValue(CompetitionCacheKey, out Competition? competition))
            {
                return competition;
            }

            competition = await this._competitionService.GetCurrentCompetition();

            if (competition is not null)
            {
                var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(
                    competition.EndTime!.Value
                );

                this._memoryCache.Set(CompetitionCacheKey, competition, cacheEntryOptions);
            }

            return competition;
        }

        private HttpContext GetHubHttpContext()
        {
            var httpContext = this._httpContextAcessor.HttpContext;

            if (httpContext is null)
            {
                throw new Exception();
            }

            return httpContext;
        }

        private ClaimsPrincipal GetHubContextUser()
        {
            var user = Context.User;

            if (user is null)
            {
                throw new Exception();
            }

            return user;
        }

        /// <summary>
        /// Handles a new client connection to the competition hub.
        /// </summary>
        /// <remarks>
        /// Upon connection, the method retrieves the current competition and checks the user's roles. Depending on the
        /// roles, the user is added to appropriate groups (Admins, Teachers, Students). It also logs the login action and sends
        /// the competition details back to the caller. If the user has no valid roles, the connection is aborted.
        /// </remarks>
        /// <returns></returns>
        public override async Task OnConnectedAsync()
        {
            //var currentCompetition = await this.FetchCurrentCompetitionAsync();
            var currentCompetition = await this._competitionService.GetCurrentCompetition();

            if (currentCompetition is null)
            {
                await this.Clients.Caller.SendAsync("OnConnectionResponse", null);
                return;
            }

            HttpContext currentHttpContext = this.GetHubHttpContext();

            ClaimsPrincipal user = this.GetHubContextUser();
            var loggedUser = this._userService.GetHttpContextLoggedUser();
            bool isInvalid = false;

            if (user.IsInRole("Admin"))
            {
                await this.Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
            }
            else if (user.IsInRole("Teacher"))
            {
                await this.Groups.AddToGroupAsync(Context.ConnectionId, "Teachers");
            }
            else if (user.IsInRole("Student"))
            {
                await this.Groups.AddToGroupAsync(Context.ConnectionId, "Students");
                await this.Groups.AddToGroupAsync(Context.ConnectionId, loggedUser.Id);
            }
            else
            {
                isInvalid = true;
                this._logger.LogCritical("Usuário não possui nenhuma role válida");
            }

            if (isInvalid == false)
            {
                await this._logService.CreateLogAsync(
                    new CreateLogRequest()
                    {
                        UserId = loggedUser.Id,
                        ActionType = LogType.Login,
                        CompetitionId = currentCompetition!.Id,
                        GroupId = loggedUser.GroupId,
                        IpAddress = currentHttpContext.Connection.RemoteIpAddress!.ToString(),
                    }
                );
            }

            if (isInvalid == true)
            {
                this.Context.Abort();
                return;
            }

            await this.Clients.Caller.SendAsync(
                "OnConnectionResponse",
                new CompetitionResponse()
                {
                    Id = currentCompetition.Id,
                    Name = currentCompetition.Name,
                    Description = currentCompetition.Description,
                    StartTime = currentCompetition.StartTime,
                    EndTime = currentCompetition.EndTime,
                    StartInscriptions = currentCompetition.StartInscriptions,
                    EndInscriptions = currentCompetition.EndInscriptions,
                    BlockSubmissions = currentCompetition.BlockSubmissions,
                    StopRanking = currentCompetition.StopRanking,
                    SubmissionPenalty = currentCompetition.SubmissionPenalty,
                    MaxExercises = currentCompetition.MaxExercises,
                    MaxMembers = currentCompetition.MaxMembers,
                    MaxSubmissionSize = currentCompetition.MaxSubmissionSize,
                    Status = currentCompetition.Status,
                    Duration = currentCompetition.Duration,
                    IsLoggedGroupInscribed =
                        loggedUser.GroupId is not null
                        && currentCompetition.GroupInCompetitions.Any(gic =>
                            gic.GroupId == loggedUser.GroupId
                        ),
                    CompetitionRankings = currentCompetition
                        .CompetitionRankings.Select(c => new CompetitionRankingResponse()
                        {
                            Id = c.Id,
                            Group = new GroupResponse()
                            {
                                Id = c.Group.Id,
                                LeaderId = c.Group.LeaderId,
                                Name = c.Group.Name,
                                Users = c
                                    .Group.Users.Select(u => new GenericUserInfoResponse()
                                    {
                                        Id = u.Id,
                                        Email = u.Email,
                                        Department = null,
                                        CreatedAt = u.CreatedAt,
                                        ExercisesCreated = null,
                                        JoinYear = u.JoinYear,
                                        LastLoggedAt = u.LastLoggedAt,
                                        Name = u.Name,
                                        Ra = u.Name,
                                        Group = null,
                                    })
                                    .ToList(),
                            },
                            Penalty = c.Penalty,
                            Points = c.Points,
                            RankOrder = c.RankOrder,
                        })
                        .ToList(),
                    Exercises = currentCompetition
                        .Exercices.Select(e => new ExerciseResponse()
                        {
                            Id = e.Id,
                            Title = e.Title,
                            AttachedFileId = (int)e.AttachedFileId!,
                            Description = e.Description,
                            ExerciseTypeId = e.ExerciseTypeId,
                            Inputs = e
                                .ExerciseInputs.Select(i => new ExerciseInputResponse()
                                {
                                    Id = i.Id,
                                    ExerciseId = i.Id,
                                    Input = i.Input,
                                })
                                .ToList(),
                            Outputs = e
                                .ExerciseOutputs.Select(o => new ExerciseOutputResponse()
                                {
                                    Id = o.Id,
                                    Output = o.Output,
                                    ExerciseId = o.ExerciseId,
                                    ExerciseInputId = o.ExerciseInputId,
                                })
                                .ToList(),
                        })
                        .ToList(),
                }
            );

            await base.OnConnectedAsync();
        }

        [Authorize]
        public async Task GetConnectionId()
        {
            string connectionId = this.Context.ConnectionId;

            await this.Clients.Caller.SendAsync("ReceiveConnectionId", connectionId);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var currentCompetition = await this.FetchCurrentCompetitionAsync();

            if (currentCompetition is null)
            {
                await base.OnDisconnectedAsync(exception);
                return;
            }

            ClaimsPrincipal user = this.GetHubContextUser();
            User loggedUser = this._userService.GetHttpContextLoggedUser();
            HttpContext httpContext = this.GetHubHttpContext();

            await this._logService.CreateLogAsync(
                new CreateLogRequest()
                {
                    UserId = loggedUser.Id,
                    CompetitionId = currentCompetition.Id,
                    GroupId = loggedUser.GroupId,
                    ActionType = LogType.Logout,
                    IpAddress = httpContext.Connection.RemoteIpAddress!.ToString(),
                }
            );

            await base.OnDisconnectedAsync(exception);
        }

        public async Task Ping()
        {
            await Clients.Caller.SendAsync("Pong", new { message = "Pong" });
        }

        /// <summary>
        /// Submits an exercise attempt for processing in the current competition.
        /// </summary>
        /// <remarks>This method enqueues the exercise attempt for asynchronous processing. If there is no
        /// active competition, a null response is sent back to the caller. If the group is blocked from the competition,
        /// an error message is sent back.</remarks>
        /// <param name="request">The exercise attempt details, including the group and exercise data.</param>
        /// <returns></returns>
        [Authorize(Roles = "Student")]
        public async Task SendExerciseAttempt(GroupExerciseAttemptRequest request)
        {
            Competition? currentCompetition = await this.FetchCurrentCompetitionAsync();

            if (currentCompetition is null)
            {
                await Clients.Caller.SendAsync("ReceiveExerciseAttemptResponse", null);
                return;
            }

            User loggedUser = this._userService.GetHttpContextLoggedUser();

            if (loggedUser.GroupId is null)
            {
                await Clients.Caller.SendAsync(
                    "ReceiveExerciseAttemptError",
                    new { message = "Usuário não pertence a nenhum grupo" }
                );
                return;
            }

            // Check if the group is blocked in the competition
            bool isBlocked = await this._groupInCompetitionService.IsGroupBlockedInCompetitionAsync(
                loggedUser.GroupId.Value,
                currentCompetition.Id
            );

            if (isBlocked)
            {
                await Clients.Caller.SendAsync(
                    "ReceiveExerciseAttemptError",
                    new
                    {
                        message = "Seu grupo está bloqueado de enviar submissões nesta competição",
                    }
                );
                return;
            }

            var queueItem = new ExerciseSubmissionQueueItem(
                new GroupExerciseAttemptWorkerRequest()
                {
                    GroupId = loggedUser.GroupId.Value,
                    ExerciseId = request.ExerciseId,
                    Code = request.Code,
                    LanguageType = request.LanguageType,
                },
                this.Context.ConnectionId
            );

            await _exerciseSubmissionQueue.EnqueueAsync(queueItem);
        }

        [Authorize(Roles = "Student")]
        public async Task SendCompetitionQuestion(CreateGroupQuestionRequest request)
        {
            var competition = await this.FetchCurrentCompetitionAsync();

            if (competition is null)
            {
                await this.Clients.Caller.SendAsync("ReceiveQuestionCreationResponse", null);
                return;
            }

            User loggedUser = this._userService.GetHttpContextLoggedUser();

            if (loggedUser.GroupId is null)
            {
                throw new Exception("Usuário não pertence a nenhum grupo");
            }

            Question question = await this._competitionService.CreateGroupQuestion(
                loggedUser,
                request
            );

            QuestionResponse response = new QuestionResponse()
            {
                Id = question.Id,
                CompetitionId = question.CompetitionId,
                Content = question.Content,
                QuestionType = question.QuestionType,
                User = new GenericUserInfoResponse()
                {
                    Id = question.User.Id,
                    Name = question.User.Name,
                    Email = question.User.Email,
                    CreatedAt = question.User.CreatedAt,
                    LastLoggedAt = question.User.LastLoggedAt,
                    Ra = question.User.RA,
                    JoinYear = question.User.JoinYear,
                    Department = null,
                    ExercisesCreated = null,
                },
                Answer = question.Answer is not null
                    ? new AnswerResponse()
                    {
                        Id = question.Answer.Id,
                        Content = question.Answer.Content,
                        User = new GenericUserInfoResponse()
                        {
                            Id = question.Answer.User.Id,
                            Name = question.Answer.User.Name,
                            Email = question.Answer.User.Email,
                            CreatedAt = question.Answer.User.CreatedAt,
                            LastLoggedAt = question.Answer.User.LastLoggedAt,
                            Ra = question.Answer.User.RA,
                            JoinYear = question.Answer.User.JoinYear,
                            Department = question.Answer.User.Department,
                            ExercisesCreated = null,
                        },
                    }
                    : null,
            };

            await Clients.Caller.SendAsync("ReceiveQuestionCreationResponse", response);
            await Clients.Group("Teachers").SendAsync("ReceiveQuestionCreation", response);
            await Clients.Group("Admins").SendAsync("ReceiveQuestionCreation", response);
        }

        /// <summary>
        /// Answers a question posed by a group in the competition.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin,Teacher")]
        public async Task AnswerQuestion(AnswerGroupQuestionRequest request)
        {
            var competition = await this.FetchCurrentCompetitionAsync();

            if (competition is null)
            {
                await this.Clients.Caller.SendAsync("ReceiveQuestionAnswerResponse", null);
                return;
            }

            User loggedUser = this._userService.GetHttpContextLoggedUser();

            Answer answer = await this._competitionService.AnswerGroupQuestion(loggedUser, request);

            await Clients.Caller.SendAsync("ReceiveQuestionAnswerResponse", answer);
            await Clients.Group("Teachers").SendAsync("ReceiveQuestionAnswer", answer);
            await Clients.Group("Admins").SendAsync("ReceiveQuestionAnswer", answer);
        }

        [Authorize(Roles = "Admin,Teacher")]
        public async Task ChangeJudgeSubmissionResponse(RevokeGroupSubmissionRequest request)
        {
            bool succeeded = await this._groupAttemptService.ChangeGroupExerciseAttempt(
                request.SubmissionId,
                request.NewJudgeResponse
            );

            await this.Clients.Caller.SendAsync("ReceiveChangeJudgeSubmissionResponse", succeeded);
        }

        /// <summary>
        /// Blocks a group's submission capabilities in a competition.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin,Teacher")]
        public async Task BlockGroupSubmission(BlockGroupSubmissionRequest request)
        {
            User loggedUser = this._userService.GetHttpContextLoggedUser();

            bool succeeded = false;

            try
            {
                succeeded = await this._competitionService.BlockGroupInCompetition(request);
            }
            catch (Exception exc)
            {
                this._logger.LogError(
                    exc,
                    "Erro ao bloquear grupo {GroupId} na competição {CompetitionId} pelo usuário {Name}",
                    request.GroupId,
                    request.CompetitionId,
                    loggedUser.Name
                );
            }

            if (succeeded == true)
            {
                await this._logService.CreateLogAsync(
                    new CreateLogRequest()
                    {
                        UserId = loggedUser.Id,
                        ActionType = LogType.GroupBlockedInCompetition,
                        CompetitionId = request.CompetitionId,
                        GroupId = request.GroupId,
                        IpAddress = this.GetHubHttpContext().Connection.RemoteIpAddress!.ToString(),
                    }
                );

                await this.Clients.Caller.SendAsync("ReceiveBlockGroupSubmissionResponse", true);
            }
            else
            {
                await this.Clients.Caller.SendAsync("ReceiveBlockGroupSubmissionResponse", false);
            }
        }
    }
}
