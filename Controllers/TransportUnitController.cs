using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // For SelectList
using Microsoft.EntityFrameworkCore;
using EShift123.Data;
using EShift123.Models;
using Microsoft.AspNetCore.Authorization; // For [Authorize]

namespace EShift123.Controllers
{
    [Authorize(Roles = "Admin")] // Only Admins can manage Transport Units (Task 2, part of Task 5)
    public class TransportUnitController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TransportUnitController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TransportUnit
        // Displays a list of all transport units for Admins
        public async Task<IActionResult> Index()
        {
            // Eagerly load related entities (Lorry, Driver, Assistant, Container) for display
            var transportUnits = _context.TransportUnits
                                         .Include(t => t.Lorry)
                                         .Include(t => t.Driver)
                                         .Include(t => t.Assistant)
                                         .Include(t => t.Container);
            return View(await transportUnits.ToListAsync());
        }

        // GET: TransportUnit/Details/5
        // Displays the details of a specific transport unit
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            // Eagerly load related entities for display
            var transportUnit = await _context.TransportUnits
                .Include(t => t.Lorry)
                .Include(t => t.Driver)
                .Include(t => t.Assistant)
                .Include(t => t.Container)
                .FirstOrDefaultAsync(m => m.TransportUnitId == id);

            if (transportUnit == null) return NotFound();
            return View(transportUnit);
        }

        // GET: TransportUnit/Create
        // Displays the form to create a new transport unit

        public async Task<IActionResult> Create()
        {
            // Lorry: PK = Id, Text = NumberPlate
            ViewData["LorryId"] = (await _context.Lorries.OrderBy(l => l.NumberPlate).ToListAsync())
                .Select(l => new SelectListItem
                {
                    Value = l.LorryId.ToString(),
                    Text = l.NumberPlate
                }).ToList();

            // Driver: PK = Id, Text = Name
            ViewData["DriverId"] = (await _context.Drivers.OrderBy(d => d.Name).ToListAsync())
                .Select(d => new SelectListItem
                {
                    Value = d.DriverId.ToString(),
                    Text = d.Name
                }).ToList();

            // Assistant: PK = Id (nullable), Text = Name
            ViewData["AssistantId"] = (await _context.Assistants.OrderBy(a => a.Name).ToListAsync())
                .Select(a => new SelectListItem
                {
                    Value = a.AssistantId.ToString(),
                    Text = a.Name
                }).ToList();

            // Container: PK = ContainerId, Text = ContainerNumber
            ViewData["ContainerId"] = (await _context.Containers.OrderBy(c => c.ContainerNumber).ToListAsync())
                .Select(c => new SelectListItem
                {
                    Value = c.ContainerId.ToString(),
                    Text = c.ContainerNumber
                }).ToList();

            return View(new TransportUnit());
        }

        // POST: TransportUnit/Create
        // Handles the submission of the new transport unit form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TransportUnitId,UnitNumber,LorryId,DriverId,AssistantId,ContainerId")] TransportUnit transportUnit)
        {
            // IMPORTANT: Remove navigation properties from ModelState to avoid validation errors.
            // These properties are not directly bound from the form, only their IDs are.
            ModelState.Remove("Lorry");
            ModelState.Remove("Driver");
            ModelState.Remove("Assistant");
            ModelState.Remove("Container");

            if (ModelState.IsValid)
            {
                _context.Add(transportUnit); // Add the new transport unit to the context
                await _context.SaveChangesAsync(); // Save changes to the database
                TempData["SuccessMessage"] = "Transport Unit created successfully!";
                return RedirectToAction(nameof(Index)); // Redirect to the transport unit list
            }

            // If ModelState is not valid, re-populate SelectLists and return to view with errors
            ViewData["LorryId"] = new SelectList(await _context.Lorries.OrderBy(l => l.NumberPlate).ToListAsync(), "LorryId", "NumberPlate", transportUnit.LorryId);
            ViewData["DriverId"] = new SelectList(await _context.Drivers.OrderBy(d => d.Name).ToListAsync(), "DriverId", "Name", transportUnit.DriverId);
            ViewData["AssistantId"] = new SelectList(await _context.Assistants.OrderBy(a => a.Name).ToListAsync(), "AssistantId", "Name", transportUnit.AssistantId);
            ViewData["ContainerId"] = new SelectList(await _context.Containers.OrderBy(c => c.ContainerNumber).ToListAsync(), "ContainerId", "ContainerNumber", transportUnit.ContainerId);
            return View(transportUnit);
        }

        // GET: TransportUnit/Edit/5
        // Displays the form to edit an existing transport unit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            // Find the transport unit by ID
            var transportUnit = await _context.TransportUnits.FindAsync(id);
            if (transportUnit == null) return NotFound();

            // Populate SelectLists for editing, with the current values pre-selected
            // Lorry: PK = Id, Text = NumberPlate
            var lorries = await _context.Lorries.OrderBy(l => l.NumberPlate).ToListAsync();
            ViewData["LorryId"] = lorries.Select(l => new SelectListItem
            {
                Value = l.LorryId.ToString(),
                Text = l.NumberPlate,
                Selected = (l.LorryId == transportUnit.LorryId) // Set 'Selected' based on current TransportUnit's LorryId
            }).ToList();

            // Driver: PK = Id, Text = Name
            var drivers = await _context.Drivers.OrderBy(d => d.Name).ToListAsync();
            ViewData["DriverId"] = drivers.Select(d => new SelectListItem
            {
                Value = d.DriverId.ToString(),
                Text = d.Name,
                Selected = (d.DriverId == transportUnit.DriverId) // Set 'Selected' based on current TransportUnit's DriverId
            }).ToList();

            // Assistant: PK = Id (nullable), Text = Name
            var assistants = await _context.Assistants.OrderBy(a => a.Name).ToListAsync();
            ViewData["AssistantId"] = assistants.Select(a => new SelectListItem
            {
                // Ensure 'Id' matches the actual PK property name in your Assistant model
                Value = a.AssistantId.ToString(),
                Text = a.Name,
                // Handle nullable AssistantId comparison (a.Id will be int, transportUnit.AssistantId is int?)
                Selected = (a.AssistantId == transportUnit.AssistantId)
            }).ToList();

            // Container: PK = ContainerId, Text = ContainerNumber
            var containers = await _context.Containers.OrderBy(c => c.ContainerNumber).ToListAsync();
            ViewData["ContainerId"] = containers.Select(c => new SelectListItem
            {
                Value = c.ContainerId.ToString(),
                Text = c.ContainerNumber,
                Selected = (c.ContainerId == transportUnit.ContainerId) // Set 'Selected' based on current TransportUnit's ContainerId
            }).ToList();

            return View(transportUnit);
        }

        // POST: TransportUnit/Edit/5
        // Handles the submission of the edited transport unit form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TransportUnitId,UnitNumber,LorryId,DriverId,AssistantId,ContainerId")] TransportUnit transportUnit)
        {
            if (id != transportUnit.TransportUnitId) return NotFound();

            // IMPORTANT: Remove navigation properties from ModelState
            ModelState.Remove("Lorry");
            ModelState.Remove("Driver");
            ModelState.Remove("Assistant");
            ModelState.Remove("Container");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(transportUnit); // Update the transport unit in the context
                    await _context.SaveChangesAsync(); // Save changes
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Handle concurrency conflicts (e.g., transport unit deleted by another user)
                    if (!TransportUnitExists(transportUnit.TransportUnitId)) return NotFound();
                    else throw; // Re-throw if it's another type of concurrency issue
                }
                TempData["SuccessMessage"] = "Transport Unit updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            // If ModelState is not valid, re-populate SelectLists and return to view with errors
            ViewData["LorryId"] = new SelectList(await _context.Lorries.OrderBy(l => l.NumberPlate).ToListAsync(), "LorryId", "NumberPlate", transportUnit.LorryId);
            ViewData["DriverId"] = new SelectList(await _context.Drivers.OrderBy(d => d.Name).ToListAsync(), "DriverId", "Name", transportUnit.DriverId);
            ViewData["AssistantId"] = new SelectList(await _context.Assistants.OrderBy(a => a.Name).ToListAsync(), "AssistantId", "Name", transportUnit.AssistantId);
            ViewData["ContainerId"] = new SelectList(await _context.Containers.OrderBy(c => c.ContainerNumber).ToListAsync(), "ContainerId", "ContainerNumber", transportUnit.ContainerId);
            return View(transportUnit);
        }

        // GET: TransportUnit/Delete/5
        // Displays a confirmation page for deleting a transport unit
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            // Eagerly load related entities for display on confirmation page
            var transportUnit = await _context.TransportUnits
                .Include(t => t.Lorry)
                .Include(t => t.Driver)
                .Include(t => t.Assistant)
                .Include(t => t.Container)
                .FirstOrDefaultAsync(m => m.TransportUnitId == id);
            if (transportUnit == null) return NotFound();
            return View(transportUnit);
        }

        // POST: TransportUnit/Delete/5
        // Handles the deletion of a transport unit after confirmation
        [HttpPost, ActionName("Delete")] // ActionName specifies which action this POST method corresponds to
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var transportUnit = await _context.TransportUnits.FindAsync(id);
            if (transportUnit != null)
            {
                _context.TransportUnits.Remove(transportUnit); // Remove the transport unit from the context
                await _context.SaveChangesAsync(); // Save changes
            }
            TempData["SuccessMessage"] = "Transport Unit deleted successfully!";
            return RedirectToAction(nameof(Index)); // Redirect to the transport unit list
        }

        // Helper method to check if a transport unit exists
        private bool TransportUnitExists(int id)
        {
            return _context.TransportUnits.Any(e => e.TransportUnitId == id);
        }
    }
}