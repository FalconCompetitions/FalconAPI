using ProjetoTccBackend.Exceptions;
using ProjetoTccBackend.Models;

namespace ProjetoTccBackend.Services.Interfaces
{
    public interface IAttachedFileService
    {
        /// <summary>
        /// Processes the provided file, saves it to the server, and creates a corresponding record in the database.
        /// </summary>
        /// <remarks>The method validates the file extension, generates a unique file name, and saves the
        /// file to a predefined directory. It also creates a database record for the file and commits the changes to
        /// the database.</remarks>
        /// <param name="file">The file to be processed and saved. Must not be null.</param>
        /// <returns>An <see cref="AttachedFile"/> object representing the saved file, including its metadata such as file path,
        /// name, size, and type.</returns>
        /// <exception cref="ErrorException">Thrown if an I/O error occurs while saving the file to the server.</exception>
        Task<AttachedFile> ProcessAndSaveFile(IFormFile file);

        /// <summary>
        /// Determines whether the submitted file is valid based on its extension.
        /// </summary>
        /// <param name="file">The file to validate. Must not be null.</param>
        /// <returns><see langword="true"/> if the file has a permitted extension; otherwise, <see langword="false"/>.</returns>
        bool IsSubmittedFileValid(IFormFile file);
    }
}
