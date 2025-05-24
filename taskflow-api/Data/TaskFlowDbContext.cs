using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using taskflow_api.Entity;

namespace taskflow_api.Data
{
    public class TaskFlowDbContext : IdentityUserContext<User, Guid>
    {
        public TaskFlowDbContext()
        {
        }

        public TaskFlowDbContext(DbContextOptions<TaskFlowDbContext> options) : base(options)
        {
        }
        //Define DbSet for each entity
        public DbSet<Project> Projects { get; set; } = null!;
        public DbSet<ProjectMember> ProjectMembers { get; set; } = null!;
        public DbSet<Board> Boards { get; set; } = null!;
        public DbSet<Sprint> Sprints { get; set; } = null!;
        public DbSet<TaskProject> TaskProjects { get; set; } = null!;
        public DbSet<TaskUser> TaskUsers { get; set; } = null!;
        public DbSet<Issue> Issues { get; set; } = null!;
        public DbSet<LogProject> LogProjects { get; set; } = null!;




        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Ignore<IdentityRole>();
            modelBuilder.Ignore<IdentityUserClaim<Guid>>();
            modelBuilder.Ignore<IdentityUserLogin<Guid>>();
            modelBuilder.Ignore<IdentityUserToken<Guid>>();
            modelBuilder.Ignore<IdentityUserRole<Guid>>();
            modelBuilder.Ignore<IdentityRoleClaim<Guid>>();

            //modelBuilder.Entity<User>().Ignore(x => x.NormalizedUserName);
            //modelBuilder.Entity<User>().Ignore(x => x.NormalizedEmail);
            //modelBuilder.Entity<User>().Ignore(x => x.EmailConfirmed);
            //modelBuilder.Entity<User>().Ignore(x => x.SecurityStamp);
            //modelBuilder.Entity<User>().Ignore(x => x.ConcurrencyStamp);
            modelBuilder.Entity<User>().Ignore(x => x.PhoneNumberConfirmed);
            modelBuilder.Entity<User>().Ignore(x => x.TwoFactorEnabled);
            modelBuilder.Entity<User>().Ignore(x => x.LockoutEnd);
            modelBuilder.Entity<User>().Ignore(x => x.LockoutEnabled);
            modelBuilder.Entity<User>().Ignore(x => x.AccessFailedCount);

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
