using ProjetoTccBackend.Database.Requests.Auth;
using ProjetoTccBackend.Swagger.Interfaces;

namespace ProjetoTccBackend.Swagger.Examples
{
    public class RegisterUserRequestExample : ISwaggerExampleProvider<RegisterUserRequest>
    {
        public RegisterUserRequest GetExample() => new RegisterUserRequest()
        {
            RA = "000001",
            Name = "Usuário Exemplo",
            Email = "exemplo@email.com",
            Password = "SenhaForte123",
            JoinYear = 2024,
            Role = "Student"
        };
    }
}
