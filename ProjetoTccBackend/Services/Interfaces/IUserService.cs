using ProjetoTccBackend.Database.Requests.Auth;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Exceptions;

namespace ProjetoTccBackend.Services.Interfaces
{
    public interface IUserService
    {
        /// <summary>
        /// Retrieves the currently logged-in user from the HTTP context.
        /// </summary>
        /// <returns>The logged-in <see cref="User"/> object.</returns>
        /// <exception cref="UnauthorizedAccessException">
        /// Thrown when the user is not authenticated or the user ID cannot be found in the claims.
        /// </exception>
        User GetHttpContextLoggerUser();

        /// <summary>
        /// Registers a new user in the system.
        /// </summary>
        /// <param name="user">The <see cref="RegisterUserRequest"/> object containing the user's registration details.</param>
        /// <returns>The registered <see cref="User"/> object and a string representing the role assigned to the user.</returns>
        /// <exception cref="FormException">
        /// Thrown when the email is already in use or if there are errors during user creation.
        /// </exception>
        Task<Tuple<User, string>> RegisterUserAsync(RegisterUserRequest request);

        /// <summary>
        /// Authenticates a user using their email and password.
        /// </summary>
        /// <param name="usr">The <see cref="LoginUserRequest"/> object containing the user's login credentials.</param>
        /// <returns>The authenticated <see cref="User"/> object.</returns>
        /// <exception cref="FormException">
        /// Thrown when the email does not exist in the system or the password is incorrect.
        /// </exception>
        Task<Tuple<User, string>> LoginUserAsync(LoginUserRequest request);


        /// <summary>
        /// Retrieves a user by their unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the user to retrieve.</param>
        /// <returns>The user with the specified identifier, or null if no user is found.</returns>
        Task<User?> GetUser(string id);


        /// <summary>
        /// Retrieves a list of all users in the system.
        /// </summary>
        /// <returns>A list of all users.</returns>
        Task<List<User>> GetAllUsers();


        /// <summary>
        /// Logs the user out of the application asynchronously.
        /// </summary>
        /// <remarks>This method clears the user's session and any associated authentication tokens. It
        /// should be called when the user explicitly requests to log out or when the application needs to terminate the
        /// user's session for security or other reasons.</remarks>
        /// <returns>A task that represents the asynchronous logout operation.</returns>
        Task LogoutAsync();
    }
}
