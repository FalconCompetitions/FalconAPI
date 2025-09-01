using ProjetoTccBackend.Database.Responses.Auth;
using ProjetoTccBackend.Swagger.Interfaces;

namespace ProjetoTccBackend.Swagger.Examples
{
    public class UserResponseExample : ISwaggerExampleProvider<UserResponse>
    {
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
