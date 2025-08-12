using HireFlow_API.Model.DTOs;
using HireFlow_API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HireFlow_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
   public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var token = await _accountService.LoginUserAsync(request.Email, request.Password, request.RememberMe);

            if (token == null)
                return Unauthorized("Invalid login attempt.");

            return Ok(new { Message = "Login successful", JWTToken = token });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _accountService.LogoutUserAsync();
            return Ok("Logged out successfully.");
        }

        [HttpGet("create-roles")]
        public async Task<IActionResult> CreateRoles()
        {

            await _accountService.InitializeRolesAsync();
            return Ok("Roles created successfully!");
        }

        [HttpGet("create-admin")]
        public async Task<IActionResult> CreateAdminUser()
        {
            var result = await _accountService.InitializeAdminUserAsync();
            return Ok(result);
        }
    }

}
