using ProjetoTccBackend.Database.Responses.Auth;
using ProjetoTccBackend.Swagger.Interfaces;

namespace ProjetoTccBackend.Swagger.Examples
{
    /// <summary>
    /// Provides example instances of <see cref="UserResponse"/> for Swagger documentation.
    /// </summary>
    public class UserResponseExample : ISwaggerExampleProvider<UserResponse>
    {
        /// <summary>
        /// Gets an example instance of <see cref="UserResponse"/>.
        /// </summary>
        /// <returns>An example user response.</returns>
        public UserResponse GetExample() => new UserResponse()
        {
            Id = "UUID",
            Email = "exemplo@email.com",
            EmailConfirmed = true,
            Name = "Usuário Exemplo",
            JoinYear = 2024,
            PhoneNumber = "11999999999",
            PhoneNumberConfirmed = true,
            RA = "000001",
            Role = "Student"
        };
    }
}
