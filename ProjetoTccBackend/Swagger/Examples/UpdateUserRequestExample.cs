using ProjetoTccBackend.Database.Requests.User;
using ProjetoTccBackend.Swagger.Interfaces;

namespace ProjetoTccBackend.Swagger.Examples
{
    /// <summary>
    /// Provides example instances of <see cref="UpdateUserRequest"/> for Swagger documentation.
    /// </summary>
    public class UpdateUserRequestExample : ISwaggerExampleProvider<UpdateUserRequest>
    {
        /// <summary>
        /// Gets an example instance of <see cref="UpdateUserRequest"/>.
        /// </summary>
        /// <returns>An example update user request.</returns>
        public UpdateUserRequest GetExample() => new UpdateUserRequest()
        {
            Name = "New Name",
            Email = "new@email.com",
            PhoneNumber = "11999999999",
            JoinYear = 2024
        };
    }
}
