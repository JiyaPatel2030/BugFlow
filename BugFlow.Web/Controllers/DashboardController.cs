using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BugFlow.Web.Data;
using BugFlow.Web.Models;
using BugFlow.Web.Models.Enums;
using BugFlow.Web.ViewModels;

namespace BugFlow.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var currentUserId = _userManager.GetUserId(User);

            var bugsQuery = _context.Bugs
                .Include(b => b.ReportedBy)
                .Include(b => b.AssignedTo)
                .AsQueryable();

            var viewModel = new DashboardViewModel
            {
                TotalBugs = await bugsQuery.CountAsync(),
                OpenBugs = await bugsQuery.CountAsync(b => b.Status == BugStatus.New),
                InProgressBugs = await bugsQuery.CountAsync(b => b.Status == BugStatus.InProgress || b.Status == BugStatus.Assigned),
                ResolvedBugs = await bugsQuery.CountAsync(b => b.Status == BugStatus.Resolved),
                ClosedBugs = await bugsQuery.CountAsync(b => b.Status == BugStatus.Closed),

                MyAssignedBugs = await bugsQuery
                    .Where(b => b.AssignedToId == currentUserId)
                    .OrderByDescending(b => b.CreatedDate)
                    .Take(5)
                    .ToListAsync(),

                MyReportedBugs = await bugsQuery
                    .Where(b => b.ReportedById == currentUserId)
                    .OrderByDescending(b => b.CreatedDate)
                    .Take(5)
                    .ToListAsync(),

                RecentBugs = await bugsQuery
                    .OrderByDescending(b => b.CreatedDate)
                    .Take(5)
                    .ToListAsync()
            };

            return View(viewModel);
        }
    }
}