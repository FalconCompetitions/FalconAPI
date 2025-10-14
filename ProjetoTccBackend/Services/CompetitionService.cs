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
        private readonly ICompetitionRepository _competitionRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly IAnswerRepository _answerRepository;
        private readonly IExerciseInCompetitionRepository _exerciseInCompetitionRepository;
        private readonly ICompetitionStateService _competitionStateService;
        private readonly TccDbContext _dbContext;

        public CompetitionService(
            ICompetitionRepository competitionRepository,
            IQuestionRepository questionRepository,
            IAnswerRepository answerRepository,
            IExerciseInCompetitionRepository exerciseInCompetitionRepository,
            ICompetitionStateService competitionStateService,
            TccDbContext dbContext
        )
        {
            this._competitionRepository = competitionRepository;
            this._questionRepository = questionRepository;
            this._answerRepository = answerRepository;
            this._exerciseInCompetitionRepository = exerciseInCompetitionRepository;
            this._competitionStateService = competitionStateService;
            this._dbContext = dbContext;
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
            newCompetition.ProcessCompetitionData(request, true);

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

            Competition? existentCompetition = (
                await this._competitionRepository.FindAsync(c =>
                    c.StartTime.Ticks <= currentTime.Ticks
                    && ((c.EndTime != null) ? c.EndTime!.Value.Ticks : 0) >= currentTime.Ticks
                )
            ).FirstOrDefault();

            return existentCompetition;
        }

        /// <inheritdoc />
        public async Task<Question> CreateGroupQuestion(
            User loggedUser,
            CreateGroupQuestionRequest request
        )
        {
            Competition? competition = await this.GetExistentCompetition();

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
                .Where(c =>
                    c.StartInscriptions != null
                    && c.EndInscriptions != null
                    && c.Status != CompetitionStatus.ModelTemplate
                )
                .Select(c => c)
                .ToListAsync();

            return validCompetitions;
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
                    MaxMembers = request.MaxMembers
                },
                false
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

            this._competitionStateService.SignalNewCompetition();

            return competition;
        }
    }
}
