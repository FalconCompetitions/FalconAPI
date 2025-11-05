using Microsoft.EntityFrameworkCore;
using ProjetoTccBackend.Database;
using ProjetoTccBackend.Database.Requests.Competition;
using ProjetoTccBackend.Database.Responses.Competition;
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
                .Include(q => q.Answer)
                .ThenInclude(a => a.User)
                .FirstAsync();

            return question;
        }

        /// <inheritdoc />
        public async Task<Answer> AnswerGroupQuestion(
            User loggedUser,
            AnswerGroupQuestionRequest request
        )
        {
            Question? questionToAnswer = this._questionRepository.GetById(request.QuestionId);

            if (questionToAnswer is null)
            {
                throw new Exception("Questão não encontrada");
            }

            Answer answer = new Answer() { UserId = loggedUser.Id, Content = request.Answer };

            this._answerRepository.Add(answer);
            questionToAnswer.AnswerId = answer.Id;

            this._questionRepository.Update(questionToAnswer);

            await this._dbContext.SaveChangesAsync();

            return answer;
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
        public Task<bool> BlockGroupInCompetition(BlockGroupSubmissionRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
