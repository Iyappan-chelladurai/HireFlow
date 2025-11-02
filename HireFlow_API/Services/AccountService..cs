using Amazon;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using HireFlow_API.Model.DataModel;
using HireFlow_API.Model.DTOs;
using HireFlow_API.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
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
        Task<string> InitializeDefultsUserAsync();
        Task<string> SendOtpAsync(string phoneNumber);
        bool VerifyOtp(string phoneNumber, string userOtp);

    }
     public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;
       
        private readonly IConfiguration _configuration;

        private readonly IAmazonSimpleNotificationService _snsClient;
    
        private readonly IMemoryCache _cache;
 
        public AccountService(IAccountRepository accountRepository,  IConfiguration configuration , IMemoryCache cache)
        {
            _accountRepository = accountRepository;
            _configuration = configuration;
            _cache = cache;
            var region = RegionEndpoint.GetBySystemName(_configuration["AwsSettings:Region"]);

            _snsClient = new AmazonSimpleNotificationServiceClient(
                _configuration["AwsSettings:AccessKey"] ,
                _configuration["AwsSettings:SecretAccessKey"] ,
               region);
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
            string[] roleNames = { "Admin", "HR", "Candidate", "IT Team" , "Interviewer" , "Manger"};

            return await _accountRepository.CreateRolesAsync(roleNames);
        }

        public async Task<string> InitializeDefultsUserAsync()
        {
            return await _accountRepository.SeedDefaultUsersAsync();
        }

        public bool VerifyOtp(string phoneNumber, string userOtp)
        {
            if (_cache.TryGetValue($"OTP_{phoneNumber}", out string cachedOtp))
            {
                if (cachedOtp == userOtp)
                {
                    // Remove OTP after successful verification
                    _cache.Remove($"OTP_{phoneNumber}");
                    return true;
                }
            }
            return false;
        }


        public async Task<string> SendOtpAsync(string phoneNumber)
        {
            var otp = new Random().Next(100000, 999999).ToString();
            _cache.Set($"OTP_{phoneNumber}", otp, TimeSpan.FromMinutes(5));

            var message = $"Your HireFlow verification code is: {otp}";

            var request = new PublishRequest
            {
                Message = message,
                PhoneNumber = phoneNumber, // Include +91 for India, e.g. +919876543210
                 MessageAttributes = new Dictionary<string, MessageAttributeValue>
            {
                {
                    "AWS.SNS.SMS.SMSType",
                    new MessageAttributeValue
                    {
                        StringValue = "Transactional",
                        DataType = "String"
                    }
                }
            }
            };

            try
            {
                var response = await _snsClient.PublishAsync(request);

                // Check if the message was sent successfully
                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    Console.WriteLine(" SMS sent successfully!");
                    Console.WriteLine($"MessageId: {response.MessageId}");
                }
                else
                {
                    Console.WriteLine($" SMS send failed. Status code: {response.HttpStatusCode}");
                }
            }
            catch (Amazon.SimpleNotificationService.AmazonSimpleNotificationServiceException snsEx)
            {
                Console.WriteLine($" SNS error: {snsEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($" General error: {ex.Message}");
            }




            return otp;
        }


    }

}
