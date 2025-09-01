using ProjetoTccBackend.Database.Requests.Auth;
using ProjetoTccBackend.Swagger.Interfaces;

namespace ProjetoTccBackend.Swagger.Examples
{
    public class LoginUserRequestExample : ISwaggerExampleProvider<LoginUserRequest>
    {
        public LoginUserRequest GetExample() => new LoginUserRequest()
        {
            Ra = "000001",
            Password = "SenhaForte123"
        };
    }
}
