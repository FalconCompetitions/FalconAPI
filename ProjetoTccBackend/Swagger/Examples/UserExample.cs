using ProjetoTccBackend.Models;
using ProjetoTccBackend.Swagger.Interfaces;

namespace ProjetoTccBackend.Swagger.Examples
{
    public class UserExample : ISwaggerExampleProvider<User>
    {
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
