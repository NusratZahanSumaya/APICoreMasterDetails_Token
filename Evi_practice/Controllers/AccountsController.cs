using Evi_practice.DTO;
using Evi_practice.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.AccessControl;
using System.Security.Claims;

namespace Evi_practice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService tokenService;
        public AccountsController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ITokenService tokenService)
        {
            this._signInManager = signInManager;
            this._userManager = userManager;
            this.tokenService = tokenService;

        }
        public async Task<List<ApplicationUser>> Getuser()
        {
            return await _userManager.Users.ToListAsync();
        }
        [HttpPost]
        public async Task<IActionResult> Register(UserDTO entity)
        {

            var user = new ApplicationUser
            {
                Email = entity.Email,
                UserName = entity.Email,

            };
            IdentityResult result = await _userManager.CreateAsync(user, entity.Password);
            if (result.Succeeded)
            {
                return Created("", user);
            }
            else if (result.Errors.Count() > 0)
            {
                return BadRequest(result.Errors);
            }
            else
            {
                return Problem("Registration failed");
            }
        }
        [HttpPost("Login")]
        public async Task<IActionResult> Login(UserDTO entity)
        {

            var result = await _signInManager.PasswordSignInAsync(entity.Email, entity.Password, false, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                var claim = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name,entity.Email??"")
                        };
                var accesstoken = tokenService.GenerateAccessToken(claim);
                return Ok(accesstoken);
            }
            else
            {
                return Unauthorized();
            }
        }

    }
  
}
