using Domain.Entities.DbEntities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace Infrastracture.Context
{
    public class ApplicationContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Stage> Stages { get; set; }
        public DbSet<StageAssignment> StageAssignments { get; set; }
        public DbSet<Commit> Commitments { get; set; }

        public ApplicationContext
            (DbContextOptions<ApplicationContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StageAssignment>()
                .HasKey(sa => new { sa.StageId, sa.UserId });

            modelBuilder.Entity<StageAssignment>()
                .HasOne(sa => sa.Stage)
                .WithMany(s => s.Assignments)
                .HasForeignKey(sa => sa.StageId);

            modelBuilder.Entity<StageAssignment>()
                .HasOne(sa => sa.User)
                .WithMany(u => u.StageAssignments)
                .HasForeignKey(sa => sa.UserId);

            modelBuilder.Entity<Commit>()
                .HasOne<StageAssignment>()
                .WithMany(sa => sa.Commits)
                .HasForeignKey(c => new { c.StageAssignmentStageId, c.StageAssignmentUserId });

            base.OnModelCreating(modelBuilder);
        }
    }
}
