using ProjetoTccBackend.Database.Responses.Global;
using ProjetoTccBackend.Database.Responses.Question;

namespace ProjetoTccBackend.Services.Interfaces
{
    public interface IQuestionService
    {
        /// <summary>
        /// Retrieves a paginated list of questions, including their associated answers, if available.
        /// </summary>
        /// <remarks>This method queries the underlying data source for questions and their associated
        /// answers, if any. The results are ordered by question ID in ascending order. Pagination is applied based on
        /// the specified <paramref name="page"/> and <paramref name="pageSize"/> parameters.</remarks>
        /// <param name="page">The page number to retrieve. Must be greater than or equal to 1.</param>
        /// <param name="pageSize">The number of items to include per page. Must be greater than 0.</param>
        /// <returns>A <see cref="PagedResult{T}"/> containing a collection of <see cref="QuestionResponse"/> objects for the
        /// specified page, along with pagination metadata such as total count and total pages.</returns>
        Task<PagedResult<QuestionResponse>> GetQuestionsAsync(int page, int pageSize);
    }
}
