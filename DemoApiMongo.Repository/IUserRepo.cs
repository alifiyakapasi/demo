using DemoApiMongo.Entities.DataModels;
using DemoApiMongo.Entities.ViewModels;

namespace DemoApiMongo.Repository
{
    public interface IUserRepo
    {
        bool IsUserUnique (string username);

        Task<LoginResponseDto> GetLogin (LoginRequestDto login);

        Task<User> Register(RegistrationRequestDto registration);
    }
}
