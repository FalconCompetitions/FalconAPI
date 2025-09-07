using ProjetoTccBackend.Database;
using ProjetoTccBackend.Database.Requests.Competition;
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
        private readonly ICompetitionStateService _competitionStateService;
        private readonly TccDbContext _dbContext;

        public CompetitionService(
            ICompetitionRepository competitionRepository,
            IQuestionRepository questionRepository,
            ICompetitionStateService competitionStateService,
            TccDbContext dbContext
        )
        {
            this._competitionRepository = competitionRepository;
            this._questionRepository = questionRepository;
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

            Competition newCompetition = new Competition()
            {
                StartTime = request.StartTime,
                EndTime = request.EndTime,
            };

            this._competitionRepository.Add(newCompetition);
            await this._dbContext.SaveChangesAsync();

            this._competitionStateService.SignalNewCompetition();

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
        public async Task<Competition?> GetCurrentCompetition()
        {
            DateTime currentTime = DateTime.UtcNow;

            Competition? existentCompetition = this
                ._competitionRepository.Find(c =>
                    c.StartTime.Ticks <= currentTime.Ticks && c.EndTime.Ticks >= currentTime.Ticks
                )
                .FirstOrDefault();

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
        public async Task<Question> AnswerGroupQuestion(
            User loggedUser,
            AnswerGroupQuestionRequest request
        )
        {
            Question? questionToAnswer = this._questionRepository.GetById(request.QuestionId);

            if (questionToAnswer is null)
            {
                throw new Exception("Questão nao encontrada");
            }

            Question answer = new Question()
            {
                CompetitionId = questionToAnswer.CompetitionId,
                UserId = loggedUser.Id,
                TargetQuestionId = request.QuestionId,
                Content = request.Answer,
            };

            this._questionRepository.Add(answer);

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
            return await this._competitionRepository.GetOpenCompetitionsAsync();
        }
    }
}
