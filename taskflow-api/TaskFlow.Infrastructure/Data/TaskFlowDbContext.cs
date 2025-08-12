using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Infrastructure.Data
{
    public class TaskFlowDbContext : IdentityUserContext<User, Guid>
    {
        public TaskFlowDbContext(DbContextOptions<TaskFlowDbContext> options) : base(options)
        {
        }
        //Define DbSet for each entity
        public DbSet<Project> Projects { get; set; } = null!;
        public DbSet<ProjectMember> ProjectMembers { get; set; } = null!;
        public DbSet<Board> Boards { get; set; } = null!;
        public DbSet<Sprint> Sprints { get; set; } = null!;
        public DbSet<TaskProject> TaskProjects { get; set; } = null!;
        public DbSet<TaskAssignee> TaskAssignees { get; set; } = null!;
        public DbSet<Issue> Issues { get; set; } = null!;
        public DbSet<LogProject> LogProjects { get; set; } = null!;
        public DbSet<RefeshToken> RefeshTokens { get; set; } = null!;
        public DbSet<VerifyToken> VerifyTokens { get; set; } = null!;
        public DbSet<UserBans> UserBans { get; set; } = null!;
        public DbSet<UserReports> UserReports { get; set; } = null!;
        public DbSet<UserAppeals> UserAppeals { get; set; } = null!;
        public DbSet<Tag> Tags { get; set; } = null!;
        public DbSet<TaskTag> TaskTags { get; set; } = null!;
        public DbSet<TaskComment> TaskComments { get; set; } = null!;
        public DbSet<Term> Terms { get; set; } = null!;
        public DbSet<ProjectPart> ProjectParts { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;
        public DbSet<CommitRecord> CommitRecords { get; set; } = null!;
        public DbSet<UserGitHubToken> UserGitHubTokens { get; set; } = null!;
        public DbSet<CommitScanIssue> CommitScanIssues { get; set; } = null!;
        public DbSet<GitMember> GitMembers { get; set; } = null!;
        public DbSet<SprintMeetingLog> SprintMeetingLogs { get; set; } = null!;

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

            ////TaskProject <-> TaskAssignee
            //modelBuilder.Entity<TaskAssignee>()
            //   .HasOne(tu => tu.Task)
            //   .WithMany(tp => tp.TaskAssignees)
            //   .HasForeignKey(tu => tu.RefId)
            //   .OnDelete(DeleteBehavior.Restrict);

            //ProjectMember <-> TaskAssignee
            modelBuilder.Entity<TaskAssignee>()
               .HasOne(tu => tu.ProjectMember)
               .WithMany(pm => pm.taskUsers)
               .HasForeignKey(tu => tu.ImplementerId)
               .OnDelete(DeleteBehavior.Restrict);

            // TaskUser <-> Issue
            modelBuilder.Entity<Issue>()
                .HasOne(i => i.TaskProject)
                .WithMany(tp => tp.Issues)
                .HasForeignKey(i => i.TaskProjectId)
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

            //Task <-> TaskTag
            modelBuilder.Entity<TaskTag>()
                .HasOne(tl => tl.Task)
                .WithMany(t => t.TaskTags)
                .HasForeignKey(tl => tl.TaskId)
                .OnDelete(DeleteBehavior.Restrict);
            // Tag <-> TaskTag
            modelBuilder.Entity<TaskTag>()
                .HasOne(tl => tl.Tag)
                .WithMany(l => l.TaskTags)
                .HasForeignKey(tl => tl.TagId)
                .OnDelete(DeleteBehavior.Restrict);

            // Task <-> TaskTag
            modelBuilder.Entity<TaskTag>()
                .HasKey(t => new { t.TaskId, t.TagId });

            //Tag <-> Project
            modelBuilder.Entity<Tag>()
                .HasOne(l => l.Project)
                .WithMany(p => p.Tags)
                .HasForeignKey(l => l.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            //Task <-> TaskComment
            modelBuilder.Entity<TaskComment>()
                .HasOne(tc => tc.Task)
                .WithMany(t => t.TaskComments)
                .HasForeignKey(tc => tc.TaskId)
                .OnDelete(DeleteBehavior.Restrict);

            // ProjectMember <-> TaskComment
            modelBuilder.Entity<TaskComment>()
                .HasOne(tc => tc.UserComment)
                .WithMany(pm => pm.TaskComments)
                .HasForeignKey(tc => tc.CommenterId)
                .OnDelete(DeleteBehavior.Restrict);

            // ProjectMember <-> Project
            modelBuilder.Entity<ProjectMember>()
                .HasOne(pm => pm.Project)
                .WithMany(p => p.Members)
                .HasForeignKey(pm => pm.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            // Term <-> User
            modelBuilder.Entity<User>()
                .HasOne(u => u.Term)
                .WithMany(t => t.Users)
                .HasForeignKey(u => u.TermId)
                .OnDelete(DeleteBehavior.Restrict);

            // Term <-> Project
            modelBuilder.Entity<Project>()
                .HasOne(p => p.Term)
                .WithMany(t => t.Projects)
                .HasForeignKey(p => p.TermId)
                .OnDelete(DeleteBehavior.Restrict);

            // Project <-> ProjectPart
            modelBuilder.Entity<ProjectPart>()
                .HasOne(pp => pp.Project)
                .WithMany(p => p.ProjectParts)
                .HasForeignKey(pp => pp.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            // ProjectPart <-> CommitRecord
            modelBuilder.Entity<CommitRecord>()
                .HasOne(cr => cr.ProjectPart)
                .WithMany(pp => pp.CommitRecords)
                .HasForeignKey(cr => cr.ProjectPartId)
                .OnDelete(DeleteBehavior.Restrict);

            // UserGitHubToken <-> ProjectPart
            modelBuilder.Entity<ProjectPart>()
                .HasOne(p => p.UserGitHubToken)
                .WithMany()
                .HasForeignKey(p => p.UserGitHubTokenId)
                .OnDelete(DeleteBehavior.SetNull);

            // CommitScanIssue <-> CommitRecord
            modelBuilder.Entity<CommitScanIssue>()
                .HasOne(x => x.CommitRecord)
                .WithMany(x => x.CommitScanIssues)
                .HasForeignKey(x => x.CommitRecordId)
                .OnDelete(DeleteBehavior.Cascade);

            // GitMember <-> ProjectPart
            modelBuilder.Entity<GitMember>()
                .HasOne(gm => gm.ProjectPart)
                .WithMany(pp => pp.GitMembers)
                .HasForeignKey(gm => gm.ProjectPartId)
                .OnDelete(DeleteBehavior.Restrict);

            // GitMember <-> ProjectMember
            modelBuilder.Entity<GitMember>()
                .HasOne(gm => gm.ProjectMember)
                .WithMany(pm => pm.GitMembers)
                .HasForeignKey(gm => gm.ProjectMemberId)
                .OnDelete(DeleteBehavior.Restrict);

            // projecMember <-> Issue
            modelBuilder.Entity<Issue>()
                .HasOne(i => i.CreatedByMember)
                .WithMany(pm => pm.Issues)
                .HasForeignKey(i => i.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

}
