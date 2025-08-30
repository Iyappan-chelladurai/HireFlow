using HireFlow_MVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace HireFlow_MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly HttpClient _httpClient;

        public AccountController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("HireFlowAPI");
        }


        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }


        [HttpGet]
        public IActionResult JobsList()
        {
            return View();
        }



        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Map to CreateUserRequest
            var formContent = new MultipartFormDataContent();

            formContent.Add(new StringContent(model.UserName), nameof(model.UserName));
            formContent.Add(new StringContent(model.FullName), nameof(model.FullName));
            formContent.Add(new StringContent(model.Email), nameof(model.Email));
            formContent.Add(new StringContent(model.PhoneNumber), nameof(model.PhoneNumber));
            formContent.Add(new StringContent(model.Password), nameof(model.Password));
            formContent.Add(new StringContent("Candidate"), nameof(model.Role));

            if (model.ProfileImage != null && model.ProfileImage.Length > 0)
            {
                var streamContent = new StreamContent(model.ProfileImage.OpenReadStream());
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(model.ProfileImage.ContentType);
                formContent.Add(streamContent, nameof(model.ProfileImage), model.ProfileImage.FileName);
            }

            var response = await _httpClient.PostAsync("api/account/register", formContent);

            if (response.IsSuccessStatusCode)
            {
                
                TempData["Success"] = "Registration successful. Please login.";
                return RedirectToAction("Login", "Account");
            }
            else
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError("", $"Registration failed: {errorResponse}");
                return View(model);
            }
        }






        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {

            var loginData = new
            {
                Email = "Iyappan@Hireflow.com",
                Password = "Wanda@3000"
            };

            //var loginData = new
            //{
            //    Email = email,
            //    Password = password
            //};

            var response = await _httpClient.PostAsJsonAsync("/api/account/login/", loginData);

            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadFromJsonAsync<LoginResponse>();

                // Save JWT in Session
                HttpContext.Session.SetString("AuthToken", responseData.JwtToken);

                // Decode JWT to get Role
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(responseData.JwtToken);

                var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                var username = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
                var Email = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                var fullName = jwtToken.Claims.FirstOrDefault(c => c.Type == "FullName")?.Value;
                var candidateId = jwtToken.Claims.FirstOrDefault(c => c.Type == "candidateid")?.Value;
                var role = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

                // Save to session
                HttpContext.Session.SetString("UserId", userId ?? "");
                HttpContext.Session.SetString("UserName", username ?? "");
                HttpContext.Session.SetString("Email", Email ?? "");
                HttpContext.Session.SetString("FullName", fullName ?? "");
                HttpContext.Session.SetString("CandidateId", candidateId ?? "");
                HttpContext.Session.SetString("UserRole", role ?? "");
                HttpContext.Session.SetString("JwtToken", responseData.JwtToken);

                if (role.ToLower() == "candidate")
                {
                    return RedirectToAction("ViewApplicationStatus", "JobApplications");
                }
                else
                {
                    return RedirectToAction("PostJob", "Job");
                }
               
            }

            ModelState.AddModelError("", "Invalid login attempt.");
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove("AuthToken");
            return RedirectToAction("Login");
        }

    }
}
