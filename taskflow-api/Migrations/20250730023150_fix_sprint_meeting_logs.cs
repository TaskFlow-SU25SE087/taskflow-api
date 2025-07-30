using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace taskflow_api.Migrations
{
    /// <inheritdoc />
    public partial class fix_sprint_meeting_logs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID('dbo.SprintMeetingLogs','U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SprintMeetingLogs](
        [Id] uniqueidentifier NOT NULL PRIMARY KEY,
        [SprintId] uniqueidentifier NOT NULL,
        [CompletedTasksJson] nvarchar(max) NOT NULL,
        [UnfinishedTasksJson] nvarchar(max) NOT NULL,
        [NextPlan] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL
    );
    ALTER TABLE [dbo].[SprintMeetingLogs]
    ADD CONSTRAINT [FK_SprintMeetingLogs_Sprints_SprintId]
    FOREIGN KEY ([SprintId]) REFERENCES [dbo].[Sprints]([Id]) ON DELETE CASCADE;

    -- Index cho SprintId để tối ưu join
    CREATE INDEX [IX_SprintMeetingLogs_SprintId] ON [dbo].[SprintMeetingLogs]([SprintId]);
END
");

            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.stats WHERE object_id = OBJECT_ID('dbo.Issues') AND name = 'IX_Issues_CreatedBy')
    DROP STATISTICS [dbo].[Issues].[IX_Issues_CreatedBy];
IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.Issues') AND name = 'IX_Issues_CreatedBy')
    DROP INDEX [IX_Issues_CreatedBy] ON [dbo].[Issues];

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.Issues') AND name = 'IX_Issues_CreatedBy')
    CREATE INDEX [IX_Issues_CreatedBy] ON [dbo].[Issues]([CreatedBy]);
");

            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.Issues')
      AND name = 'CreatedBy'
      AND is_nullable = 0
)
    ALTER TABLE [dbo].[Issues] ALTER COLUMN [CreatedBy] uniqueidentifier NULL;
");

            migrationBuilder.Sql(@"
-- Map theo (ProjectId, UserId) -> ProjectMembers.Id
UPDATE i
SET i.CreatedBy = pm.Id
FROM dbo.Issues i
JOIN dbo.ProjectMembers pm
  ON pm.ProjectId = i.ProjectId
 AND pm.UserId    = i.CreatedBy;  -- nếu CreatedBy trước đây là UserId

-- Set NULL các dòng không map được để tránh fail khi add FK
UPDATE i
SET i.CreatedBy = NULL
FROM dbo.Issues i
LEFT JOIN dbo.ProjectMembers pm ON pm.Id = i.CreatedBy
WHERE i.CreatedBy IS NOT NULL AND pm.Id IS NULL;
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Issues_ProjectMembers_CreatedBy')
BEGIN
    ALTER TABLE [dbo].[Issues] WITH NOCHECK
    ADD CONSTRAINT [FK_Issues_ProjectMembers_CreatedBy]
    FOREIGN KEY ([CreatedBy]) REFERENCES [dbo].[ProjectMembers]([Id])
    ON DELETE SET NULL;
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Issues_ProjectMembers_CreatedBy')
    ALTER TABLE [dbo].[Issues] DROP CONSTRAINT [FK_Issues_ProjectMembers_CreatedBy];
");

            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.Issues') AND name = 'IX_Issues_CreatedBy')
    DROP INDEX [IX_Issues_CreatedBy] ON [dbo].[Issues];
");

            migrationBuilder.Sql(@"
IF OBJECT_ID('dbo.SprintMeetingLogs','U') IS NOT NULL
    DROP TABLE [dbo].[SprintMeetingLogs];
");
        }
    }
}
