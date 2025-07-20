using EShift123.Data;
using EShift123.Models;
using EShift123.Models.ViewModels; // To use LoadEditViewModel and ProductInputModel
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EShift123.Controllers
{
    [Authorize(Roles = "Admin")] // Only administrators can manage loads
    [Route("[controller]")]
    public class LoadController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LoadController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Load
        // Displays a list of all loads.
        [HttpGet]
        [Route("Index")]
        [Route("")]
        public async Task<IActionResult> Index()
        {
            // Include related Job and Customer information for better context
            var loads = await _context.Loads
                                      .Include(l => l.Job)
                                        .ThenInclude(j => j.Customer)
                                      .Include(l => l.LoadProducts)
                                        .ThenInclude(lp => lp.Product)
                                      .OrderByDescending(l => l.PickupDate)
                                      .ToListAsync();
            return View(loads);
        }

        private async Task<LoadEditViewModel> LoadEditViewModel(Load load)
        {
            // Map the Load entity to the LoadEditViewModel
            var model = new LoadEditViewModel
            {
                LoadId = load.LoadId,
                JobId = load.JobId,
                CustomerId = load.Job.CustomerId, // Needed for potential product operations (though not in this specific UI)
                LoadNumber = load.LoadNumber,
                Description = load.Description,
                WeightKg = load.WeightKg,
                PickupDate = load.PickupDate,
                DeliveryDate = load.DeliveryDate,
                Status = load.Status,
                TransportUnitId = load.TransportUnitId, // Populate with the currently assigned TransportUnitId (can be null)
                // Products are included in the ViewModel as per the ViewModel definition,
                // even if this specific "assign transport unit" UI doesn't edit them.
                // This ensures the ViewModel remains consistent and can be used in other contexts.
                Products = load.LoadProducts
                               .Select(lp => new ProductInputModel1
                               {
                                   ProductId = lp.ProductId,
                                   Name = lp.Product.Name,
                                   Category = lp.Product.Category,
                                   Description = lp.Product.Description,
                                   WeightKg = lp.Product.WeightKg,
                                   Quantity = lp.Quantity
                               })
                               .ToList()
            };

            // Fetch all available transport units from the database
            //var transportUnits = await _context.TransportUnits.ToListAsync();
            var transportUnits = await _context.TransportUnits
                                              .Include(tu => tu.Lorry)
                                              .Include(tu => tu.Driver)
                                              .ToListAsync();

            // Create a list of SelectListItem for the dropdown
            var transportUnitItems = transportUnits.Select(tu => new SelectListItem
            {
                Value = tu.TransportUnitId.ToString(), // Value of the option (TransportUnitId)
                // FIX APPLIED HERE: Explicitly accessing the 'Name' property of Lorry and Driver objects
                // Using null-conditional operator (?. ) for safety in case Lorry or Driver are null
                Text = $"Unit: {tu.UnitNumber} | Lorry: {tu.Lorry?.NumberPlate}, Driver: {tu.Driver?.Name}" // Display text for the option
            }).ToList();

            // Add a default "Select or Unassign" option at the beginning of the list
            transportUnitItems.Insert(0, new SelectListItem
            {
                Value = "", // Empty string value for "unassigned"
                Text = "-- Select or Unassign Transport Unit --",
                Selected = !model.TransportUnitId.HasValue // Select this option if no unit is currently assigned
            });

            // Set the SelectList on the ViewModel
            model.TransportUnits = new SelectList(transportUnitItems, "Value", "Text", model.TransportUnitId);

            // Ensure the Products collection is never null and contains at least one item
            // This is primarily for the general-purpose LoadEditViewModel, even if this specific UI doesn't use it.
            if (!model.Products.Any())
            {
                model.Products.Add(new ProductInputModel1());
            }

            return model;
        }

        // GET: /Load/Details/5
        // Displays comprehensive details of a specific load, including its products.
        [HttpGet]
        [Route("Details/{id?}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var load = await _context.Loads
                .Include(l => l.Job)
                    .ThenInclude(j => j.Customer) // Include Customer of the Job
                .Include(l => l.TransportUnit) // Include Transport Unit if assigned
                .Include(l => l.TransportUnit)
                    .ThenInclude(tu => tu.Lorry) // Ensure Lorry is included for Details view
                .Include(l => l.TransportUnit)
                    .ThenInclude(tu => tu.Driver)
                .Include(l => l.LoadProducts)
                    .ThenInclude(lp => lp.Product) // Include Products associated with the load
                .FirstOrDefaultAsync(m => m.LoadId == id);

            if (load == null)
            {
                return NotFound();
            }

            return View(load);
        }

        // GET: /Load/Edit/5
        // Displays the form to assign/unassign a TransportUnit to an existing load.
        // Other load details are displayed as read-only.
        [HttpGet]
        [Route("Edit/{id?}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Fetch the Load and its related entities required for the ViewModel.
            var load = await _context.Loads
                .Include(l => l.Job) // Required to get CustomerId for PrepareLoadEditViewModel
                .Include(l => l.LoadProducts) // Required to populate Products list in ViewModel
                    .ThenInclude(lp => lp.Product)
                .FirstOrDefaultAsync(m => m.LoadId == id);

            if (load == null)
            {
                return NotFound();
            }

            // Prepare the ViewModel with load data and transport unit options.
            var model = await LoadEditViewModel(load);
            return View(model);
        }

        // POST: /Load/Edit/5
        // Processes the submitted form to update ONLY the TransportUnitId of a load.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Edit/{id?}")]
        public async Task<IActionResult> Edit(int id, LoadEditViewModel model)
        {
            // Ensure the ID from the URL matches the ID in the submitted model.
            if (id != model.LoadId)
            {
                return NotFound();
            }

            // Remove ModelState entries for properties that are NOT being bound/updated by this specific form logic.
            // This prevents validation errors for fields not present or not editable in the UI.
            ModelState.Remove("Description");
            ModelState.Remove("WeightKg");
            ModelState.Remove("PickupDate");
            ModelState.Remove("DeliveryDate");
            ModelState.Remove("Status");
            ModelState.Remove("LoadNumber");
            ModelState.Remove("JobId");
            ModelState.Remove("CustomerId");
            ModelState.Remove("Products"); // Product list is not edited by this form
            ModelState.Remove("TransportUnits"); // SelectList is for view-only, not for binding

            // Check if the model state is valid after removing specific entries.
            if (ModelState.IsValid)
            {
                // Use a database transaction to ensure atomicity of the update operation.
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // Fetch the Load entity from the database that needs to be updated.
                        // We only need the Load itself, as we're just updating TransportUnitId.
                        var loadToUpdate = await _context.Loads
                            .FirstOrDefaultAsync(l => l.LoadId == id);

                        if (loadToUpdate == null)
                        {
                            await transaction.RollbackAsync();
                            return NotFound();
                        }

                        // --- Core Logic: ONLY UPDATE TRANSPORT UNIT ID ---
                        loadToUpdate.TransportUnitId = model.TransportUnitId;

                        if (model.TransportUnitId.HasValue)
                        {
                            // Only change to Assigned if it's not already Delivered or Cancelled
                            if (loadToUpdate.Status != LoadStatus.Delivered && loadToUpdate.Status != LoadStatus.Cancelled)
                            {
                                loadToUpdate.Status = LoadStatus.Assigned;
                            }
                        }
                        else
                        {
                            // If TransportUnit is unassigned, set status to Pending
                            // Only change to Pending if it's not already Delivered or Cancelled
                            if (loadToUpdate.Status != LoadStatus.Delivered && loadToUpdate.Status != LoadStatus.Cancelled)
                            {
                                loadToUpdate.Status = LoadStatus.Pending;
                            }
                        }

                        _context.Loads.Update(loadToUpdate); // Mark the entity as modified

                        // Save changes to the database
                        await _context.SaveChangesAsync();
                        // Commit the transaction if all operations were successful
                        await transaction.CommitAsync();

                        // Provide user feedback and redirect to the Load Details page.
                        TempData["SuccessMessage"] = "Transport Unit assigned successfully!";
                        return RedirectToAction(nameof(Details), new { id = model.LoadId });
                    }
                    catch (DbUpdateConcurrencyException) // Handle concurrency conflicts
                    {
                        await transaction.RollbackAsync();
                        if (!LoadExists(model.LoadId))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw; // Re-throw if it's a genuine concurrency issue
                        }
                    }
                    catch (Exception ex) // Catch any other exceptions during the update process
                    {
                        await transaction.RollbackAsync();
                        ModelState.AddModelError("", "An error occurred while assigning the transport unit. Please try again. " + ex.Message);
                        // Log the full exception details to the console for debugging purposes
                        Console.WriteLine($"Error assigning transport unit: {ex.ToString()}");
                    }
                }
            }
            else // If ModelState is NOT valid (even after removals)
            {
                // Log all remaining validation errors to the console for debugging
                Console.WriteLine("Model State is Invalid:");
                foreach (var state in ModelState)
                {
                    if (state.Value.Errors.Any())
                    {
                        Console.WriteLine($"  {state.Key}:");
                        foreach (var error in state.Value.Errors)
                        {
                            Console.WriteLine($"    - {error.ErrorMessage}");
                        }
                    }
                }
            }

            // If ModelState is invalid or an error occurred, re-populate the model for the view
            // and re-display the form so the user can correct errors.
            // Re-fetch the actual load to get its latest data for display (since only TransportUnit was bound)
            var reloadedLoad = await _context.Loads
                                            .Include(l => l.Job) // Need Job for CustomerId for PrepareLoadEditViewModel
                                            .Include(l => l.LoadProducts) // Need Products for PrepareLoadEditViewModel
                                                .ThenInclude(lp => lp.Product)
                                            .FirstOrDefaultAsync(l => l.LoadId == model.LoadId);
            if (reloadedLoad != null)
            {
                var preparedModel = await LoadEditViewModel(reloadedLoad);
                // Ensure the selected TransportUnitId from the failed submission is re-selected in the dropdown
                preparedModel.TransportUnitId = model.TransportUnitId;
                return View(preparedModel);
            }
            return NotFound(); // This should ideally not happen if the id was valid initially
        }

        // GET: /Load/Delete/5
        // Displays a confirmation page for deleting a load.
        [HttpGet]
        [Route("Delete/{id?}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var load = await _context.Loads
                .Include(l => l.Job)
                    .ThenInclude(j => j.Customer)
                .Include(l => l.LoadProducts)
                    .ThenInclude(lp => lp.Product)
                .FirstOrDefaultAsync(m => m.LoadId == id);

            if (load == null)
            {
                return NotFound();
            }

            return View(load);
        }

        // POST: /Load/Delete/5
        // Confirms and processes the deletion of a load.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Route("Delete/{id?}")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var load = await _context.Loads
                        .Include(l => l.LoadProducts) // Include LoadProducts to delete them
                        .FirstOrDefaultAsync(l => l.LoadId == id);

                    if (load == null)
                    {
                        await transaction.RollbackAsync();
                        return NotFound();
                    }

                    // Remove all associated LoadProducts first
                    _context.LoadProducts.RemoveRange(load.LoadProducts);
                    await _context.SaveChangesAsync();

                    // Now remove the Load itself
                    _context.Loads.Remove(load);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                    TempData["SuccessMessage"] = "Load deleted successfully!";
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    TempData["ErrorMessage"] = "An error occurred while deleting the load: " + ex.Message;
                    Console.WriteLine($"Error deleting load: {ex.ToString()}");
                }
            }
            return RedirectToAction(nameof(Index));
        }

        private bool LoadExists(int id)
        {
            return _context.Loads.Any(e => e.LoadId == id);
        }
    }
}
