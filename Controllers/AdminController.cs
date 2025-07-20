using EShift123.Data;
using EShift123.Models;  // Adjust namespaces as needed
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace EShift123.Controllers;

[Authorize(Roles = "Admin")]

public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Dashboard()
    {
        var totalJobs = await _context.Jobs.CountAsync();

        var inProgressJobs = await _context.Jobs.CountAsync(j => j.Status == JobStatus.InProgress);
        var completedJobs = await _context.Jobs.CountAsync(j => j.Status == JobStatus.Completed);
        var cancelledJobs = await _context.Jobs.CountAsync(j => j.Status == JobStatus.Cancelled);

        var model = new DashboardViewModel
        {
            TotalJobs = totalJobs,
            ActiveJobs = inProgressJobs,   // Treat InProgress as Active
            CompletedJobs = completedJobs,
            CancelledJobs = cancelledJobs
        };

        return View(model);
    }
}

