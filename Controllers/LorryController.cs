using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EShift123.Data; // Assuming your DbContext is here
using EShift123.Models; // Your Lorry model
using Microsoft.AspNetCore.Authorization; // For [Authorize]

namespace EShift123.Controllers
{
    [Authorize(Roles = "Admin")] // Only Admins can manage Lorries
    public class LorryController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Constructor: Injects the database context
        public LorryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Lorry
        // Displays a list of all lorries
        public async Task<IActionResult> Index()
        {
            return View(await _context.Lorries.ToListAsync());
        }

        // GET: Lorry/Details/5
        // Displays the details of a specific lorry
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            // Find the lorry by ID
            var lorry = await _context.Lorries
                .FirstOrDefaultAsync(m => m.LorryId == id);

            if (lorry == null)
                return NotFound();

            return View(lorry);
        }

        // GET: Lorry/Create
        // Displays the form to create a new lorry
        public IActionResult Create()
        {
            return View();
        }

        // POST: Lorry/Create
        // Handles the submission of the new lorry form
        [HttpPost]
        [ValidateAntiForgeryToken] // Protects against Cross-Site Request Forgery attacks
        public async Task<IActionResult> Create(Lorry lorry)
        {
            if (ModelState.IsValid)
            {
                _context.Add(lorry); // Add the new lorry to the context
                await _context.SaveChangesAsync(); // Save changes to the database
                return RedirectToAction(nameof(Index)); // Redirect to the lorry list
            }
            // If ModelState is not valid, return to view with validation errors
            return View(lorry);
        }

        // GET: Lorry/Edit/5
        // Displays the form to edit an existing lorry
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var lorry = await _context.Lorries.FindAsync(id); // Find the lorry by ID
            if (lorry == null)
                return NotFound();

            return View(lorry);
        }

        // POST: Lorry/Edit/5
        // Handles the submission of the edited lorry form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Lorry lorry)
        {
            if (!ModelState.IsValid) return View(lorry);

            var existing = await _context.Lorries.FindAsync(lorry.LorryId);
            if (existing == null) return NotFound();

            // Update properties explicitly, similar to ContainerController's approach
            existing.NumberPlate = lorry.NumberPlate;
            existing.Model = lorry.Model; // Assuming 'Model' is a property on your Lorry model

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // GET: Lorry/Delete/5
        // Displays a confirmation page for deleting a lorry
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var lorry = await _context.Lorries
                .FirstOrDefaultAsync(m => m.LorryId == id);
            if (lorry == null)
                return NotFound();

            return View(lorry);
        }

        // POST: Lorry/Delete/5
        // Handles the deletion of a lorry after confirmation
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lorry = await _context.Lorries.FindAsync(id);
            if (lorry != null)
            {
                _context.Lorries.Remove(lorry); // Remove the lorry from the context
                await _context.SaveChangesAsync(); // Save changes
            }
            return RedirectToAction(nameof(Index));
        }

        // Helper method to check if a lorry exists
        private bool LorryExists(int id)
        {
            return _context.Lorries.Any(e => e.LorryId == id);
        }
    }
}
