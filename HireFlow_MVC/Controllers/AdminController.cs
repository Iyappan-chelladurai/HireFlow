using HireFlow_MVC.Models.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace HireFlow_MVC.Controllers
{
    public class AdminController : Controller
    {

        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }
      
        public async Task<IActionResult> CreateRoles()
        {
            string[] roleNames = { "Admin", "HR", "Candidate" , "IT Team"};

            foreach (var roleName in roleNames)
            {
                var roleExists = await _roleManager.RoleExistsAsync(roleName);
                if (!roleExists)
                {
                    await _roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            return Content("Roles created successfully!");
        }

        public async Task<IActionResult> CreateAdminUser()
        {
            string UserName = "Admin";
            string Password = "Admin@123";
            string Role = "Admin";
            string Email = "Admin@HireFlow.com";

            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("", "User already exists.");
                return Content("User already exists.");
            }

            // Create new user
            var user = new IdentityUser
            {
                UserName =UserName,
                Email = Email,
            };

            var result = await _userManager.CreateAsync(user, Password);

            if (result.Succeeded)
            {
                // Ensure role exists
                var roleExists = await _roleManager.RoleExistsAsync(Role);
                if (!roleExists)
                {
                    await _roleManager.CreateAsync(new IdentityRole(Role));
                }

                // Assign role to user
                await _userManager.AddToRoleAsync(user, Role);

                ViewBag.Message = $"User '{Email}' created and added to role '{Role}'.";
                ModelState.Clear();
                return Content("");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View();
            }
        }
 
    }
}
