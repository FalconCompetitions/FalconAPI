using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjetoTccBackend.Services.Interfaces;

namespace ProjetoTccBackend.Controllers
{
    [Authorize]
    [Route("/api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly IAttachedFileService _attachedFileService;
        private readonly ILogger<FileController> _logger;

        public FileController(
            IAttachedFileService attachedFileService,
            ILogger<FileController> logger
        )
        {
            this._attachedFileService = attachedFileService;
            this._logger = logger;
        }

        /// <summary>
        /// Retrieves a file by its unique identifier and returns it as a downloadable file stream.
        /// </summary>
        /// <remarks>The file is retrieved from an external service and returned as a stream. The caller
        /// is responsible for ensuring that the <paramref name="fileId"/> is valid and authorized for access. The
        /// file's metadata, including its name and type, is used to set the appropriate response headers for the
        /// download.</remarks>
        /// <param name="fileId">The unique identifier of the file to retrieve. This value cannot be null or empty.</param>
        /// <returns>An <see cref="IActionResult"/> representing the result of the operation: <list type="bullet">
        /// <item><description>A <see cref="FileStreamResult"/> containing the file stream if the file is
        /// found.</description></item> <item><description>A <see cref="NotFoundObjectResult"/> if the file with the
        /// specified <paramref name="fileId"/> does not exist.</description></item> <item><description>A <see
        /// cref="UnauthorizedResult"/> if access to the file is denied.</description></item> </list></returns>
        [HttpGet("{fileId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [Produces("application/pdf")]
        public async Task<IActionResult> GetFile(int fileId)
        {
            try
            {
                Tuple<string, string, string>? fileInfoTuple =
                    await this._attachedFileService.GetFileAsync(fileId);

                if (fileInfoTuple == null)
                {
                    return NotFound(new { FileId = fileId });
                }

                string fullFilePath = fileInfoTuple.Item1;
                string fileName = fileInfoTuple.Item2;
                string fileType = fileInfoTuple.Item3;

                FileStream fileStream = new FileStream(
                    fullFilePath,
                    FileMode.Open,
                    FileAccess.Read
                );

                // Explicitly set Content-Disposition header for CORS
                Response.Headers.Append("Content-Disposition", $"attachment; filename=\"{fileName}\"");

                return new FileStreamResult(fileStream, fileType)
                {
                    FileDownloadName = fileName,
                };
            }
            catch (UnauthorizedAccessException exception)
            {
                return Unauthorized();
            }
        }
    }
}
