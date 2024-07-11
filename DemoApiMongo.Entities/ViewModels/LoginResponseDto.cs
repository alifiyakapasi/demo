using DemoApiMongo.Entities.DataModels;

namespace DemoApiMongo.Entities.ViewModels
{
    public class LoginResponseDto
    {
        public User User { get; set; }
        public string Token { get; set; }
    }
}
