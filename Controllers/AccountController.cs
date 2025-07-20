using EShift123.Data;
using EShift123.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EShift123.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager; 
        private readonly ApplicationDbContext _context;

        //public AccountController(
        //UserManager<IdentityUser> userManager,
        //SignInManager<IdentityUser> signInManager,
        //ApplicationDbContext context)
        //{
        //    _userManager = userManager;
        //    _signInManager = signInManager;
        //    _context = context;
        //}

        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            RoleManager<IdentityRole> roleManager, 
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new IdentityUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    PhoneNumber = model.Phone
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // --- START: Role Assignment Logic (from previous immersive) ---

                    // 1. Ensure the "Customer" role exists
                    bool customerRoleExists = await _roleManager.RoleExistsAsync("Customer");
                    if (!customerRoleExists)
                    {
                        // If the role doesn't exist, create it.
                        // In a production app, roles are often seeded on startup,
                        // but this ensures it's created if somehow missing.
                        await _roleManager.CreateAsync(new IdentityRole("Customer"));
                    }

                    // 2. Assign the newly created user to the "Customer" role
                    var roleAssignmentResult = await _userManager.AddToRoleAsync(user, "Customer");

                    if (!roleAssignmentResult.Succeeded)
                    {
                        // Handle case where role assignment fails (e.g., log error).
                        // For simplicity, we'll add errors to ModelState, but consider
                        // more robust error handling for production.
                        foreach (var error in roleAssignmentResult.Errors)
                        {
                            ModelState.AddModelError("", $"Failed to assign role: {error.Description}");
                        }
                        // Optionally, you might want to delete the user here if role assignment is critical
                        // await _userManager.DeleteAsync(user);
                        // return View(model); // Or redirect to a specific error page
                    }
                    // --- END: Role Assignment Logic ---


                    // Insert into Customers table (existing logic)
                    var customer = new Customer
                    {
                        UserId = user.Id,
                        Name = model.Name,
                        Email = model.Email,
                        Phone = model.Phone,
                        Address = model.Address,
                        CreatedAt = DateTime.UtcNow,
                    };

                    _context.Customers.Add(customer);
                    await _context.SaveChangesAsync();

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Home", "Customer"); // Redirect to customer home page
                }

                // If user creation failed (e.g., password too weak, email already exists)
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
            }

            // If ModelState is invalid or user creation failed
            return View(model);
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    if (await _userManager.IsInRoleAsync(user, "Admin"))
                    {
                        // Redirect to Admin Dashboard
                        return RedirectToAction("Dashboard", "Admin"); // Assuming "Dashboard" action in "Admin" controller
                    }

                    // Redirect to customer home page within the Customer area
                    // Corrected line:
                    return RedirectToAction("Home", "Customer");
                }

                ModelState.AddModelError("", "Invalid login attempt.");
            }

            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        //public IActionResult AccessDenied()
        //{
        //    return View();
        //}
    }
}

