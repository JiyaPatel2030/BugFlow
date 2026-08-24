using System.ComponentModel.DataAnnotations;
using BugFlow.Web.Models.Enums;

namespace BugFlow.Web.Models
{
    public class Bug
    {
        public int Id { get; set; }

        [Required, StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        public BugPriority Priority { get; set; } = BugPriority.Medium;

        public BugStatus Status { get; set; } = BugStatus.New;

        // Who reported it (Tester)
        [Required]
        public string ReportedById { get; set; } = string.Empty;
        public ApplicationUser? ReportedBy { get; set; }

        // Who it's assigned to (Developer) - nullable until Admin assigns it
        public string? AssignedToId { get; set; }
        public ApplicationUser? AssignedTo { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ResolvedDate { get; set; }

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}