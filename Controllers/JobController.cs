using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // Required for SelectList
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization; // For [Authorize]

// Make sure these namespaces match your project structure
using EShift123.Data; // Assuming your DbContext is ApplicationDbContext here
using EShift123.Models; // Your Job, Customer, and JobStatus models

namespace EShift123.Controllers
{
    [Authorize(Roles = "Admin")] // Only Admins can manage Jobs
    public class JobController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Constructor: Injects the database context
        public JobController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Job
        // Displays a list of all jobs, eagerly loading the associated Customer
        public async Task<IActionResult> Index(string status, int pageNumber = 1)
        {
            //return View(await _context.Jobs.Include(j => j.Customer).ToListAsync());
            int pageSize = 5;
            IQueryable<Job> jobs = _context.Jobs.Include(j => j.Customer);

            if (!string.IsNullOrEmpty(status))
            {
                // Attempt to parse the string 'status' into the JobStatus enum
                if (Enum.TryParse(status, out JobStatus jobStatusEnum))
                {
                    jobs = jobs.Where(j => j.Status == jobStatusEnum); 
                }
            }

            int totalCount = await jobs.CountAsync();
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize); // Use pageSize variable

            jobs = jobs.OrderByDescending(j => j.JobDate)
                       .Skip((pageNumber - 1) * pageSize) // Use pageSize variable
                       .Take(pageSize); // Use pageSize variable

            jobs = jobs.OrderByDescending(j => j.JobDate);

            ViewData["CurrentFilterStatus"] = status;
            ViewData["CurrentPage"] = pageNumber;
            ViewData["TotalPages"] = totalPages;
            ViewData["HasPreviousPage"] = (pageNumber > 1);
            ViewData["HasNextPage"] = (pageNumber < totalPages);

            return View(await jobs.ToListAsync());
        }

        // GET: Job/Details/5
        // Displays the details of a specific job, eagerly loading the associated Customer and Loads
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var job = await _context.Jobs
                .Include(j => j.Customer) // Include Customer details if needed
                .Include(j => j.Loads) // Include the Loads for the Job
                    .ThenInclude(l => l.TransportUnit) // Then include the TransportUnit for each Load
                .Include(j => j.Loads) // Re-Include Loads to start a new ThenInclude chain for LoadProducts
                    .ThenInclude(l => l.LoadProducts) // Then include LoadProducts for each Load
                        .ThenInclude(lp => lp.Product) // Then include the Product for each LoadProduct
                .FirstOrDefaultAsync(m => m.JobId == id);

            if (job == null)
            {
                return NotFound();
            }

            return View(job);
        }

        // GET: Job/Create
        // Displays the form to create a new job, populating the Customer dropdown
        public IActionResult Create()
        {
            // Populate a SelectList for the Customer dropdown.
            // Ensure "CustomerName" matches the actual property name in your Customer model.
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerName");
            return View();
        }

        // POST: Job/Create
        // Handles the submission of the new job form
        [HttpPost]
        [ValidateAntiForgeryToken] // Protects against Cross-Site Request Forgery attacks
        // Use [Bind] here to explicitly allow CustomerId to be set during creation
        public async Task<IActionResult> Create([Bind("CustomerId,StartLocation,Destination,JobDate,Status")] Job job)
        {
            if (ModelState.IsValid)
            {
                _context.Add(job); // Add the new job to the context
                await _context.SaveChangesAsync(); // Save changes to the database
                TempData["SuccessMessage"] = "Job created successfully!"; // Add success message
                return RedirectToAction(nameof(Index)); // Redirect to the job list
            }
            // If ModelState is not valid, re-populate the SelectList before returning the view
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerName", job.CustomerId);
            return View(job); // Return the view with validation errors
        }

        // GET: Job/Edit/5
        // Displays the form to edit an existing job.
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Find the job by ID, INCLUDING its Customer to display the Customer's name in the view
            var job = await _context.Jobs
                .Include(j => j.Customer) // IMPORTANT: Include Customer to ensure Model.Customer is not null in the view
                .FirstOrDefaultAsync(m => m.JobId == id);

            if (job == null)
            {
                return NotFound();
            }

            // No need to populate ViewData["CustomerId"] for a dropdown here, as Customer is read-only.
            return View(job);
        }

        // POST: Job/Edit/5
        // Handles the submission of the edited job form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Job job)
        {
            ModelState.Remove("CustomerId");
            ModelState.Remove("Customer");

            if (!ModelState.IsValid)
            {
                var existingJobForDisplay = await _context.Jobs
                    .Include(j => j.Customer)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(j => j.JobId == job.JobId);

                if (existingJobForDisplay != null)
                {
                    job.Customer = existingJobForDisplay.Customer;
                }
                ModelState.AddModelError("", "Please correct the highlighted errors and try again.");
                return View(job);
            }

            var existingJob = await _context.Jobs
                .Include(j => j.Loads) // Eagerly load the associated loads
                .FirstOrDefaultAsync(j => j.JobId == job.JobId);

            if (existingJob == null)
            {
                TempData["ErrorMessage"] = "The job you are trying to edit was not found or has been deleted.";
                return NotFound();
            }

            // --- NEW LOGIC START ---
            // Only allow status change if the new status is different from the old one
            if (existingJob.Status != job.Status)
            {
                if (job.Status == JobStatus.Completed)
                {
                    // Check if ANY associated load is NOT 'Assigned'
                    if (existingJob.Loads != null && existingJob.Loads.Any(l => l.Status != LoadStatus.Assigned))
                    {
                        ModelState.AddModelError("Status", "Cannot change job status to 'Completed' unless all associated loads are in 'Assigned' status.");
                        // Re-fetch customer for display in case of validation error
                        var jobWithError = await _context.Jobs.Include(j => j.Customer).AsNoTracking().FirstOrDefaultAsync(j => j.JobId == job.JobId);
                        if (jobWithError != null)
                        {
                            job.Customer = jobWithError.Customer;
                        }
                        return View(job);
                    }
                }
            }
            // --- NEW LOGIC END ---

            try
            {
                existingJob.StartLocation = job.StartLocation;
                existingJob.Destination = job.Destination;
                existingJob.JobDate = job.JobDate;
                existingJob.Status = job.Status; // Update the status only after validation
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Job updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!JobExists(job.JobId))
                {
                    TempData["ErrorMessage"] = "The job was deleted by another user while you were editing.";
                    return NotFound();
                }
                else
                {
                    ModelState.AddModelError("", "The job has been modified by another user. Please review the changes and try again.");
                    var jobWithError = await _context.Jobs.Include(j => j.Customer).AsNoTracking().FirstOrDefaultAsync(j => j.JobId == job.JobId);
                    if (jobWithError != null)
                    {
                        job.Customer = jobWithError.Customer;
                    }
                    return View(job);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An unexpected error occurred while updating the job. Please try again. " + ex.Message);
                Console.WriteLine($"Error updating job: {ex.ToString()}");

                var existingJobWithCustomer = await _context.Jobs.Include(j => j.Customer).AsNoTracking().FirstOrDefaultAsync(j => j.JobId == job.JobId);
                if (existingJobWithCustomer != null)
                {
                    job.Customer = existingJobWithCustomer.Customer;
                }
                return View(job);
            }
        }

        // GET: Job/Delete/5
        // Displays a confirmation page for deleting a job, eagerly loading the associated Customer
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Find the job by ID, including its Customer for display on the confirmation page
            var job = await _context.Jobs
                .Include(j => j.Customer)
                .FirstOrDefaultAsync(m => m.JobId == id);

            if (job == null)
            {
                return NotFound();
            }

            return View(job);
        }

        // POST: Job/Delete/5
        // Handles the deletion of a job after confirmation
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Use a transaction for consistency, especially if deleting related entities might be involved
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Fetch the job, potentially including related loads if you have cascade delete or custom logic
                    var job = await _context.Jobs
                        .Include(j => j.Loads) // Include loads if you have related loads that should also be handled (e.g., deleted)
                        .FirstOrDefaultAsync(j => j.JobId == id);

                    if (job != null)
                    {
                        // Example: If you want to delete associated loads when a job is deleted, uncomment this:
                        // _context.Loads.RemoveRange(job.Loads);

                        _context.Jobs.Remove(job); // Remove the job from the context
                        await _context.SaveChangesAsync(); // Save changes
                        await transaction.CommitAsync(); // Commit the transaction
                        TempData["SuccessMessage"] = "Job deleted successfully!"; // Add success message
                    }
                    else
                    {
                        await transaction.RollbackAsync(); // Rollback if job not found to prevent partial operations
                        TempData["ErrorMessage"] = "Job not found.";
                    }
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(); // Rollback on error
                    TempData["ErrorMessage"] = "An error occurred while deleting the job: " + ex.Message;
                    Console.WriteLine($"Error deleting job: {ex.ToString()}"); // Log the full exception
                }
            }
            return RedirectToAction(nameof(Index));
        }

        // Helper method to check if a job exists
        private bool JobExists(int id)
        {
            return _context.Jobs.Any(e => e.JobId == id);
        }
    }
}