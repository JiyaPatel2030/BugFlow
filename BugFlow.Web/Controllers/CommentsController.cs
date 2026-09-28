using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using BugFlow.Web.Data;
using BugFlow.Web.Models;

namespace BugFlow.Web.Controllers
{
    [Authorize]
    public class CommentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CommentsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // POST: Comments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int bugId, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                TempData["ErrorMessage"] = "Comment text cannot be empty.";
                return RedirectToAction("Details", "Bugs", new { id = bugId });
            }

            var bug = await _context.Bugs.FindAsync(bugId);
            if (bug == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var comment = new Comment
            {
                BugId = bugId,
                UserId = userId,
                Text = text.Trim(),
                CreatedDate = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Comment added successfully.";
            return RedirectToAction("Details", "Bugs", new { id = bugId });
        }
    }
}
