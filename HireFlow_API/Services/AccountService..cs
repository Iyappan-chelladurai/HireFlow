using HireFlow_API.Model.DataModel;
using HireFlow_API.Model.DTOs;
using HireFlow_API.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Data;
using System.Security.Authentication;
using System.Security.Cryptography.Xml;

namespace HireFlow_API.Services
{
  public  interface IAccountService
    {
        Task<(bool , string)> RegisterUserAsync(CreateUserRequest model);
        Task<string?> LoginUserAsync(string email, string password, bool rememberMe);
        Task LogoutUserAsync();
        Task<bool> InitializeRolesAsync();
        Task<string> InitializeAdminUserAsync();
    }
     public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;
       
        private readonly IConfiguration _configuration;

        public AccountService(IAccountRepository accountRepository,  IConfiguration configuration)
        {
            _accountRepository = accountRepository;
            _configuration = configuration;
        }

        public async Task<(bool, string)> RegisterUserAsync(CreateUserRequest model)
        {
          
            string profilePath = null;

            var ImagePath = _configuration["FilePath:ImagePath"];

            if (model.ProfileImage != null && model.ProfileImage.Length > 0)
            {
                if (!Directory.Exists(ImagePath))
                    Directory.CreateDirectory(ImagePath);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(model.ProfileImage.FileName)}";

                profilePath   = Path.Combine(ImagePath, fileName);

                using (var stream = new FileStream(profilePath, FileMode.Create))
                {
                    await model.ProfileImage.CopyToAsync(stream);
                }
                 
            }

           
            var user = new UserAccount
            {
                Id = Guid.NewGuid(),
                UserName = model.UserName,
                FullName = model.FullName,
                Email = model.Email,
                NormalizedUserName = model.UserName.ToUpper(),
                NormalizedEmail = model.Email.ToUpper(),
                PhoneNumber = model.PhoneNumber,
                ProfileImagePath = profilePath,
                IsAccountActive = true,
                AccountCreatedOn = DateTime.UtcNow

            };

            var result = await _accountRepository.CreateUserAsync(user, model.Password, model.Role);


           

            if (!result.Contains("Failed"))
            {
                return (true, "User Account Created!");
            }
            else
            {
                return (false, result);
            }

            
        }
    

        public async Task<string?> LoginUserAsync(string email, string password, bool rememberMe)
        {
            return await _accountRepository.LoginAsync(email, password, rememberMe);
        }

        public async Task LogoutUserAsync()
        {
            await _accountRepository.LogoutAsync();
        }

        public async Task<bool> InitializeRolesAsync()
        {
            string[] roleNames = { "Admin", "HR", "Candidate", "IT Team" };

            return await _accountRepository.CreateRolesAsync(roleNames);
        }

        public async Task<string> InitializeAdminUserAsync()
        {
            return await _accountRepository.CreateAdminUserAsync();
        }
    }

}
