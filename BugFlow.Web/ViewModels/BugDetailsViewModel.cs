using BugFlow.Web.Models;

namespace BugFlow.Web.ViewModels
{
    public class BugDetailsViewModel
    {
        public Bug Bug { get; set; } = null!;
        public List<Comment> Comments { get; set; } = new List<Comment>();
        public string NewCommentText { get; set; } = string.Empty;
        public bool CanUserEdit { get; set; }
        public bool CanUserAssign { get; set; }
        public bool CanUserChangeStatus { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsTester { get; set; }
        public bool IsDeveloper { get; set; }
    }
}
