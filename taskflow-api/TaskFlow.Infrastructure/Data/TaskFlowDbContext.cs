using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Infrastructure.Data
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
        public DbSet<RefeshToken> RefeshTokens { get; set; } = null!;
        public DbSet<VerifyToken> VerifyTokens { get; set; } = null!;
        public DbSet<UserBans> UserBans { get; set; } = null!;
        public DbSet<UserReports> UserReports { get; set; } = null!;
        public DbSet<UserAppeals> UserAppeals { get; set; } = null!;
        public DbSet<Labels> Labels { get; set; } = null!;
        public DbSet<TaskLabels> TaskLabels { get; set; } = null!;
        public DbSet<TaskComment> TaskComments { get; set; } = null!;


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

            //Project <-> Board
            modelBuilder.Entity<Board>()
                .HasOne(b => b.Project)
                .WithMany(p => p.Boards)
                .HasForeignKey(b => b.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            // Project <-> Sprint
            modelBuilder.Entity<Sprint>()
                .HasOne(s => s.Project)
                .WithMany(p => p.Sprints)
                .HasForeignKey(s => s.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            // Board <-> TaskProject
            modelBuilder.Entity<TaskProject>()
                .HasOne(t => t.Board)
                .WithMany(b => b.TaskProject)
                .HasForeignKey(t => t.BoardId)
                .OnDelete(DeleteBehavior.Restrict);

            //Sprint <-> TaskProject
            modelBuilder.Entity<TaskProject>()
                .HasOne(t => t.Sprint)
                .WithMany(s => s.TaskProject)
                .HasForeignKey(t => t.SprintId)
                .OnDelete(DeleteBehavior.Restrict);

            // Project <-> Task
            modelBuilder.Entity<TaskProject>()
                .HasOne(t => t.Project)
                .WithMany(p => p.TaskProject)
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            //TaskProject <-> TaskUser
            modelBuilder.Entity<TaskUser>()
               .HasOne(tu => tu.Task)
               .WithMany(tp => tp.TaskUsers)
               .HasForeignKey(tu => tu.TaskId)
               .OnDelete(DeleteBehavior.Restrict);

            //ProjectMember <-> TaskUser
            modelBuilder.Entity<TaskUser>()
               .HasOne(tu => tu.ProjectMember)
               .WithMany(pm => pm.taskUsers)
               .HasForeignKey(tu => tu.Implementer)
               .OnDelete(DeleteBehavior.Restrict);

            // TaskUser <-> Issue
            modelBuilder.Entity<Issue>()
                .HasOne(i => i.TaskProject)
                .WithMany(tp => tp.Issues)
                .HasForeignKey(i => i.TaskProjectID)
                .OnDelete(DeleteBehavior.Restrict);

            //User <-> UserReport
            modelBuilder.Entity<UserReports>()
                .HasOne(ur => ur.ReportedUser)
                .WithMany(u => u.Reports)
                .HasForeignKey(ur => ur.UserReportId)
                .OnDelete(DeleteBehavior.Restrict);

            //User <-> UserAppeal
            modelBuilder.Entity<UserAppeals>()
                .HasOne(ua => ua.User)
                .WithMany(u => u.Appeals)
                .HasForeignKey(ua => ua.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            //User <-> UserBan
            modelBuilder.Entity<UserBans>()
                .HasOne(ub => ub.User)
                .WithMany(u => u.Bans)
                .HasForeignKey(ub => ub.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            //Task <-> TaskLabels
            modelBuilder.Entity<TaskLabels>()
                .HasOne(tl => tl.Task)
                .WithMany(t => t.TaskLabels)
                .HasForeignKey(tl => tl.TaskId)
                .OnDelete(DeleteBehavior.Restrict);
            // Labels <-> TaskLabels
            modelBuilder.Entity<TaskLabels>()
                .HasOne(tl => tl.Label)
                .WithMany(l => l.TaskLabels)
                .HasForeignKey(tl => tl.LabelId)
                .OnDelete(DeleteBehavior.Restrict);

            //Labels <-> Project
            modelBuilder.Entity<Labels>()
                .HasOne(l => l.Project)
                .WithMany(p => p.Labels)
                .HasForeignKey(l => l.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            //Task <-> TaskComment
            modelBuilder.Entity<TaskComment>()
                .HasOne(tc => tc.Task)
                .WithMany(t => t.TaskComments)
                .HasForeignKey(tc => tc.TaskId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

}
