using ProjetoTccBackend.Models;
using ProjetoTccBackend.Swagger.Interfaces;

namespace ProjetoTccBackend.Swagger.Examples
{
    /// <summary>
    /// Provides example instances of <see cref="User"/> for Swagger documentation.
    /// </summary>
    public class UserExample : ISwaggerExampleProvider<User>
    {
        /// <summary>
        /// Gets an example instance of <see cref="User"/>.
        /// </summary>
        /// <returns>An example user.</returns>
        public User GetExample() => new User()
        {
            Id = "UUID",
            RA = "000000",
            Email = "test@email.com",
            PasswordHash = "##############",
            GroupId = 1,
        };

    }
}
