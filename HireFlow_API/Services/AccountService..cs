using HireFlow_API.Repositories;

namespace HireFlow_API.Services
{
  public  interface IAccountService
    {
        Task<string?> LoginUserAsync(string email, string password, bool rememberMe);
        Task LogoutUserAsync();
        Task<bool> InitializeRolesAsync();
        Task<string> InitializeAdminUserAsync();
    }
     public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;

        public AccountService(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
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
