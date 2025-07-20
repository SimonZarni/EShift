// EShift123/Controllers/CustomerController.cs
using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Mvc;

namespace EShift123.Controllers 
{
    // [Authorize(Roles = "Customer")]
    public class CustomerController : Controller
    {
        // This action will serve the customer's home page.
        // By default, it will look for Views/Customer/Home.cshtml
        //public IActionResult Home()
        //{
        //    return View();
        //}
        public IActionResult Home(string message) // Or [FromQuery] string message
        {
            if (!string.IsNullOrEmpty(message))
            {
                ViewData["SuccessMessage"] = message; // Or set it to TempData for a single display
            }
            return View();
        }

        public IActionResult About()
        {
            return View();
        }
    }
}
