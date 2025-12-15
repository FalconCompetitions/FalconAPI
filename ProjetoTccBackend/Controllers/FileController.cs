using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjetoTccBackend.Services.Interfaces;

namespace ProjetoTccBackend.Controllers
{
    /// <summary>
    /// Controller responsible for managing file operations.
    /// </summary>
    [Authorize]
    [Route("/api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly IAttachedFileService _attachedFileService;
        private readonly ILogger<FileController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileController"/> class.
        /// </summary>
        /// <param name="attachedFileService">The service responsible for file operations.</param>
        /// <param name="logger">Logger for registering information and errors.</param>
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
        [ProducesResponseType(typeof(FileStreamResult), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetFile(int fileId)
        {
            try
            {
                Tuple<string, string, string>? fileInfoTuple =
                    await this._attachedFileService.GetFileAsync(fileId);

                if (fileInfoTuple == null)
                {
                    this._logger.LogWarning("File with ID {FileId} not found in database", fileId);
                    return NotFound(new { FileId = fileId, Message = "Arquivo não encontrado" });
                }

                string fullFilePath = fileInfoTuple.Item1;
                string fileName = fileInfoTuple.Item2;
                string fileType = fileInfoTuple.Item3;

                // Check if file exists before trying to open it
                if (!System.IO.File.Exists(fullFilePath))
                {
                    this._logger.LogError("File with ID {FileId} exists in database but not found on disk at path: {FilePath}", fileId, fullFilePath);
                    return NotFound(new { FileId = fileId, Message = "Arquivo não encontrado no servidor", Path = fullFilePath });
                }

                // Set headers for file download - critical for CORS and browser handling
                Response.Headers.Append(
                    "Content-Disposition",
                    $"attachment; filename=\"{fileName}\""
                );

                this._logger.LogInformation(
                    "Successfully serving file {FileId} - {FileName} with type {FileType}",
                    fileId,
                    fileName,
                    fileType
                );

                // Return file using PhysicalFile for better performance and proper content negotiation handling
                return PhysicalFile(fullFilePath, fileType, fileName, enableRangeProcessing: true);
            }
            catch (UnauthorizedAccessException exception)
            {
                this._logger.LogError(exception, "Unauthorized access attempting to get file {FileId}", fileId);
                return Unauthorized(new { Message = "Acesso não autorizado ao arquivo" });
            }
            catch (FileNotFoundException exception)
            {
                this._logger.LogError(exception, "File {FileId} not found on disk", fileId);
                return NotFound(new { FileId = fileId, Message = "Arquivo não encontrado no servidor" });
            }
            catch (Exception exception)
            {
                this._logger.LogError(exception, "Error retrieving file {FileId}", fileId);
                return StatusCode(500, new { Message = "Erro ao recuperar o arquivo" });
            }
        }
    }
}
