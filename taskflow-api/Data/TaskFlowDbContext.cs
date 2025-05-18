using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using taskflow_api.Entity;

namespace taskflow_api.Data
{
    public class TaskFlowDbContext : IdentityUserContext<User>
    {
        public TaskFlowDbContext()
        {
        }
        public TaskFlowDbContext(DbContextOptions<TaskFlowDbContext> options) : base(options)
        {
        }
        //Define DbSet for each entity
        public DbSet<Entity.Project> Projects { get; set; } = null!;
        public DbSet<Entity.ProjectMember> ProjectMembers { get; set; } = null!;
        public DbSet<Entity.Board> Boards { get; set; } = null!;
        public DbSet<Entity.Sprint> Sprints { get; set; } = null!;
        public DbSet<Entity.TaskProject> TaskProjects { get; set; } = null!;
        public DbSet<Entity.TaskUser> TaskUsers { get; set; } = null!;
        public DbSet<Entity.Issue> Issues { get; set; } = null!;
        public DbSet<Entity.LogProject> LogProjects { get; set; } = null!;




        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //1 project có nhìu board
            modelBuilder.Entity<Board>()
                .HasOne(b => b.Project)
                .WithMany(p => p.Boards)
                .HasForeignKey(b => b.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            //1 project có nhìu sprint
            modelBuilder.Entity<Sprint>()
                .HasOne(s => s.Project)
                .WithMany(p => p.Sprints)
                .HasForeignKey(s => s.ProjectId)
                .OnDelete(DeleteBehavior.NoAction);

            //1 board có nhìu task
            modelBuilder.Entity<TaskProject>()
                .HasOne(t => t.Board)
                .WithMany(b => b.TaskProject)
                .HasForeignKey(t => t.BoardId)
                .OnDelete(DeleteBehavior.SetNull);

            //1 sprint có nhìu task
            modelBuilder.Entity<TaskProject>()
                .HasOne(t => t.Sprint)
                .WithMany(s => s.TaskProject)
                .HasForeignKey(t => t.SprintId)
                .OnDelete(DeleteBehavior.SetNull);

            //1 project có nhìu task
            modelBuilder.Entity<TaskProject>()
                .HasOne(t => t.Project)
                .WithMany(p => p.TaskProject)
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.NoAction);

            //1 task có nhìu user
            modelBuilder.Entity<TaskUser>()
               .HasOne(tu => tu.Task)
               .WithMany(tp => tp.taskUsers)
               .HasForeignKey(tu => tu.TaskId)
               .OnDelete(DeleteBehavior.Cascade);

            //1 user có nhìu task
             modelBuilder.Entity<TaskUser>()
                .HasOne(tu => tu.ProjectMember)
                .WithMany(pm => pm.taskUsers)
                .HasForeignKey(tu => tu.ProjectMemberID)
                .OnDelete(DeleteBehavior.Cascade);

            //1 task có nhìu issue
            modelBuilder.Entity<Issue>()
                .HasOne(i => i.TaskProject)
                .WithMany(tp => tp.issues)
                .HasForeignKey(i => i.TaskProjectID)
                .OnDelete(DeleteBehavior.Cascade);

            

        }
    }
       
}
