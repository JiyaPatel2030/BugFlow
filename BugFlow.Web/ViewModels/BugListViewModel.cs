using BugFlow.Web.Models;
using BugFlow.Web.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BugFlow.Web.ViewModels
{
    public class BugListViewModel
    {
        public List<Bug> Bugs { get; set; } = new List<Bug>();
        public string? SearchQuery { get; set; }
        public BugStatus? SelectedStatus { get; set; }
        public BugPriority? SelectedPriority { get; set; }
        public string? SelectedAssigneeId { get; set; }
        public List<SelectListItem> Developers { get; set; } = new List<SelectListItem>();
    }
}
