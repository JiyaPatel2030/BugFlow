using BugFlow.Web.Models;

namespace BugFlow.Web.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalBugs { get; set; }
        public int OpenBugs { get; set; }        // Status == New
        public int InProgressBugs { get; set; }
        public int ResolvedBugs { get; set; }
        public int ClosedBugs { get; set; }

        public List<Bug> MyAssignedBugs { get; set; } = new();
        public List<Bug> MyReportedBugs { get; set; } = new();
        public List<Bug> RecentBugs { get; set; } = new();
    }
}