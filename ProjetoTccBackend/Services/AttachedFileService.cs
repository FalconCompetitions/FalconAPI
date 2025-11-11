using ProjetoTccBackend.Database;
using ProjetoTccBackend.Exceptions;
using ProjetoTccBackend.Exceptions.AttachedFile;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Repositories.Interfaces;
using ProjetoTccBackend.Services.Interfaces;
using System.Threading.Tasks;

namespace ProjetoTccBackend.Services
{
    public class AttachedFileService : IAttachedFileService
    {
        private readonly IUserService _userService;
        private readonly IAttachedFileRepository _attachedFileRepository;
        private readonly TccDbContext _dbContext;
        private readonly ILogger<AttachedFileService> _logger;
        private readonly string _privateFilesPath;

        public AttachedFileService(
            IUserService userService,
            IAttachedFileRepository attachedFileRepository,
            TccDbContext dbContext,
            ILogger<AttachedFileService> logger
        )
        {
            this._userService = userService;
            this._attachedFileRepository = attachedFileRepository;
            this._dbContext = dbContext;
            this._logger = logger;
            this._privateFilesPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "UserUploads"
            );
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
            }
            catch (IOException exception)
            {
                throw new ErrorException(new { Target = "file", Error = exception.Message });
            }

            AttachedFile attachedFile = new AttachedFile()
            {
                FilePath = filePath,
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
                File.Delete(attachedFile.FilePath);
            }
            catch (IOException exception)
            {
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
                return null;
            }

            string fullPath = Path.Combine(this._privateFilesPath, file.FilePath);

            return Tuple.Create(fullPath, file.Name, file.Type);
        }
    }
}
