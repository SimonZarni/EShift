using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EShift123.Data; // Assuming your DbContext is here
using EShift123.Models; // Your Container model
using Microsoft.AspNetCore.Authorization; // For [Authorize]

namespace EShift123.Controllers
{
    [Authorize(Roles = "Admin")] // Only Admins can manage Containers
    public class ContainerController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Constructor: Injects the database context
        public ContainerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Container
        // Displays a list of all containers
        public async Task<IActionResult> Index()
        {
            return View(await _context.Containers.ToListAsync());
        }

        // GET: Container/Details/5
        // Displays the details of a specific container
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            // Find the container by ID
            var container = await _context.Containers
                .FirstOrDefaultAsync(m => m.ContainerId == id);

            if (container == null)
                return NotFound();

            return View(container);
        }

        // GET: Container/Create
        // Displays the form to create a new container
        public IActionResult Create()
        {
            return View();
        }

        // POST: Container/Create
        // Handles the submission of the new container form
        [HttpPost]
        [ValidateAntiForgeryToken] // Protects against Cross-Site Request Forgery attacks
        // No [Bind] specified here, so it matches AssistantController's Create action
        public async Task<IActionResult> Create(Container container)
        {
            if (ModelState.IsValid)
            {
                _context.Add(container); // Add the new container to the context
                await _context.SaveChangesAsync(); // Save changes to the database
                return RedirectToAction(nameof(Index)); // Redirect to the container list
            }
            // If ModelState is not valid, return to view with validation errors
            return View(container);
        }

        // GET: Container/Edit/5
        // Displays the form to edit an existing container
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var container = await _context.Containers.FindAsync(id); // Find the container by ID
            if (container == null)
                return NotFound();

            return View(container);
        }

        // POST: Container/Edit/5
        // Handles the submission of the edited container form
        [HttpPost]
        [ValidateAntiForgeryToken]        
        public async Task<IActionResult> Edit(Container container)
        {
            if (!ModelState.IsValid) return View(container);

            var existing = await _context.Containers.FindAsync(container.ContainerId);
            if (existing == null) return NotFound();

            existing.ContainerNumber = container.ContainerNumber;
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // GET: Container/Delete/5
        // Displays a confirmation page for deleting a container
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var container = await _context.Containers
                .FirstOrDefaultAsync(m => m.ContainerId == id);
            if (container == null)
                return NotFound();

            return View(container);
        }

        // POST: Container/Delete/5
        // Handles the deletion of a container after confirmation
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var container = await _context.Containers.FindAsync(id);
            if (container != null)
            {
                _context.Containers.Remove(container); // Remove the container from the context
                await _context.SaveChangesAsync(); // Save changes
            }
            return RedirectToAction(nameof(Index));
        }

        // Helper method to check if a container exists
        private bool ContainerExists(int id)
        {
            return _context.Containers.Any(e => e.ContainerId == id);
        }
    }
}
