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

        void DeleteAttachedFile(AttachedFile attachedFile);


        Task<AttachedFile> DeleteAndReplaceExistentFile(int fileId, IFormFile newFile);

        /// <summary>
        /// Determines whether the submitted file is valid based on its extension.
        /// </summary>
        /// <param name="file">The file to validate. Must not be null.</param>
        /// <returns><see langword="true"/> if the file has a permitted extension; otherwise, <see langword="false"/>.</returns>
        bool IsSubmittedFileValid(IFormFile file);


        /// <summary>
        /// Retrieves the file information associated with the specified file ID.
        /// </summary>
        /// <remarks>The method retrieves the file information for the currently logged-in user. The
        /// returned tuple  includes the full path to the file on the server, the file's original name, and its MIME
        /// type.</remarks>
        /// <param name="fileId">The unique identifier of the file to retrieve. This parameter cannot be null or empty.</param>
        /// <returns>A <see cref="Tuple{T1, T2, T3}"/> containing the full file path, file name, and file type,  or <see
        /// langword="null"/> if no file is found for the specified <paramref name="fileId"/>.</returns>
        Task<Tuple<string, string, string>?> GetFileAsync(string fileId);
    }
}
