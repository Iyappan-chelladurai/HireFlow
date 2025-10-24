using HireFlow_API.Model;
using HireFlow_API.Model.DataModel;
using HireFlow_API.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Data;

namespace HireFlow_API.Repositories
{
    public interface IAccountRepository
    {
        Task<string> CreateUserAsync(UserAccount user, string password, string role);
        Task<string?> LoginAsync(string email, string password, bool rememberMe);
        Task LogoutAsync();
        Task<bool> CreateRolesAsync(string[] roles);
        Task<string> SeedDefaultUsersAsync();
    }

   public  class AccountRepository : IAccountRepository
    {
        private readonly UserManager<UserAccount> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly SignInManager<UserAccount> _signInManager;
        private readonly JwtTokenService _jwtTokenService;

        private readonly ApplicationDbContext _context;

        public AccountRepository(
            UserManager<UserAccount> userManager,
            RoleManager<IdentityRole<Guid>> roleManager,
            SignInManager<UserAccount> signInManager,
            JwtTokenService jwtTokenService,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _jwtTokenService = jwtTokenService;
            _context = context;
        }


        public async Task<string> CreateUserAsync(UserAccount user, string password ,string role)
        {
            var result = await _userManager.CreateAsync(user, password);
            user.SecurityStamp = Guid.NewGuid().ToString();
            await _userManager.AddToRoleAsync(user, role);

            if (role.ToLower()  == "candidate")
            {
                ICandidateDetailRepository candidateDetailRepository = new CandidateDetailRepository(_context);
                await candidateDetailRepository.CreateCandidateAsync(user);
            }


            if (result.Succeeded)
            {
                return $"User '{user.UserName}' created ";
            }

            var errors = string.Join(", ", result.Errors.Select(e => e.Description));

            return $"Failed to create user: {errors}";
        }

        public async Task<string?> LoginAsync(string email, string password, bool rememberMe)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return null;

            var result = await _signInManager.PasswordSignInAsync(user.UserName, password, rememberMe, false);
            if (!result.Succeeded)
                return null;

            var candidate =  _context.CandidateDetails.Where(a=>a.UserId == user.Id).FirstOrDefault();

            user.LastLoginTimestamp = DateTime.Now;
            
            await _context.SaveChangesAsync();

            var roles = await _userManager.GetRolesAsync(user);

            var token = _jwtTokenService.GenerateToken(user, candidate,  roles.FirstOrDefault());
            return token;
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<bool> CreateRolesAsync(string[] roleNames)
        {
            foreach (var roleName in roleNames)
            {
                var roleExists = await _roleManager.RoleExistsAsync(roleName);
                if (!roleExists)
                {
                    var role = new IdentityRole<Guid>
                    {
                        Id = Guid.NewGuid(),
                        Name = roleName,
                        NormalizedName = roleName.ToUpper()
                    };
                    await _roleManager.CreateAsync(role);
                }
            }
            return true;
        }

        public async Task<string> SeedDefaultUsersAsync()
        {
            var usersToCreate = new List<(string UserName, string FullName, string Email, string Password, string Role, string Phone)>
    {
        ("Admin", "Admin", "admin@hireflow.com", "Admin@123", "Admin", "8098776961"),
        ("kavya.ramesh", "Kavya Ramesh", "kavya.ramesh@hireflow.com", "HRTeam@123", "HR", "8098776961"),
        ("aravind.kumar", "Aravind Kumar", "aravind.kumar@hireflow.com", "ITTeam@123", "IT Team", "8098776961"),
        ("vignesh.raj", "Vignesh Raj", "vignesh.raj@hireflow.com", "Interviewer@123", "Interviewer", "8098776961"),
        ("sneha.subramanian", "Sneha Subramanian", "sneha.subramanian@hireflow.com", "Manager@123", "Manager", "8098776961")
    };

            var results = new List<string>();

            foreach (var (userName, fullName, email, password, role, phone) in usersToCreate)
            {
                var existingUser = await _userManager.FindByEmailAsync(email);
                if (existingUser != null)
                {
                    results.Add($"User {email} already exists.");
                    continue;
                }

                var user = new UserAccount
                {
                    Id = Guid.NewGuid(),
                    UserName = userName,
                    FullName = fullName,
                    Email = email,
                    NormalizedUserName = userName.ToUpper(),
                    NormalizedEmail = email.ToUpper(),
                    AccountCreatedOn = DateTime.Now,
                    IsAccountActive = true,
                    EmailConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    PhoneNumber = phone
                };

                var createResult = await _userManager.CreateAsync(user, password);
                if (!createResult.Succeeded)
                {
                    var errorMsg = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    results.Add($"❌ Failed to create {email}: {errorMsg}");
                    continue;
                }

                // Ensure role exists
                var roleExists = await _roleManager.RoleExistsAsync(role);
                if (!roleExists)
                {
                    await _roleManager.CreateAsync(new IdentityRole<Guid>(role));
                }

                // Assign role
                await _userManager.AddToRoleAsync(user, role);
                results.Add($"✅ Created {email} with role '{role}'.");
            }

            return string.Join(Environment.NewLine, results);
        }

    }

}
