using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using ProjetoTccBackend.Database;
using ProjetoTccBackend.Exceptions;
using ProjetoTccBackend.Exceptions.AttachedFile;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Repositories.Interfaces;
using ProjetoTccBackend.Services.Interfaces;

namespace ProjetoTccBackend.Services
{
    /// <summary>
    /// Service responsible for managing attached file operations.
    /// </summary>
    public class AttachedFileService : IAttachedFileService
    {
        private readonly IUserService _userService;
        private readonly IAttachedFileRepository _attachedFileRepository;
        private readonly TccDbContext _dbContext;
        private readonly ILogger<AttachedFileService> _logger;
        private readonly string _privateFilesPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="AttachedFileService"/> class.
        /// </summary>
        /// <param name="userService">The service for user operations.</param>
        /// <param name="attachedFileRepository">The repository for attached file data access.</param>
        /// <param name="dbContext">The database context.</param>
        /// <param name="logger">Logger for registering information and errors.</param>
        public AttachedFileService(
            IUserService userService,
            IAttachedFileRepository attachedFileRepository,
            TccDbContext dbContext,
            ILogger<AttachedFileService> logger,
            IConfiguration configuration
        )
        {
            this._userService = userService;
            this._attachedFileRepository = attachedFileRepository;
            this._dbContext = dbContext;
            this._logger = logger;
            
            // Get file storage path from configuration or use default
            string storagePath = configuration.GetValue<string>("FileStorage:Path") ?? "UserUploads";
            this._privateFilesPath = Path.IsPathFullyQualified(storagePath) 
                ? storagePath 
                : Path.Combine(Directory.GetCurrentDirectory(), storagePath);
            
            this._logger.LogInformation("File storage path initialized: {Path} (Current directory: {CurrentDir})", 
                this._privateFilesPath, Directory.GetCurrentDirectory());
        }

        /// <inheritdoc />
        public async Task<AttachedFile> ProcessAndSaveFile(IFormFile file)
        {
            string validatedExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

            string originalName = Path.GetFileNameWithoutExtension(file.FileName);
            string uniqueFileName = $"{Guid.NewGuid()}{validatedExtension}";

            if (!Directory.Exists(this._privateFilesPath))
            {
                Directory.CreateDirectory(this._privateFilesPath);
            }

            string filePath = Path.Combine(this._privateFilesPath, uniqueFileName);

            try
            {
                using (FileStream fileStream = new FileStream(filePath, FileMode.CreateNew))
                {
                    await file.CopyToAsync(fileStream);
                }

                this._logger.LogInformation("File saved successfully: {FileName} -> {FilePath}", 
                    file.FileName, filePath);
            }
            catch (IOException exception)
            {
                this._logger.LogError(exception, "Failed to save file: {FileName}", file.FileName);
                throw new ErrorException(new { Target = "file", Error = exception.Message });
            }

            AttachedFile attachedFile = new AttachedFile()
            {
                FilePath = filePath, // Store full path for consistency
                Name = originalName,
                Size = file.Length,
                Type = file.ContentType,
            };

            this._attachedFileRepository.Add(attachedFile);
            await this._dbContext.SaveChangesAsync();

            return attachedFile;
        }

        public void DeleteAttachedFile(AttachedFile attachedFile)
        {
            this._attachedFileRepository.Remove(attachedFile);

            try
            {
                if (File.Exists(attachedFile.FilePath))
                {
                    File.Delete(attachedFile.FilePath);
                    this._logger.LogInformation("File deleted successfully: {FilePath}", attachedFile.FilePath);
                }
                else
                {
                    this._logger.LogWarning("File not found on disk for deletion: {FilePath}", attachedFile.FilePath);
                }
            }
            catch (IOException exception)
            {
                this._logger.LogError(exception, "Failed to delete file: {FilePath}", attachedFile.FilePath);
                throw new ErrorException(new { Target = "file", Error = exception.Message });
            }
        }

        public async Task<AttachedFile> DeleteAndReplaceExistentFile(int fileId, IFormFile newFile)
        {
            AttachedFile? existentFile = await this._attachedFileRepository.GetByIdAsync(fileId);

            if (existentFile != null)
            {
                this.DeleteAttachedFile(existentFile);
            }

            AttachedFile newSavedFile = await this.ProcessAndSaveFile(newFile);

            return newSavedFile;
        }

        /// <inheritdoc />
        public bool IsSubmittedFileValid(IFormFile file)
        {
            var permittedExtensions = new[] { ".pdf" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (string.IsNullOrEmpty(extension) || !permittedExtensions.Contains(extension))
            {
                return false;
            }

            return true;
        }

        /// <inheritdoc />
        public async Task<Tuple<string, string, string>?> GetFileAsync(int fileId)
        {
            User loggedUser = this._userService.GetHttpContextLoggedUser();

            AttachedFile? file = (
                await this._attachedFileRepository.FindAsync(f => f.Id.Equals(fileId))
            ).FirstOrDefault();

            if (file == null)
            {
                this._logger.LogWarning("AttachedFile with ID {FileId} not found in database", fileId);
                return null;
            }

            // file.FilePath should contain the full path from ProcessAndSaveFile
            string fullPath = file.FilePath;

            // If the stored path is not absolute or doesn't exist, reconstruct it
            if (!Path.IsPathFullyQualified(fullPath) || !File.Exists(fullPath))
            {
                this._logger.LogWarning("Stored file path is not valid or file doesn't exist: {StoredPath}", fullPath);
                
                // Try to reconstruct the path using current directory
                string fileName = Path.GetFileName(fullPath);
                string reconstructedPath = Path.Combine(this._privateFilesPath, fileName);
                
                this._logger.LogInformation("Attempting to use reconstructed path: {ReconstructedPath}", reconstructedPath);
                
                if (File.Exists(reconstructedPath))
                {
                    fullPath = reconstructedPath;
                    this._logger.LogInformation("File found at reconstructed path");
                }
                else
                {
                    this._logger.LogError("File not found at either stored path ({StoredPath}) or reconstructed path ({ReconstructedPath}). Current directory: {CurrentDir}, Private files path: {PrivatePath}", 
                        file.FilePath, reconstructedPath, Directory.GetCurrentDirectory(), this._privateFilesPath);
                }
            }

            // Log for debugging
            this._logger.LogInformation("Retrieving file: ID={FileId}, StoredPath={StoredPath}, FinalPath={FinalPath}, Name={FileName}, Exists={FileExists}", 
                fileId, file.FilePath, fullPath, file.Name, File.Exists(fullPath));

            return Tuple.Create(fullPath, file.Name, file.Type);
        }
    }
}
