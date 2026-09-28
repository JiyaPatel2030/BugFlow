using System.ComponentModel.DataAnnotations;
using BugFlow.Web.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BugFlow.Web.ViewModels
{
    public class BugEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(150, ErrorMessage = "Title cannot exceed 150 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; } = string.Empty;

        public BugPriority Priority { get; set; }

        public BugStatus Status { get; set; }

        public string? AssignedToId { get; set; }

        public List<SelectListItem> AvailableDevelopers { get; set; } = new List<SelectListItem>();

        public bool CanEditDetails { get; set; }
        public bool CanAssign { get; set; }
        public bool CanUpdateStatus { get; set; }
    }
}
