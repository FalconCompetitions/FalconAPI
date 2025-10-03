using ProjetoTccBackend.Database;
using ProjetoTccBackend.Exceptions;
using ProjetoTccBackend.Exceptions.AttachedFile;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Repositories.Interfaces;
using ProjetoTccBackend.Services.Interfaces;

namespace ProjetoTccBackend.Services
{
    public class AttachedFileService : IAttachedFileService
    {
        private readonly IAttachedFileRepository _attachedFileRepository;
        private readonly TccDbContext _dbContext;
        private readonly ILogger<AttachedFileService> _logger;

        public AttachedFileService(
            IAttachedFileRepository attachedFileRepository,
            TccDbContext dbContext,
            ILogger<AttachedFileService> logger
        )
        {
            this._attachedFileRepository = attachedFileRepository;
            this._dbContext = dbContext;
            this._logger = logger;
        }

        /// <inheritdoc />
        public async Task<AttachedFile> ProcessAndSaveFile(IFormFile file)
        {
            string validatedExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

            string originalName = Path.GetFileNameWithoutExtension(file.FileName);
            string uniqueFileName = $"{Guid.NewGuid()}{validatedExtension}";

            string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "AttachedFiles");
            Directory.CreateDirectory(uploadPath);

            string filePath = Path.Combine(uploadPath, uniqueFileName);

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
                size = file.Length,
                Type = file.ContentType,
            };

            this._attachedFileRepository.Add(attachedFile);
            await this._dbContext.SaveChangesAsync();

            return attachedFile;
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
    }
}
