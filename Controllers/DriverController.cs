using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EShift123.Data; // Assuming your DbContext is here
using EShift123.Models; // Your Driver model
using Microsoft.AspNetCore.Authorization; // For [Authorize]

namespace EShift123.Controllers
{
    [Authorize(Roles = "Admin")] // Only Admins can manage Drivers
    public class DriverController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Constructor: Injects the database context
        public DriverController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Driver
        // Displays a list of all drivers
        public async Task<IActionResult> Index()
        {
            return View(await _context.Drivers.ToListAsync());
        }

        // GET: Driver/Details/5
        // Displays the details of a specific driver
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            // Find the driver by ID
            var driver = await _context.Drivers
                .FirstOrDefaultAsync(m => m.DriverId == id);

            if (driver == null)
                return NotFound();

            return View(driver);
        }

        // GET: Driver/Create
        // Displays the form to create a new driver
        public IActionResult Create()
        {
            return View();
        }

        // POST: Driver/Create
        // Handles the submission of the new driver form
        [HttpPost]
        [ValidateAntiForgeryToken] // Protects against Cross-Site Request Forgery attacks
        public async Task<IActionResult> Create(Driver driver)
        {
            if (ModelState.IsValid)
            {
                _context.Add(driver); // Add the new driver to the context
                await _context.SaveChangesAsync(); // Save changes to the database
                return RedirectToAction(nameof(Index)); // Redirect to the driver list
            }
            // If ModelState is not valid, return to view with validation errors
            return View(driver);
        }

        // GET: Driver/Edit/5
        // Displays the form to edit an existing driver
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var driver = await _context.Drivers.FindAsync(id); // Find the driver by ID
            if (driver == null)
                return NotFound();

            return View(driver);
        }

        // POST: Driver/Edit/5
        // Handles the submission of the edited driver form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Driver driver)
        {
            if (!ModelState.IsValid) return View(driver);

            var existing = await _context.Drivers.FindAsync(driver.DriverId);
            if (existing == null) return NotFound();

            // Update properties explicitly, similar to ContainerController's approach
            existing.Name = driver.Name;
            existing.LicenseNumber = driver.LicenseNumber; // Assuming 'LicenseNumber' is a property on your Driver model
            existing.Phone = driver.Phone; // Assuming 'Phone' is a property on your Driver model

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // GET: Driver/Delete/5
        // Displays a confirmation page for deleting a driver
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var driver = await _context.Drivers
                .FirstOrDefaultAsync(m => m.DriverId == id);
            if (driver == null)
                return NotFound();

            return View(driver);
        }

        // POST: Driver/Delete/5
        // Handles the deletion of a driver after confirmation
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var driver = await _context.Drivers.FindAsync(id);
            if (driver != null)
            {
                _context.Drivers.Remove(driver); // Remove the driver from the context
                await _context.SaveChangesAsync(); // Save changes
            }
            return RedirectToAction(nameof(Index));
        }

        // Helper method to check if a driver exists
        private bool DriverExists(int id)
        {
            return _context.Drivers.Any(e => e.DriverId == id);
        }
    }
}
