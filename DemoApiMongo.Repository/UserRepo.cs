using DemoApiMongo.Configuration;
using DemoApiMongo.Entities.DataModels;
using DemoApiMongo.Entities.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DemoApiMongo.Repository
{
    public class UserRepo : IUserRepo
    {

        private readonly IMongoCollection<User> _user;
        private string secretkey;

        public UserRepo(
         IOptions<ProductDBSettings> productDatabaseSetting, IConfiguration configuration)
        {
            var mongoClient = new MongoClient(productDatabaseSetting.Value.ConnectionString);
            var database = mongoClient.GetDatabase(productDatabaseSetting.Value.DatabaseName);
            // _user = database.GetCollection<User>(productDatabaseSetting.Value.CollectionName);
            _user = database.GetCollection<User>("User");
            secretkey = configuration.GetValue<string>("ApiSettings:Secret");
        }
        public bool IsUserUnique(string username)
        {
            var user = _user.Find(x=> x.UserName == username).FirstOrDefaultAsync();
            if (user.Result == null)
            {
                return true;
            }  
            return false;
        }

        public async Task<LoginResponseDto> GetLogin(LoginRequestDto logindto)
        {
           var login =  await _user.Find(x=>x.UserName.ToLower() == logindto.UserName.ToLower() && x.Password == logindto.Password).FirstOrDefaultAsync();

            if(login == null)
            {
                return new LoginResponseDto()
                {
                    Token ="",
                    User = null
                };
            }

            // If user found generate JWT token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secretkey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                 Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, login.Id.ToString()),
                    new Claim(ClaimTypes.Role, login.Role)
                }),
                 Expires = DateTime.UtcNow.AddDays(7),
                 SigningCredentials = new (new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            LoginResponseDto loginResponseDto = new LoginResponseDto()
            {
                Token = tokenHandler.WriteToken(token),
                User = login

            };
            return loginResponseDto;
        }   

        public async Task<User> Register(RegistrationRequestDto registration)
        {
            User user = new()
            {
                Id = Guid.NewGuid().ToString(),
                UserName = registration.UserName,
                Password = registration.Password,
                Name = registration.Name,
                Role = registration.Role,
            };
            await _user.InsertOneAsync(user);
            // user.Password = ""; // dont show password when returning i.e empty the password
            return user;

        }
    }
}
