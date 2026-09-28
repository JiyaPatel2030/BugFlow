using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugFlow.Web.Data;
using BugFlow.Web.Models;
using BugFlow.Web.Models.Enums;
using BugFlow.Web.ViewModels;

namespace BugFlow.Web.Controllers
{
    [Authorize]
    public class BugsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BugsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Bugs
        public async Task<IActionResult> Index(
            string? search,
            BugStatus? status,
            BugPriority? priority,
            string? assigneeId)
        {
            var query = _context.Bugs
                .Include(b => b.ReportedBy)
                .Include(b => b.AssignedTo)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var lowerSearch = search.Trim().ToLower();

                query = query.Where(b =>
                    b.Title.ToLower().Contains(lowerSearch) ||
                    b.Description.ToLower().Contains(lowerSearch));
            }

            if (status.HasValue)
            {
                query = query.Where(b => b.Status == status.Value);
            }

            if (priority.HasValue)
            {
                query = query.Where(b => b.Priority == priority.Value);
            }

            if (!string.IsNullOrWhiteSpace(assigneeId))
            {
                query = query.Where(b => b.AssignedToId == assigneeId);
            }

            var bugs = await query
                .OrderByDescending(b => b.CreatedDate)
                .ToListAsync();

            // Populate developers for filter dropdown
            var devUsers = await _userManager.GetUsersInRoleAsync("Developer");

            var devSelectList = devUsers.Select(u => new SelectListItem
            {
                Value = u.Id,
                Text = !string.IsNullOrWhiteSpace(u.FullName)
                    ? u.FullName
                    : u.Email,
                Selected = u.Id == assigneeId
            }).ToList();

            var viewModel = new BugListViewModel
            {
                Bugs = bugs,
                SearchQuery = search,
                SelectedStatus = status,
                SelectedPriority = priority,
                SelectedAssigneeId = assigneeId,
                Developers = devSelectList
            };

            return View(viewModel);
        }

        // GET: Bugs/Create
        [Authorize(Roles = "Tester,Admin")]
        public IActionResult Create()
        {
            var model = new BugCreateViewModel();
            return View(model);
        }

        // POST: Bugs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Tester,Admin")]
        public async Task<IActionResult> Create(BugCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var bug = new Bug
            {
                Title = model.Title.Trim(),
                Description = model.Description.Trim(),
                Priority = model.Priority,
                Status = BugStatus.New,
                ReportedById = userId,
                CreatedDate = DateTime.UtcNow
            };

            _context.Bugs.Add(bug);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"Bug #{bug.Id} reported successfully!";

            return RedirectToAction(nameof(Details), new { id = bug.Id });
        }

        // GET: Bugs/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var bug = await _context.Bugs
                .Include(b => b.ReportedBy)
                .Include(b => b.AssignedTo)
                .Include(b => b.Comments)
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bug == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);

            var isAdmin = User.IsInRole("Admin");
            var isTester = User.IsInRole("Tester");
            var isDeveloper = User.IsInRole("Developer");

            var viewModel = new BugDetailsViewModel
            {
                Bug = bug,
                Comments = bug.Comments
                    .OrderBy(c => c.CreatedDate)
                    .ToList(),

                IsAdmin = isAdmin,
                IsTester = isTester,
                IsDeveloper = isDeveloper,

                CanUserEdit =
                    isAdmin ||
                    (isTester && bug.ReportedById == userId),

                CanUserAssign = isAdmin,

                CanUserChangeStatus =
                    isAdmin ||
                    isDeveloper ||
                    isTester
            };

            return View(viewModel);
        }

        // GET: Bugs/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var bug = await _context.Bugs
                .Include(b => b.ReportedBy)
                .Include(b => b.AssignedTo)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bug == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);

            var isAdmin = User.IsInRole("Admin");
            var isTester = User.IsInRole("Tester");
            var isDeveloper = User.IsInRole("Developer");

            // Developers can only update status
            // for bugs assigned to them.
            if (!isAdmin &&
                isDeveloper &&
                bug.AssignedToId != userId)
            {
                TempData["ErrorMessage"] =
                    "You can only update status for bugs assigned to you.";

                return RedirectToAction(nameof(Details), new { id });
            }

            var devUsers =
                await _userManager.GetUsersInRoleAsync("Developer");

            if (!devUsers.Any())
            {
                // Fallback to all users if roles are not yet populated
                devUsers = await _userManager.Users.ToListAsync();
            }

            var devSelectList = devUsers.Select(u => new SelectListItem
            {
                Value = u.Id,
                Text = !string.IsNullOrWhiteSpace(u.FullName)
                    ? $"{u.FullName} ({u.Email})"
                    : u.Email,

                Selected = u.Id == bug.AssignedToId
            }).ToList();

            var viewModel = new BugEditViewModel
            {
                Id = bug.Id,
                Title = bug.Title,
                Description = bug.Description,
                Priority = bug.Priority,
                Status = bug.Status,
                AssignedToId = bug.AssignedToId,

                AvailableDevelopers = devSelectList,

                CanEditDetails =
                    isAdmin ||
                    (isTester && bug.ReportedById == userId),

                CanAssign = isAdmin,

                CanUpdateStatus =
                    isAdmin ||
                    isDeveloper ||
                    isTester
            };

            return View(viewModel);
        }

        // POST: Bugs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            BugEditViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            var bug = await _context.Bugs
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bug == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);

            var isAdmin = User.IsInRole("Admin");
            var isTester = User.IsInRole("Tester");
            var isDeveloper = User.IsInRole("Developer");

            // Admin or reporter can edit bug details
            if (isAdmin ||
                (isTester && bug.ReportedById == userId))
            {
                bug.Title = model.Title.Trim();
                bug.Description = model.Description.Trim();
                bug.Priority = model.Priority;
            }

            // Admin can assign developer
            if (isAdmin)
            {
                if (bug.AssignedToId != model.AssignedToId)
                {
                    bug.AssignedToId = model.AssignedToId;

                    // New bug becomes Assigned when
                    // a developer is assigned.
                    if (!string.IsNullOrEmpty(model.AssignedToId) &&
                        bug.Status == BugStatus.New)
                    {
                        bug.Status = BugStatus.Assigned;
                    }
                }
            }

            // Status update logic
            if (bug.Status != model.Status)
            {
                bug.Status = model.Status;

                if (model.Status == BugStatus.Resolved &&
                    bug.ResolvedDate == null)
                {
                    bug.ResolvedDate = DateTime.UtcNow;
                }
            }

            _context.Update(bug);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"Bug #{bug.Id} updated successfully!";

            return RedirectToAction(
                nameof(Details),
                new { id = bug.Id });
        }

        // POST: Bugs/Assign/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Assign(
            int id,
            string developerId)
        {
            var bug = await _context.Bugs.FindAsync(id);

            if (bug == null)
            {
                return NotFound();
            }

            bug.AssignedToId =
                string.IsNullOrWhiteSpace(developerId)
                    ? null
                    : developerId;

            // New bug becomes Assigned when
            // a developer is assigned.
            if (!string.IsNullOrEmpty(bug.AssignedToId) &&
                bug.Status == BugStatus.New)
            {
                bug.Status = BugStatus.Assigned;
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"Bug #{bug.Id} assignment updated!";

            return RedirectToAction(
                nameof(Details),
                new { id });
        }

        // POST: Bugs/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var bug = await _context.Bugs.FindAsync(id);

            if (bug != null)
            {
                _context.Bugs.Remove(bug);

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] =
                    $"Bug #{id} was deleted.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}