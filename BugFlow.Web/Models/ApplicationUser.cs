using Microsoft.AspNetCore.Identity;

namespace BugFlow.Web.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;

        // Navigation properties (optional but useful)
        public ICollection<Bug> ReportedBugs { get; set; } = new List<Bug>();
        public ICollection<Bug> AssignedBugs { get; set; } = new List<Bug>();
    }
}