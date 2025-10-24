using HireFlow_API.Model.DataModel;
using HireFlow_API.Model.DTOs;
using HireFlow_API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using LoginRequest = HireFlow_API.Model.DTOs.LoginRequest;

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

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] CreateUserRequest model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _accountService.RegisterUserAsync(model);

            if (!result.Item1)
                return BadRequest(new { success = false, message = result.Item2 });

            return Ok(new { success = true, message = result.Item2 });
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


        [HttpPost("Otp-send")]
        public async Task<IActionResult> SendOtp([FromBody] OtpRequest request)
        {
            await _accountService.SendOtpAsync(request.PhoneNumber);
            return Ok(new { success = true, message = "OTP sent successfully." });
        }

        [HttpPost("Otp-verify")]
        public IActionResult VerifyOtp([FromBody] CreateOTPRequest request)
        {
            bool isValid = _accountService.VerifyOtp(request.PhoneNumber, request.Otp);
            if (isValid)
                return Ok(new { success = true, message = "OTP verified successfully." });

            return BadRequest(new { success = false, message = "Invalid OTP." });
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
        public async Task<IActionResult> CreateDefultUsers()
        {
            var result = await _accountService.InitializeDefultsUserAsync();
            return Ok(result);
        }
    }
    public class OtpRequest
    {
        public string PhoneNumber { get; set; }
    }
    public class CreateOTPRequest
    {
        public string PhoneNumber { get; set; }
        public string Otp { get; set; }
    }
}
