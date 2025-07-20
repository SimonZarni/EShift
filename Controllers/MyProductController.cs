using EShift123.Data;
using EShift123.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims; // Required for HttpContext.User.FindFirstValue
using System.Threading.Tasks;

namespace EShift123.Controllers
{
    // Authorize only customers to access this part of product management
    [Authorize(Roles = "Customer")]
    public class MyProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MyProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Helper method to get the current customer's ID from the authenticated user
        private async Task<int?> GetCurrentCustomerIdAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get the UserId from Identity
            if (string.IsNullOrEmpty(userId)) return null;

            // Find the Customer associated with this Identity UserId
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
            return customer?.CustomerId;
        }

        // GET: MyProduct/Index (for Customer's "My Products" view)
        // Displays a list of products belonging to the logged-in customer
        public async Task<IActionResult> Index()
        {
            var customerId = await GetCurrentCustomerIdAsync();
            if (!customerId.HasValue)
            {
                TempData["ErrorMessage"] = "Could not retrieve customer information. Please log in.";
                return RedirectToAction("Login", "Account");
            }

            var customerProducts = await _context.Products
                                                .Where(p => p.CustomerId == customerId.Value)
                                                .ToListAsync();

            return View(customerProducts);
        }

        // GET: MyProduct/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: MyProduct/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Name,Category,Description,WeightKg")] Product product)
        {
            var customerId = await GetCurrentCustomerIdAsync();
            if (!customerId.HasValue)
            {
                TempData["ErrorMessage"] = "Customer not authenticated. Please log in.";
                return RedirectToAction("Login", "Account");
            }

            product.CustomerId = customerId.Value;
            ModelState.Remove(nameof(product.Customer));
            ModelState.Remove(nameof(product.ProductId));

            if (ModelState.IsValid)
            {
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Product added successfully!";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = "Please correct the errors in the form.";
            return View(product);
        }

        // GET: MyProduct/Details/5
        public async Task<IActionResult> Details(int? productId)
        {
            var customerId = await GetCurrentCustomerIdAsync();
            if (!customerId.HasValue) return Unauthorized();

            if (productId == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                                        .FirstOrDefaultAsync(m => m.ProductId == productId && m.CustomerId == customerId.Value);

            if (product == null)
            {
                TempData["ErrorMessage"] = "Product not found or you do not have permission to view it.";
                return NotFound();
            }

            return View(product);
        }

        // GET: MyProduct/Edit/5
        [Route("MyProduct/Edit/{productId}")]
        public async Task<IActionResult> Edit(int? productId)
        {
            var customerId = await GetCurrentCustomerIdAsync();
            if (!customerId.HasValue) return Unauthorized();

            if (productId == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FirstOrDefaultAsync(m => m.ProductId == productId && m.CustomerId == customerId.Value);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Product not found or you do not have permission to edit it.";
                return NotFound();
            }
            return View(product);
        }

        // POST: MyProduct/Edit/5
        // FIX: Explicitly define the route to ensure productId from the URL is matched.
        // The [FromRoute] attribute ensures productId is looked for in the route data.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("MyProduct/Edit/{productId}")] // <-- THIS IS THE KEY ADDITION
        public async Task<IActionResult> Edit(
            [FromRoute] int productId, // <-- Also add [FromRoute] for clarity
            [Bind("ProductId,Name,Category,Description,WeightKg")] Product product)
        {
            var customerId = await GetCurrentCustomerIdAsync();
            if (!customerId.HasValue) return Unauthorized();

            // Ensure the product's CustomerId matches the logged-in customer's ID for security
            // Note: product.CustomerId might not be directly bound if not in the [Bind] list.
            // We ensure it's correct later.
            if (productId != product.ProductId)
            {
                TempData["ErrorMessage"] = "Security check failed: Product ID mismatch.";
                return NotFound(); // This handles if someone tampers with the hidden input ProductId
            }

            // Re-fetch the existing product to get its CustomerId (which is not sent via bind)
            var existingProduct = await _context.Products.AsNoTracking().FirstOrDefaultAsync(e => e.ProductId == productId);
            if (existingProduct == null || existingProduct.CustomerId != customerId.Value)
            {
                TempData["ErrorMessage"] = "Unauthorized attempt to edit product or product not found.";
                return NotFound();
            }

            // Set the CustomerId on the inbound 'product' model from the existing product to ensure ownership
            product.CustomerId = existingProduct.CustomerId; // Re-set CustomerId from the database owner
            // Remove navigation property from ModelState to avoid validation issues if not bound
            ModelState.Remove(nameof(product.Customer));


            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product); // Mark the product as modified
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Products.AnyAsync(e => e.ProductId == product.ProductId && e.CustomerId == customerId.Value))
                    {
                        TempData["ErrorMessage"] = "Product not found or already deleted.";
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["SuccessMessage"] = "Product updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            TempData["ErrorMessage"] = "Please correct the errors in the form.";
            return View(product);
        }

        // GET: MyProduct/Delete/5
        public async Task<IActionResult> Delete(int? productId)
        {
            var customerId = await GetCurrentCustomerIdAsync();
            if (!customerId.HasValue) return Unauthorized();

            if (productId == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FirstOrDefaultAsync(m => m.ProductId == productId && m.CustomerId == customerId.Value);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Product not found or you do not have permission to delete it.";
                return NotFound();
            }

            return View(product);
        }

        // POST: MyProduct/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        // FIX: Explicitly define the route for the DeleteConfirmed POST action as well, for consistency
        [Route("MyProduct/Delete/{productId}")] // <-- ADD THIS FOR CONSISTENCY
        public async Task<IActionResult> DeleteConfirmed([FromRoute] int productId) // <-- ADD [FromRoute]
        {
            var customerId = await GetCurrentCustomerIdAsync();
            if (!customerId.HasValue) return Unauthorized();

            var product = await _context.Products.FirstOrDefaultAsync(m => m.ProductId == productId && m.CustomerId == customerId.Value);

            if (product == null)
            {
                TempData["ErrorMessage"] = "Product not found or already deleted.";
                return RedirectToAction(nameof(Index));
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Product deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
