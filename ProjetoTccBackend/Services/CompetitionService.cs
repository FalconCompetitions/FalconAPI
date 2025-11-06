using Microsoft.EntityFrameworkCore;
using ProjetoTccBackend.Database;
using ProjetoTccBackend.Database.Requests.Competition;
using ProjetoTccBackend.Database.Responses.Competition;
using ProjetoTccBackend.Database.Responses.User;
using ProjetoTccBackend.Enums.Competition;
using ProjetoTccBackend.Exceptions;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Repositories.Interfaces;
using ProjetoTccBackend.Services.Interfaces;

namespace ProjetoTccBackend.Services
{
    public class CompetitionService : ICompetitionService
    {
        private readonly IUserService _userService;
        private readonly ICompetitionRepository _competitionRepository;
        private readonly IGroupInCompetitionRepository _groupInCompetitionRepository;
        private readonly ICompetitionRankingRepository _competitionRankingRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly IAnswerRepository _answerRepository;
        private readonly IExerciseInCompetitionRepository _exerciseInCompetitionRepository;
        private readonly ICompetitionStateService _competitionStateService;
        private readonly TccDbContext _dbContext;
        private readonly ILogger<CompetitionService> _logger;

        public CompetitionService(
            IUserService userService,
            ICompetitionRepository competitionRepository,
            IGroupInCompetitionRepository groupInCompetitionRepository,
            ICompetitionRankingRepository competitionRankingRepository,
            IQuestionRepository questionRepository,
            IAnswerRepository answerRepository,
            IExerciseInCompetitionRepository exerciseInCompetitionRepository,
            ICompetitionStateService competitionStateService,
            TccDbContext dbContext,
            ILogger<CompetitionService> logger
        )
        {
            this._userService = userService;
            this._competitionRepository = competitionRepository;
            this._groupInCompetitionRepository = groupInCompetitionRepository;
            this._competitionRankingRepository = competitionRankingRepository;
            this._questionRepository = questionRepository;
            this._answerRepository = answerRepository;
            this._exerciseInCompetitionRepository = exerciseInCompetitionRepository;
            this._competitionStateService = competitionStateService;
            this._dbContext = dbContext;
            this._logger = logger;
        }

        /// <inheritdoc/>
        public async Task<Competition> CreateCompetition(CompetitionRequest request)
        {
            Competition? existentCompetition = this
                ._competitionRepository.Find(c => c.StartTime.Date.Equals(request.StartTime.Date))
                .FirstOrDefault();

            if (existentCompetition is not null)
            {
                throw new ExistentCompetitionException();
            }

            Competition newCompetition = new Competition();
            newCompetition.ProcessCompetitionData(request, true, true);

            this._competitionRepository.Add(newCompetition);

            if (request.ExerciseIds.Count > 0)
            {
                await this._exerciseInCompetitionRepository.AddRangeAsync(
                    request.ExerciseIds.Select(e => new ExerciseInCompetition()
                    {
                        CompetitionId = newCompetition.Id,
                        ExerciseId = e,
                    })
                );
            }

            await this._dbContext.SaveChangesAsync();

            return newCompetition;
        }

        /// <inheritdoc/>
        public async Task<Competition?> GetExistentCompetition()
        {
            Competition? existentCompetition = this
                ._competitionRepository.Find(c => c.StartTime.Ticks > DateTime.UtcNow.Ticks)
                .FirstOrDefault();

            return existentCompetition;
        }

        /// <inheritdoc />
        public async Task<ICollection<CompetitionResponse>> GetCreatedTemplateCompetitions()
        {
            List<Competition> templateCompetitions = await this
                ._competitionRepository.Query()
                .Include(c => c.Exercices)
                .Where(c => c.Status == CompetitionStatus.ModelTemplate)
                .ToListAsync();

            List<CompetitionResponse> response = templateCompetitions
                .Select(c => new CompetitionResponse()
                {
                    Id = c.Id,
                    Name = c.Name,
                    BlockSubmissions = c.BlockSubmissions,
                    Duration = c.Duration,
                    StartInscriptions = c.StartInscriptions,
                    EndInscriptions = c.EndInscriptions,
                    StartTime = c.StartTime,
                    EndTime = c.EndTime,
                    ExerciseIds = c.Exercices.Select(e => e.Id).ToList(),
                    MaxExercises = c.MaxExercises,
                    MaxSubmissionSize = c.MaxSubmissionSize,
                    Status = c.Status,
                    StopRanking = c.StopRanking,
                    SubmissionPenalty = c.SubmissionPenalty,
                    Description = c.Description,
                    MaxMembers = c.MaxMembers,
                })
                .ToList();

            return response;
        }

        /// <inheritdoc />
        public async Task<Competition?> GetCurrentCompetition()
        {
            DateTime currentTime = DateTime.UtcNow;

            Competition? existentCompetition = await this
                ._competitionRepository.Query()
                .AsSplitQuery()
                .Include(c => c.Exercices)
                .ThenInclude(e => e.ExerciseInputs)
                .Include(c => c.Exercices)
                .ThenInclude(e => e.ExerciseOutputs)
                .Include(c => c.Groups)
                .ThenInclude(g => g.Users)
                .Include(c => c.CompetitionRankings)
                .ThenInclude(c => c.Group)
                .ThenInclude(g => g.Users)
                .Include(c => c.GroupInCompetitions)
                .Where(c =>
                    c.StartInscriptions <= currentTime
                    && c.EndTime >= currentTime
                    && c.Status == CompetitionStatus.Ongoing
                )
                .FirstOrDefaultAsync();

            return existentCompetition;
        }

        /// <inheritdoc />
        public async Task<Question> CreateGroupQuestion(
            User loggedUser,
            CreateGroupQuestionRequest request
        )
        {
            Competition? competition = await this.GetCurrentCompetition();

            if (competition is null)
            {
                throw new ExistentCompetitionException();
            }

            Question question = new Question()
            {
                CompetitionId = competition.Id,
                ExerciseId = request.ExerciseId,
                QuestionType = request.QuestionType,
                Content = request.Content,
                UserId = loggedUser.Id,
            };

            this._questionRepository.Add(question);

            await this._dbContext.SaveChangesAsync();

            question = await this
                ._questionRepository.Query()
                .Where(q => q.Id == question.Id)
                .Include(q => q.User)
                    .ThenInclude(u => u.Group!)
                .Include(q => q.Answer)
                    .ThenInclude(a => a!.User)
                .FirstAsync();

            return question;
        }

        /// <inheritdoc />
        public async Task<AnswerResponse> AnswerGroupQuestion(
            User loggedUser,
            AnswerGroupQuestionRequest request
        )
        {
            Question? questionToAnswer = this._questionRepository.GetById(request.QuestionId);

            if (questionToAnswer is null)
            {
                throw new Exception("Questão não encontrada");
            }

            // Create and save the answer first to generate its ID
            Answer answer = new Answer() { UserId = loggedUser.Id, Content = request.Answer };
            this._answerRepository.Add(answer);
            await this._dbContext.SaveChangesAsync();

            // Now we can safely set the AnswerId foreign key
            questionToAnswer.AnswerId = answer.Id;
            this._questionRepository.Update(questionToAnswer);
            await this._dbContext.SaveChangesAsync();

            // Return AnswerResponse DTO instead of Answer model
            return new AnswerResponse()
            {
                Id = answer.Id,
                Content = answer.Content,
                QuestionId = questionToAnswer.Id,
                User = new GenericUserInfoResponse()
                {
                    Id = loggedUser.Id,
                    Name = loggedUser.Name,
                    Email = loggedUser.Email!,
                    CreatedAt = loggedUser.CreatedAt,
                    LastLoggedAt = loggedUser.LastLoggedAt,
                    Ra = loggedUser.RA,
                    JoinYear = loggedUser.JoinYear,
                    Department = loggedUser.Department,
                    ExercisesCreated = null,
                }
            };
        }

        /// <inheritdoc />
        public async Task OpenCompetitionInscriptionsAsync(Competition competition)
        {
            competition.ChangeCompetitionStatus(CompetitionStatus.OpenInscriptions);
            this._competitionRepository.Update(competition);

            await this._dbContext.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task CloseCompetitionInscriptionsAsync(Competition competition)
        {
            competition.ChangeCompetitionStatus(CompetitionStatus.ClosedInscriptions);
            this._competitionRepository.Update(competition);

            await this._dbContext.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task StartCompetitionAsync(Competition competition)
        {
            competition.ChangeCompetitionStatus(CompetitionStatus.Ongoing);
            this._competitionRepository.Update(competition);

            await this._dbContext.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task EndCompetitionAsync(Competition competition)
        {
            competition.ChangeCompetitionStatus(CompetitionStatus.Finished);
            this._competitionRepository.Update(competition);

            await this._dbContext.SaveChangesAsync();

            this._competitionStateService.SignalNoActiveCompetitions();
        }

        /// <inheritdoc />
        public async Task<ICollection<Competition>> GetOpenCompetitionsAsync()
        {
            List<Competition> validCompetitions = await this
                ._competitionRepository.Query()
                .Where(c => c.Status != CompetitionStatus.Finished)
                .Select(c => c)
                .ToListAsync();

            return validCompetitions;
        }

        /// <inheritdoc />
        public async Task<ICollection<Competition>> GetOpenSubscriptionCompetitionsAsync()
        {
            List<Competition> competitions = await this
                ._competitionRepository.Query()
                .Where(c => c.Status == CompetitionStatus.OpenInscriptions)
                .ToListAsync();

            return competitions;
        }

        /// <inheritdoc />
        public async Task<Competition?> UpdateCompetitionAsync(
            int id,
            UpdateCompetitionRequest request
        )
        {
            var competition = await this._dbContext.Competitions.FindAsync(id);
            if (competition == null)
            {
                return null;
            }

            competition.ProcessCompetitionData(
                new CompetitionRequest()
                {
                    StartTime = request.StartTime,
                    BlockSubmissions = request.BlockSubmissions,
                    Duration = request.Duration,
                    ExerciseIds = request.ExerciseIds,
                    MaxExercises = request.MaxExercises,
                    MaxSubmissionSize = request.MaxSubmissionSize,
                    Name = request.Name,
                    StopRanking = request.StopRanking,
                    SubmissionPenalty = request.SubmissionPenalty,
                    Description = request.Description,
                    MaxMembers = request.MaxMembers,
                    StartInscriptions = request.StartInscriptions,
                    EndInscriptions = request.EndInscriptions,
                },
                false,
                true
            );
            competition.ChangeCompetitionStatus(CompetitionStatus.Pending);

            List<int> currentExerciseIds = await this
                ._exerciseInCompetitionRepository.Query()
                .Where(x => x.CompetitionId.Equals(competition.Id))
                .Select(x => x.ExerciseId)
                .ToListAsync();

            List<int> newExerciseIds = request.ExerciseIds.ToList();

            List<int> exerciseIdsToAdd = newExerciseIds.Except(currentExerciseIds).ToList();
            List<int> exerciseIdsToDelete = currentExerciseIds.Except(newExerciseIds).ToList();

            await this
                ._exerciseInCompetitionRepository.Query()
                .Where(x =>
                    x.CompetitionId.Equals(competition.Id)
                    && exerciseIdsToDelete.Contains(x.ExerciseId)
                )
                .ExecuteDeleteAsync();

            IEnumerable<ExerciseInCompetition> newExercisesEntities = exerciseIdsToAdd.Select(
                exerciseId => new ExerciseInCompetition()
                {
                    CompetitionId = competition.Id,
                    ExerciseId = exerciseId,
                }
            );
            await this._exerciseInCompetitionRepository.AddRangeAsync(newExercisesEntities);

            await this._dbContext.SaveChangesAsync();

            if (
                (
                    competition.Status == CompetitionStatus.Pending
                    || competition.Status == CompetitionStatus.Ongoing
                )
                && competition.StartTime >= DateTime.UtcNow
            )
            {
                this._competitionStateService.SignalNewCompetition();
            }

            return competition;
        }

        /// <inheritdoc />
        public async Task<GroupInCompetition> InscribeGroupInCompetition(
            InscribeGroupToCompetitionRequest request
        )
        {
            User loggedUser = this._userService.GetHttpContextLoggedUser();

            if (loggedUser.Id != loggedUser.Group!.LeaderId)
            {
                throw new UserIsNotLeaderException();
            }

            Competition? competition = await this
                ._competitionRepository.Query()
                .Include(c => c.GroupInCompetitions)
                .Where(c => c.Id == request.CompetitionId)
                .FirstOrDefaultAsync();

            if (competition == null)
            {
                throw new NotExistentCompetitionException();
            }

            if (
                competition.GroupInCompetitions.Count(g =>
                    g.CompetitionId == request.CompetitionId && g.GroupId == request.GroupId
                ) > 0
            )
            {
                throw new AlreadyInCompetitionException();
            }

            DateTime now = DateTime.UtcNow;

            if (competition.StartInscriptions > now || competition.EndInscriptions < now)
            {
                throw new NotValidCompetitionException();
            }

            if (loggedUser.Group.Users.Count > competition.MaxMembers)
            {
                throw new MaxMembersExceededException();
            }

            GroupInCompetition groupInCompetition = new GroupInCompetition()
            {
                CompetitionId = competition.Id,
                GroupId = loggedUser.Group!.Id,
            };

            await this._groupInCompetitionRepository.AddAsync(groupInCompetition);

            int rankingCount = await this
                ._competitionRankingRepository.Query()
                .Where(c => c.CompetitionId == competition.Id)
                .CountAsync();

            CompetitionRanking competitionRanking = new CompetitionRanking()
            {
                CompetitionId = competition.Id,
                GroupId = loggedUser.Group.Id,
                RankOrder = rankingCount + 1,
                Points = 0,
                Penalty = 0,
            };

            await this._dbContext.SaveChangesAsync();

            return groupInCompetition;
        }

        /// <inheritdoc />
        /// <summary>
        /// Blocks a group from submitting exercises in a specific competition.
        /// </summary>
        /// <param name="request">The request containing group and competition IDs.</param>
        /// <returns>True if the group was successfully blocked, false otherwise.</returns>
        public async Task<bool> BlockGroupInCompetition(BlockGroupSubmissionRequest request)
        {
            var groupInCompetition = await this._dbContext.GroupsInCompetitions
                .FirstOrDefaultAsync(gic => gic.GroupId == request.GroupId && gic.CompetitionId == request.CompetitionId);

            if (groupInCompetition == null)
            {
                return false;
            }

            groupInCompetition.Blocked = true;
            await this._dbContext.SaveChangesAsync();

            return true;
        }

        /// <inheritdoc />
        public async Task<ICollection<Question>> GetAllCompetitionQuestionsAsync(int competitionId)
        {
            var questions = await this._questionRepository
                .Query()
                .Include(q => q.User)
                    .ThenInclude(u => u.Group!)
                .Include(q => q.Answer)
                    .ThenInclude(a => a!.User)
                .Where(q => q.CompetitionId == competitionId)
                .OrderBy(q => q.Id)
                .ToListAsync();

            return questions;
        }

        /// <inheritdoc />
        public async Task<ICollection<CompetitionRankingResponse>> GetCompetitionRankingAsync(int competitionId)
        {
            var rankings = await this._competitionRankingRepository
                .Query()
                .Include(r => r.Group)
                    .ThenInclude(g => g.Users)
                .Where(r => r.CompetitionId == competitionId)
                .OrderBy(r => r.RankOrder)
                .ToListAsync();

            // Get all attempts for the competition to calculate exercise attempts
            var allAttempts = await this._dbContext.GroupExerciseAttempts
                .Where(a => a.CompetitionId == competitionId)
                .ToListAsync();

            var rankingResponses = rankings.Select(r =>
            {
                // Get attempts for this group
                var groupAttempts = allAttempts
                    .Where(a => a.GroupId == r.GroupId)
                    .GroupBy(a => a.ExerciseId)
                    .Select(g => new Database.Responses.Competition.GroupExerciseAttemptResponse()
                    {
                        GroupId = r.GroupId,
                        ExerciseId = g.Key,
                        Attempts = g.Count()
                    }).ToList();

                return new CompetitionRankingResponse()
                {
                    Id = r.Id,
                    Group = new Database.Responses.Group.GroupResponse()
                    {
                        Id = r.Group.Id,
                        LeaderId = r.Group.LeaderId,
                        Name = r.Group.Name,
                        Users = r.Group.Users.Select(u => new Database.Responses.User.GenericUserInfoResponse()
                        {
                            Id = u.Id,
                            Email = u.Email!,
                            Department = null,
                            CreatedAt = u.CreatedAt,
                            ExercisesCreated = null,
                            JoinYear = u.JoinYear,
                            LastLoggedAt = u.LastLoggedAt,
                            Name = u.Name,
                            Ra = u.RA,
                            Group = null,
                        }).ToList(),
                    },
                    Penalty = r.Penalty,
                    Points = r.Points,
                    RankOrder = r.RankOrder,
                    ExerciseAttempts = groupAttempts,
                };
            }).ToList();

            return rankingResponses;
        }

        /// <summary>
        /// Retrieves all exercise submissions for a specific competition, including group and exercise details.
        /// </summary>
        /// <param name="competitionId">The ID of the competition.</param>
        /// <returns>A collection of <see cref="GroupExerciseAttempt"/> objects ordered by submission time (most recent first).</returns>
        public async Task<ICollection<GroupExerciseAttempt>> GetCompetitionSubmissionsAsync(int competitionId)
        {
            var submissions = await this._dbContext.GroupExerciseAttempts
                .Include(a => a.Group)
                    .ThenInclude(g => g.Users)
                .Include(a => a.Exercise)
                .Where(a => a.CompetitionId == competitionId)
                .OrderByDescending(a => a.SubmissionTime)
                .ToListAsync();

            return submissions;
        }

        /// <inheritdoc />
        public async Task<bool> UpdateCompetitionSettingsAsync(UpdateCompetitionSettingsRequest request)
        {
            try
            {
                var competition = await this._competitionRepository
                    .Query()
                    .FirstOrDefaultAsync(c => c.Id == request.CompetitionId);

                if (competition is null)
                {
                    this._logger.LogWarning("Competition with ID {CompetitionId} not found", request.CompetitionId);
                    return false;
                }

                // Check if competition is finished
                if (competition.Status == CompetitionStatus.Finished)
                {
                    this._logger.LogWarning("Cannot update settings for finished competition {CompetitionId}", request.CompetitionId);
                    return false;
                }

                // Convert seconds to TimeSpan
                competition.Duration = TimeSpan.FromSeconds(request.Duration);
                competition.SubmissionPenalty = TimeSpan.FromSeconds(request.SubmissionPenalty);
                competition.MaxSubmissionSize = request.MaxSubmissionSize;

                // Calculate EndTime based on StartTime + Duration
                competition.EndTime = competition.StartTime.Add(competition.Duration);

                // Calculate BlockSubmissions and StopRanking based on EndTime
                if (competition.EndTime.HasValue)
                {
                    competition.BlockSubmissions = competition.EndTime.Value.AddSeconds(-request.StopSubmissionsBeforeEnd);
                    competition.StopRanking = competition.EndTime.Value.AddSeconds(-request.StopRankingBeforeEnd);
                }

                this._competitionRepository.Update(competition);
                await this._dbContext.SaveChangesAsync();

                this._logger.LogInformation("Competition {CompetitionId} settings updated successfully", request.CompetitionId);
                return true;
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Error updating competition {CompetitionId} settings", request.CompetitionId);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<bool> StopCompetitionAsync(int competitionId)
        {
            try
            {
                var competition = await this._competitionRepository
                    .Query()
                    .FirstOrDefaultAsync(c => c.Id == competitionId);

                if (competition is null)
                {
                    this._logger.LogWarning("Competition with ID {CompetitionId} not found", competitionId);
                    return false;
                }

                // Check if competition is already finished
                if (competition.Status == CompetitionStatus.Finished)
                {
                    this._logger.LogWarning("Competition {CompetitionId} is already finished", competitionId);
                    return false;
                }

                // Set EndTime to now and update status
                var now = DateTime.UtcNow;
                competition.EndTime = now;
                competition.BlockSubmissions = now;
                competition.StopRanking = now;
                competition.Status = CompetitionStatus.Finished;

                this._competitionRepository.Update(competition);
                await this._dbContext.SaveChangesAsync();

                this._logger.LogInformation("Competition {CompetitionId} stopped successfully", competitionId);
                return true;
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Error stopping competition {CompetitionId}", competitionId);
                return false;
            }
        }
    }
}
