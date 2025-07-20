using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EShift123.Data;
using EShift123.Models;
using System.Linq; // Required for .Any()
using System.Threading.Tasks; // Required for async/await

namespace EShift123.Controllers
{
    // Authorize only admins to manage products
    // [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Product
        // Displays a list of all products
        public async Task<IActionResult> Index()
        {
            // Fetch all products and EAGERLY LOAD the associated Customer
            var allProducts = await _context.Products
                                            .Include(p => p.Customer) // <--- This is crucial!
                                            .ToListAsync();

            return View(allProducts);
        }

        // GET: Product/Create
        // Displays the form to create a new product
        public IActionResult Create()
        {
            return View();
        }

        // POST: Product/Create
        // Handles the submission of the new product form
        [HttpPost]
        [ValidateAntiForgeryToken] // Protects against Cross-Site Request Forgery (CSRF) attacks
        public async Task<IActionResult> Create(
            [Bind("Name,Category,Description,WeightKg")] Product product) // [Bind] protects against overposting
        {
            if (ModelState.IsValid) // Check if model validation passes
            {
                _context.Products.Add(product); // Add the new product to the database context
                await _context.SaveChangesAsync(); // Save changes to the database
                return RedirectToAction(nameof(Index)); // Redirect to the list of products
            }
            return View(product); // If validation fails, return the view with validation errors
        }

        // GET: Product/Edit/5
        // Displays the form to edit an existing product
        public async Task<IActionResult> Edit(int? productId) // Parameter name matches the model's primary key
        {
            if (productId == null) // Check if an ID was provided in the URL
            {
                return NotFound(); // Return a 404 error if no ID
            }

            // Find the product by its primary key
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return NotFound(); // Return a 404 error if product not found
            }
            return View(product); // Return the view with the product data for editing
        }

        // POST: Product/Edit/5
        // Handles the submission of the edited product form
        [HttpPost]
        [ValidateAntiForgeryToken] // Protects against CSRF attacks
        public async Task<IActionResult> Edit(
            int productId, // ID from the URL, used for verification
            [Bind("ProductId,Name,Category,Description,WeightKg")] Product product) // [Bind] includes ProductId for update
        {
            // Security check: Ensure the ID from the URL matches the ID of the model being updated
            if (productId != product.ProductId)
            {
                return NotFound(); // Return 404 if there's an ID mismatch
            }

            if (ModelState.IsValid) // Check if model validation passes
            {
                try
                {
                    _context.Update(product); // Mark the product as modified
                    await _context.SaveChangesAsync(); // Save changes to the database
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Handle concurrency conflicts (e.g., another user modified the same record)
                    if (!_context.Products.Any(e => e.ProductId == product.ProductId))
                    {
                        return NotFound(); // Product might have been deleted by another process
                    }
                    else
                    {
                        throw; // Re-throw if it's a true concurrency issue or other database update error
                    }
                }
                return RedirectToAction(nameof(Index)); // Redirect to the list of products on success
            }
            return View(product); // If validation fails, return the view with validation errors
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Protect against CSRF attacks
        public async Task<IActionResult> ToggleProductValidation(int productId, bool isValid)
        {
            var product = await _context.Products.FindAsync(productId);

            if (product == null)
            {
                TempData["ErrorMessage"] = "Product not found.";
                return NotFound();
            }

            try
            {
                product.IsValid = isValid; // Update the IsValid property
                _context.Update(product); // Mark for update
                await _context.SaveChangesAsync(); // Save changes

                TempData["SuccessMessage"] = $"Product '{product.Name}' validation status updated to {(isValid ? "Valid" : "Not Valid")}.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating product validation status: {ex.Message}";
                // Log the exception for debugging
                Console.WriteLine($"Error toggling product validation: {ex.Message}");
            }

            return RedirectToAction(nameof(Index)); // Redirect back to the product list
        }

        // GET: Product/Details/5
        // Displays the details of a single product
        // GET: AdminProduct/Details/5
        public async Task<IActionResult> Details(int? productId)
        {
            if (productId == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                                        .Include(p => p.Customer) // <--- CRUCIAL: Include Customer data
                                        .FirstOrDefaultAsync(m => m.ProductId == productId);

            if (product == null)
            {
                // Consider a more informative message for admin if product exists but no customer
                TempData["ErrorMessage"] = "Product not found.";
                return NotFound();
            }

            return View(product);
        }

        // GET: Product/Delete/5
        // Displays the confirmation page for deleting a product
        public async Task<IActionResult> Delete(int? productId) // Parameter name matches the model's primary key
        {
            if (productId == null)
            {
                return NotFound();
            }

            // Find the product to be deleted
            var product = await _context.Products.FirstOrDefaultAsync(m => m.ProductId == productId);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Product/Delete/5
        // Handles the deletion confirmation
        [HttpPost, ActionName("Delete")] // ActionName specifies this method handles POST requests to "Delete"
        [ValidateAntiForgeryToken] // Protects against CSRF attacks
        public async Task<IActionResult> DeleteConfirmed(int productId) // Parameter name matches the model's primary key
        {
            // Find the product to remove
            var product = await _context.Products.FindAsync(productId);

            if (product == null) // Check if the product still exists
            {
                return RedirectToAction(nameof(Index)); // If not found, redirect gracefully (it was already deleted)
            }

            _context.Products.Remove(product); // Remove the product from the context
            await _context.SaveChangesAsync(); // Save changes to the database
            return RedirectToAction(nameof(Index)); // Redirect to the list of products
        }
    }
}
