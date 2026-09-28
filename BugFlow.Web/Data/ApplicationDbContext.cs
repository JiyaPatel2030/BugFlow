using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BugFlow.Web.Models;

namespace BugFlow.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Bug> Bugs { get; set; }
        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // required — sets up Identity tables

            // Bug -> ReportedBy (restrict delete so deleting a user doesn't cascade-delete bugs)
            builder.Entity<Bug>()
                .HasOne(b => b.ReportedBy)
                .WithMany(u => u.ReportedBugs)
                .HasForeignKey(b => b.ReportedById)
                .OnDelete(DeleteBehavior.Restrict);

            // Bug -> AssignedTo
            //one user many bugs
            builder.Entity<Bug>()
                .HasOne(b => b.AssignedTo)
                .WithMany(u => u.AssignedBugs)
                .HasForeignKey(b => b.AssignedToId)
                .OnDelete(DeleteBehavior.Restrict);

            // Comment -> Bug (cascade delete is fine here — comments belong to the bug)
            //1 bug many comments
            builder.Entity<Comment>()
                .HasOne(c => c.Bug)
                .WithMany(b => b.Comments)
                .HasForeignKey(c => c.BugId)
                .OnDelete(DeleteBehavior.Cascade);

            // Comment -> User
            // 1 user many comments
            builder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Store enums as strings in DB (more readable than raw ints)
            builder.Entity<Bug>()
                .Property(b => b.Status)
                .HasConversion<string>();

            builder.Entity<Bug>()
                .Property(b => b.Priority)
                .HasConversion<string>();
        }
    }
}