using System.ComponentModel.DataAnnotations;
using BugFlow.Web.Models.Enums;

namespace BugFlow.Web.ViewModels
{
    public class BugCreateViewModel
    {
        [Required(ErrorMessage = "Bug title is required")]
        [StringLength(150, ErrorMessage = "Title cannot exceed 150 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Bug description is required")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Priority selection is required")]
        public BugPriority Priority { get; set; } = BugPriority.Medium;
    }
}
