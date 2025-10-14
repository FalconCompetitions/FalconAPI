using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProjetoTccBackend.Services.Interfaces;

namespace ProjetoTccBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionController : ControllerBase
    {
        private readonly IQuestionService _questionService;

        public QuestionController(IQuestionService questionService)
        {
            this._questionService = questionService;
        }

        /// <summary>
        /// Retrieves a paginated list of questions with their answers (if any).
        /// </summary>
        /// <param name="page">The page number to retrieve. Default is 1.</param>
        /// <param name="pageSize">The number of questions per page. Default is 10.</param>
        /// <returns>An <see cref="IActionResult"/> containing a paginated list of questions.</returns>
        /// <remarks>
        /// Exemplo de uso:
        /// <code>
        ///     GET /api/question?page=1&pageSize=10
        /// </code>
        /// </remarks>
        /// <response code="200">Returns the paginated list of questions.</response>
        [HttpGet()]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetQuestions(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10
        )
        {
            var result = await this._questionService.GetQuestionsAsync(page, pageSize);
            return Ok(result);
        }
    }
}
