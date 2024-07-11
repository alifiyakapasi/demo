using DemoApiMongo.Entities.ViewModels;
using DemoApiMongo.Repository;
using Microsoft.AspNetCore.Mvc;

namespace DemoApiMongo.Controllers
{
    [Route("api/UsersAuth")]
    [ApiController]
    public class UsersController : Controller
    {
        private readonly  IUserRepo _userRepo;

        public UsersController(IUserRepo userRepo)
        {
            _userRepo = userRepo;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto model)
        {
            var loginresponse = await _userRepo.GetLogin(model);
            if (loginresponse.User == null || string.IsNullOrEmpty(loginresponse.Token))
                {
                    return BadRequest(new { message = "Username or Password is incorrect" });
                }
            return Ok(loginresponse);
        } 
        
        [HttpPost("registration")]
        public async Task<IActionResult> Registration([FromBody] RegistrationRequestDto model)
        {
            bool isUserNmaeunique = _userRepo.IsUserUnique(model.UserName);

            if (!isUserNmaeunique)
            {
                return BadRequest(new { message = "Username already exists." });
            }

            var register = await _userRepo.Register(model);
            if (register == null)
            {
                return BadRequest(new { message = "Error while registering" });
            }
            return Ok();
        }
    }
}
