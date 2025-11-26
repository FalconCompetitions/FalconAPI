using ProjetoTccBackend.Database.Requests.Auth;
using ProjetoTccBackend.Swagger.Interfaces;

namespace ProjetoTccBackend.Swagger.Examples
{
    /// <summary>
    /// Provides example instances of <see cref="LoginUserRequest"/> for Swagger documentation.
    /// </summary>
    public class LoginUserRequestExample : ISwaggerExampleProvider<LoginUserRequest>
    {
        /// <summary>
        /// Gets an example instance of <see cref="LoginUserRequest"/>.
        /// </summary>
        /// <returns>An example login user request.</returns>
        public LoginUserRequest GetExample() => new LoginUserRequest()
        {
            Ra = "000001",
            Password = "StrongPassword123"
        };
    }
}
