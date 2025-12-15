using Microsoft.EntityFrameworkCore;
using ProjetoTccBackend.Database.Responses.Global;
using ProjetoTccBackend.Database.Responses.Group;
using ProjetoTccBackend.Database.Responses.Question;
using ProjetoTccBackend.Database.Responses.User;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Repositories.Interfaces;
using ProjetoTccBackend.Services.Interfaces;

namespace ProjetoTccBackend.Services
{
    /// <summary>
    /// Service responsible for managing question operations.
    /// </summary>
    public class QuestionService : IQuestionService
    {
        private readonly IQuestionRepository _questionRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionService"/> class.
        /// </summary>
        /// <param name="questionRepository">The repository for question data access.</param>
        public QuestionService(IQuestionRepository questionRepository)
        {
            this._questionRepository = questionRepository;
        }

        /// <inheritdoc />
        public async Task<PagedResult<QuestionResponse>> GetQuestionsAsync(int page, int pageSize)
        {
            var query = this
                ._questionRepository.Query()
                .Include(q => q.User)
                .ThenInclude(u => u.Group)
                .ThenInclude(g => g.Users)
                .Include(q => q.Answer);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var items = await query
                .OrderBy(q => q.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new PagedResult<QuestionResponse>
            {
                Items = items.Select(q => new QuestionResponse
                {
                    Id = q.Id,
                    CompetitionId = q.CompetitionId,
                    ExerciseId = q.ExerciseId,
                    UserId = q.UserId,
                    User = new GenericUserInfoResponse()
                    {
                        Id = q.User.Id,
                        Ra = q.User.RA,
                        Email = q.User.Email!,
                        CreatedAt = q.User.CreatedAt,
                        JoinYear = q.User.JoinYear,
                        LastLoggedAt = q.User.LastLoggedAt,
                        Name = q.User.Name,
                        Group = new GroupResponse()
                        {
                            Id = q.User.Group!.Id,
                            LeaderId = q.User.Group.LeaderId,
                            Name = q.User.Group.Name,
                            Users = [],
                        },
                    },
                    Content = q.Content,
                    AnswerId = q.AnswerId,
                    Answer =
                        q.Answer != null
                            ? new AnswerResponse
                            {
                                Id = q.Answer.Id,
                                Content = q.Answer.Content,
                                UserId = q.Answer.UserId,
                            }
                            : null,
                    QuestionType = (int)q.QuestionType,
                }),
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
            };

            return result;
        }
    }
}
