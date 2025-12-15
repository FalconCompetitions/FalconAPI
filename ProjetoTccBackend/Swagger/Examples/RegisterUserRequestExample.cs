using ProjetoTccBackend.Database.Requests.Auth;
using ProjetoTccBackend.Swagger.Interfaces;

namespace ProjetoTccBackend.Swagger.Examples
{
    /// <summary>
    /// Provides example instances of <see cref="RegisterUserRequest"/> for Swagger documentation.
    /// </summary>
    public class RegisterUserRequestExample : ISwaggerExampleProvider<RegisterUserRequest>
    {
        /// <summary>
        /// Gets an example instance of <see cref="RegisterUserRequest"/>.
        /// </summary>
        /// <returns>An example register user request.</returns>
        public RegisterUserRequest GetExample() => new RegisterUserRequest()
        {
            RA = "000001",
            Name = "Example User",
            Email = "example@email.com",
            Password = "StrongPassword123",
            JoinYear = 2024,
            Role = "Student"
        };
    }
}
