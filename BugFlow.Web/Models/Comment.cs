using System.ComponentModel.DataAnnotations;

namespace BugFlow.Web.Models
{
    public class Comment
    {
        public int Id { get; set; }

        public int BugId { get; set; }
        public Bug? Bug { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        [Required, StringLength(1000)]
        public string Text { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}