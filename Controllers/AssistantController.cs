using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EShift123.Data; // Assuming your DbContext is here
using EShift123.Models; // Your Assistant model
using Microsoft.AspNetCore.Authorization; // For [Authorize]

namespace EShift123.Controllers
{
    [Authorize(Roles = "Admin")] // Only Admins can manage Assistants
    public class AssistantController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Constructor: Injects the database context
        public AssistantController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Assistant
        // Displays a list of all assistants
        public async Task<IActionResult> Index()
        {
            return View(await _context.Assistants.ToListAsync());
        }

        // GET: Assistant/Details/5
        // Displays the details of a specific assistant
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            // Find the assistant by ID
            var assistant = await _context.Assistants
                .FirstOrDefaultAsync(m => m.AssistantId == id);

            if (assistant == null)
                return NotFound();

            return View(assistant);
        }

        // GET: Assistant/Create
        // Displays the form to create a new assistant
        public IActionResult Create()
        {
            return View();
        }

        // POST: Assistant/Create
        // Handles the submission of the new assistant form
        [HttpPost]
        [ValidateAntiForgeryToken] // Protects against Cross-Site Request Forgery attacks
        public async Task<IActionResult> Create(Assistant assistant)
        {
            if (ModelState.IsValid)
            {
                _context.Add(assistant); // Add the new assistant to the context
                await _context.SaveChangesAsync(); // Save changes to the database
                return RedirectToAction(nameof(Index)); // Redirect to the assistant list
            }
            // If ModelState is not valid, return to view with validation errors
            return View(assistant);
        }

        // GET: Assistant/Edit/5
        // Displays the form to edit an existing assistant
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var assistant = await _context.Assistants.FindAsync(id); // Find the assistant by ID
            if (assistant == null)
                return NotFound();

            return View(assistant);
        }

        // POST: Assistant/Edit/5
        // Handles the submission of the edited assistant form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Assistant assistant)
        {
            if (!ModelState.IsValid) return View(assistant);

            var existing = await _context.Assistants.FindAsync(assistant.AssistantId);
            if (existing == null) return NotFound();

            // Update properties explicitly to prevent overposting and ensure only allowed fields are modified
            existing.Name = assistant.Name;
            existing.Phone = assistant.Phone;

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // GET: Assistant/Delete/5
        // Displays a confirmation page for deleting an assistant
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var assistant = await _context.Assistants
                .FirstOrDefaultAsync(m => m.AssistantId == id);
            if (assistant == null)
                return NotFound();

            return View(assistant);
        }

        // POST: Assistant/Delete/5
        // Handles the deletion of an assistant after confirmation
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var assistant = await _context.Assistants.FindAsync(id);
            if (assistant != null)
            {
                _context.Assistants.Remove(assistant); // Remove the assistant from the context
                await _context.SaveChangesAsync(); // Save changes
            }
            return RedirectToAction(nameof(Index));
        }

        // Helper method to check if an assistant exists
        private bool AssistantExists(int id)
        {
            return _context.Assistants.Any(e => e.AssistantId == id);
        }
    }
}