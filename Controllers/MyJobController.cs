using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EShift123.Data;
using EShift123.Models;
using EShift123.Models.ViewModels; // Add this line to access your new ViewModels

namespace EShift123.Controllers
{
    [Authorize(Roles = "Customer")]
    [Route("[controller]")]
    public class MyJobController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MyJobController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Helper method to get the current customer's ID from the authenticated user.
        private async Task<int?> GetCurrentCustomerIdAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return null;

            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
            return customer?.CustomerId;
        }

        // GET: /MyJob
        // Displays a list of jobs for the current customer.
        [HttpGet]
        [Route("Index")]
        [Route("")]
        public async Task<IActionResult> Index()
        {
            var customerId = await GetCurrentCustomerIdAsync();
            if (!customerId.HasValue)
            {
                TempData["ErrorMessage"] = "Your customer profile is incomplete. Please ensure your details are set up to view your jobs.";
                return RedirectToAction("Index", "Home");
            }
            var myJobs = await _context.Jobs
                                       .Where(j => j.CustomerId == customerId.Value)
                                       .Include(j => j.Customer)
                                       .OrderByDescending(j => j.JobDate)
                                       .ToListAsync();
            return View(myJobs);
        }

        // GET: /MyJob/RequestJob
        // Displays the form for requesting a new job, including initial load and product fields.
        [HttpGet]
        [Route("RequestJob")]
        public async Task<IActionResult> RequestJob()
        {
            var customerId = await GetCurrentCustomerIdAsync();
            if (!customerId.HasValue)
            {
                TempData["ErrorMessage"] = "Please complete your customer profile before requesting a job.";
                return RedirectToAction("Index", "Home");
            }

            // Initialize the ViewModel with default values and an initial empty load/product block.
            var model = new JobRequestViewModel
            {
                CustomerId = customerId.Value,
                JobDate = DateTime.Today,
                Loads = new List<LoadInputModel>
                {
                    new LoadInputModel
                    {
                        PickupDate = DateTime.Today,
                        Products = new List<ProductInputModel> { new ProductInputModel() } // Start with one product in the first load
                    }
                }
            };
            return View(model);
        }

        // POST: /MyJob/RequestJob
        // Processes the submitted job request, creating the Job, Loads, and LoadProducts.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("RequestJob")]
        public async Task<IActionResult> RequestJob(JobRequestViewModel model)
        {
            var customerId = await GetCurrentCustomerIdAsync();
            if (!customerId.HasValue || model.CustomerId != customerId.Value)
            {
                return Unauthorized(); // Ensure the submitted customer ID matches the authenticated user.
            }

            if (ModelState.IsValid)
            {
                // Use a database transaction to ensure atomicity for Job, Load, and Product creation.
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // 1. Create the Job
                        var job = new Job
                        {
                            CustomerId = model.CustomerId,
                            StartLocation = model.StartLocation,
                            Destination = model.Destination,
                            JobDate = model.JobDate,
                            Status = JobStatus.InProgress // Set initial job status
                        };
                        _context.Jobs.Add(job);
                        await _context.SaveChangesAsync(); // Save job to get its generated JobId

                        // 2. Iterate and create Loads for the newly created Job
                        foreach (var loadInput in model.Loads)
                        {
                            var load = new Load
                            {
                                JobId = job.JobId,
                                LoadNumber = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper(), // Generate a unique load number
                                Description = loadInput.Description,
                                WeightKg = loadInput.WeightKg,
                                PickupDate = loadInput.PickupDate,
                                Status = LoadStatus.Pending // Set initial load status
                            };
                            _context.Loads.Add(load);
                            await _context.SaveChangesAsync(); // Save load to get its generated LoadId

                            // 3. Iterate and create Products and LoadProducts for each Load
                            foreach (var productInput in loadInput.Products)
                            {
                                // Create a new Product (assuming products are newly defined for each job request)
                                var product = new Product
                                {
                                    CustomerId = model.CustomerId, // Product belongs to the customer who made the job request
                                    Name = productInput.Name,
                                    Category = productInput.Category,
                                    Description = productInput.Description,
                                    WeightKg = productInput.WeightKg
                                };
                                _context.Products.Add(product);
                                await _context.SaveChangesAsync(); // Save product to get its generated ProductId

                                // Create the many-to-many relationship entry between Load and Product
                                var loadProduct = new LoadProduct
                                {
                                    LoadId = load.LoadId,
                                    ProductId = product.ProductId,
                                    Quantity = productInput.Quantity
                                };
                                _context.LoadProducts.Add(loadProduct);
                            }
                        }

                        await _context.SaveChangesAsync(); // Save all LoadProduct entries
                        await transaction.CommitAsync(); // Commit the transaction if all operations succeed

                        TempData["SuccessMessage"] = "Your job request has been submitted successfully, including all loads and products!";
                        //return RedirectToAction(nameof(Index));
                        //return Redirect("https://localhost:7198/Customer/Home");
                        return Redirect($"https://localhost:7198/Customer/Home?message={Uri.EscapeDataString(TempData["SuccessMessage"].ToString())}");

                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync(); // Rollback on error
                        ModelState.AddModelError("", "An error occurred while saving your job request. Please try again. If the issue persists, contact support.");
                        // Log the full exception for debugging purposes.
                        Console.WriteLine($"Error during job request: {ex.ToString()}");
                    }
                }
            }

            // If ModelState is not valid or an exception occurred, redisplay the form with current data.
            // Ensure collections are re-initialized if they become null (e.g., during model binding failures).
            if (model.Loads == null || model.Loads.Count == 0)
            {
                model.Loads = new List<LoadInputModel> { new LoadInputModel { Products = new List<ProductInputModel> { new ProductInputModel() } } };
            }
            else
            {
                foreach (var load in model.Loads)
                {
                    if (load.Products == null || load.Products.Count == 0)
                    {
                        load.Products = new List<ProductInputModel> { new ProductInputModel() };
                    }
                }
            }
            return View(model);
        }

        // GET: /MyJob/Details/5
        // Displays details of a specific job, now including its loads and products.
        [HttpGet]
        [Route("Details/{id?}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var customerId = await GetCurrentCustomerIdAsync();
            if (!customerId.HasValue) return Unauthorized();

            var job = await _context.Jobs
                .Include(j => j.Customer)
                .Include(j => j.Loads) // Eager load the Loads related to the Job
                    .ThenInclude(l => l.LoadProducts) // Then load the LoadProduct join table entries for each Load
                        .ThenInclude(lp => lp.Product) // Then load the actual Product details for each LoadProduct
                .FirstOrDefaultAsync(m => m.JobId == id && m.CustomerId == customerId.Value);

            if (job == null) return NotFound();
            return View(job);
        }

        // GET: /MyJob/Edit/5
        // Allows customer to edit a job if its status is 'InProgress'.
        [HttpGet]
        [Route("Edit/{id?}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var customerId = await GetCurrentCustomerIdAsync();
            if (!customerId.HasValue) return Unauthorized();
            var job = await _context.Jobs
                .FirstOrDefaultAsync(m => m.JobId == id && m.CustomerId == customerId.Value);
            if (job == null) return NotFound();
            if (job.Status != JobStatus.InProgress)
            {
                TempData["ErrorMessage"] = $"Job cannot be edited as it is already '{job.Status}'.";
                return RedirectToAction(nameof(Details), new { id = job.JobId });
            }
            return View(job);
        }

        // POST: /MyJob/Edit/5
        // Processes job edits.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Edit/{id?}")]
        public async Task<IActionResult> Edit(int id, [Bind("JobId,StartLocation,Destination,JobDate")] Job job)
        {
            if (id != job.JobId) return NotFound();
            var customerId = await GetCurrentCustomerIdAsync();
            if (!customerId.HasValue) return Unauthorized();
            var jobToUpdate = await _context.Jobs.FindAsync(id);
            if (jobToUpdate == null || jobToUpdate.CustomerId != customerId.Value) return NotFound();
            if (jobToUpdate.Status != JobStatus.InProgress)
            {
                TempData["ErrorMessage"] = $"Job cannot be edited as it is already '{jobToUpdate.Status}'.";
                return RedirectToAction(nameof(Details), new { id = jobToUpdate.JobId });
            }
            // ModelState.Remove("Customer"); // Not strictly necessary if Customer is not bound
            // ModelState.Remove("Status"); // Status is not bound from form
            if (ModelState.IsValid)
            {
                try
                {
                    jobToUpdate.StartLocation = job.StartLocation;
                    jobToUpdate.Destination = job.Destination;
                    jobToUpdate.JobDate = job.JobDate;
                    _context.Update(jobToUpdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!JobExists(job.JobId)) return NotFound();
                    else throw;
                }
                TempData["SuccessMessage"] = $"Job updated successfully!";
                return RedirectToAction(nameof(Details), new { id = jobToUpdate.JobId });
            }
            return View(job);
        }

        // GET: /MyJob/Cancel/5
        // Displays confirmation for job cancellation.
        [HttpGet]
        [Route("Cancel/{id?}")]
        public async Task<IActionResult> Cancel(int? id)
        {
            if (id == null) return NotFound();
            var customerId = await GetCurrentCustomerIdAsync();
            if (!customerId.HasValue) return Unauthorized();
            var job = await _context.Jobs
                .Include(j => j.Customer)
                .FirstOrDefaultAsync(m => m.JobId == id && m.CustomerId == customerId.Value);
            if (job == null) return NotFound();
            if (job.Status != JobStatus.InProgress)
            {
                TempData["ErrorMessage"] = $"Job cannot be cancelled as it is already '{job.Status}'.";
                return RedirectToAction(nameof(Details), new { id = job.JobId });
            }
            return View(job);
        }

        // POST: /MyJob/Cancel/5
        // Confirms and processes job cancellation.
        [HttpPost, ActionName("Cancel")]
        [ValidateAntiForgeryToken]
        [Route("Cancel/{id?}")]
        public async Task<IActionResult> CancelConfirmed(int id)
        {
            var customerId = await GetCurrentCustomerIdAsync();
            if (!customerId.HasValue) return Unauthorized();
            var job = await _context.Jobs.FindAsync(id);
            if (job == null || job.CustomerId != customerId.Value) return NotFound();
            if (job.Status != JobStatus.InProgress)
            {
                TempData["ErrorMessage"] = $"Job cannot be cancelled as it is already '{job.Status}'.";
                return RedirectToAction(nameof(Details), new { id = job.JobId });
            }
            job.Status = JobStatus.Cancelled;
            _context.Update(job);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Job has been cancelled successfully!";
            return RedirectToAction(nameof(Index));
        }

        // Helper to check if a job exists.
        private bool JobExists(int id)
        {
            return _context.Jobs.Any(e => e.JobId == id);
        }
    }
}

