using Microsoft.AspNetCore.Http.HttpResults;
using taskflow_api.TaskFlow.Shared.Exceptions;

namespace taskflow_api.TaskFlow.Domain.Common.Enums
{
    public static class ErrorCode
    {
        public static readonly ErrorDetail UncategorizedException = new(9999, "Uncategorized error", StatusCodes.Status500InternalServerError);

        public static readonly ErrorDetail InvalidEmail = new(1001, "Invalid email address", StatusCodes.Status400BadRequest);
        public static readonly ErrorDetail EmailExists = new(1002, "Email already exists", StatusCodes.Status400BadRequest);
        public static readonly ErrorDetail AccountBanned = new(1003, "Account is banned", StatusCodes.Status403Forbidden);
        public static readonly ErrorDetail InvalidPasswordOrUserName = new(1004, "Invalid password or username", StatusCodes.Status401Unauthorized);
        public static readonly ErrorDetail NoUserFound = new(1005, "No user found", StatusCodes.Status404NotFound);
        public static readonly ErrorDetail CannotBanAdmin = new(1006, "Cannot ban admin user", StatusCodes.Status403Forbidden);
        public static readonly ErrorDetail UserAlreadyBanned = new(1007, "User is already banned", StatusCodes.Status400BadRequest);
        public static readonly ErrorDetail UserNotBanned = new(1008, "User not banned", StatusCodes.Status400BadRequest);
        public static readonly ErrorDetail UsernameExists = new(1009, "Username already exists", StatusCodes.Status400BadRequest);
        public static readonly ErrorDetail EmailAlreadyVerified = new(1010, "Email already verified", StatusCodes.Status400BadRequest);
        public static readonly ErrorDetail UsernameAlreadyExists = new(1011, "Username already exists", StatusCodes.Status400BadRequest);
        public static readonly ErrorDetail UsernameAlreadyAdded = new(1012, "Username has already been added for this account", StatusCodes.Status400BadRequest);
        public static readonly ErrorDetail EmailNotConfirmed = new(1013, "Email not confirmed", StatusCodes.Status400BadRequest);
        public static readonly ErrorDetail CannotResetPassword = new(1014, "cannot reset password", StatusCodes.Status400BadRequest);
        public static readonly ErrorDetail InvalidEmailOrUsername = new(1015, "Invalid email or username", StatusCodes.Status400BadRequest);
        public static readonly ErrorDetail AccountExpired = new(1016, "Account has expired", StatusCodes.Status403Forbidden);
        public static readonly ErrorDetail TermExpired = new(1017, "Term has expired", StatusCodes.Status403Forbidden);

        public static readonly ErrorDetail ImageExists = new(2001, "Image Exists", StatusCodes.Status404NotFound);
        public static readonly ErrorDetail ImageNotCanSave = new(2002, "Image cannot be saved", StatusCodes.Status400BadRequest);
        public static readonly ErrorDetail MaxFileLimitReached = new(2003, "You have reached the maximum file limit", StatusCodes.Status403Forbidden);

        public static readonly ErrorDetail CannotCreateProject = new(3001, "Cannot create project", StatusCodes.Status400BadRequest);
        public static readonly ErrorDetail CannotLeaveProjectAsPM = new(3002, "Cannot leave project as project manager", StatusCodes.Status400BadRequest);
        public static readonly ErrorDetail ProjectNotFound = new(3003, "Project not found", StatusCodes.Status404NotFound);
        public static readonly ErrorDetail MaxProjectLimitReached = new(3004, "You have reached the maximum number of projects allowed", StatusCodes.Status403Forbidden);
        public static readonly ErrorDetail CannotUpdateSprint = new(3005, "Cannot update sprint", StatusCodes.Status400BadRequest);
        public static readonly ErrorDetail UserNotInProject = new(3006, "User is not in the project", StatusCodes.Status403Forbidden);
        public static readonly ErrorDetail SprintNameAlreadyExists = new(3007, "Sprint name already exists", StatusCodes.Status400BadRequest);
        public static readonly ErrorDetail CannotCreateSprint = new(3008, "Cannot create sprint", StatusCodes.Status400BadRequest);
        public static readonly ErrorDetail TermHasProjectsOrUsers = new(3009, "Term has projects or users", StatusCodes.Status400BadRequest);
        public static readonly ErrorDetail PartNotFound = new(3010, "Repo not found", StatusCodes.Status404NotFound);
        public static readonly ErrorDetail SprintAlreadyInProgress = new(3011, "Sprint is already in progress", StatusCodes.Status400BadRequest);
        public static readonly ErrorDetail CannotStartSprint = new(3012, "Cannot start sprint", StatusCodes.Status400BadRequest);

        public static readonly ErrorDetail InvalidToken = new(4001, "Invalid token", StatusCodes.Status401Unauthorized);
        public static readonly ErrorDetail RefreshTokenExpired = new(4002, "Refresh token expired", StatusCodes.Status401Unauthorized);
        public static readonly ErrorDetail InvalidRefreshToken = new(4003, "Invalid refresh token", StatusCodes.Status401Unauthorized);
        public static readonly ErrorDetail TooManyAttempts = new(4004, "Too many attempts, please try again later", StatusCodes.Status429TooManyRequests);

        public static readonly ErrorDetail CannotCreateBoard = new(5001, "Cannot create board", StatusCodes.Status400BadRequest);
        public static readonly ErrorDetail BoardNotFound = new(5002, "Board not found", StatusCodes.Status404NotFound);
        public static readonly ErrorDetail CannotUpdateBoard = new(5003, "Cannot update board", StatusCodes.Status400BadRequest);
        public static readonly ErrorDetail CannotDeleteBoard = new(5004, "Cannot delete board", StatusCodes.Status400BadRequest);

        public static readonly ErrorDetail NoProjectsFound = new(6001, "No projects found", StatusCodes.Status404NotFound);
        public static readonly ErrorDetail TaskAlreadyAssigned = new(6002, "Task has already been assigned to this user", StatusCodes.Status409Conflict);
        public static readonly ErrorDetail UserNotAssignedToTask = new(6003, "You are not assigned to this task", StatusCodes.Status403Forbidden);
        public static readonly ErrorDetail TaskAlreadyInThisBoard = new(6004, "Task already exists in this board", StatusCodes.Status409Conflict);

        public static readonly ErrorDetail TagIsNull = new(7001, "Tag cannot be null", StatusCodes.Status400BadRequest);
        public static readonly ErrorDetail TagNotFound = new(7002, "Tag not found", StatusCodes.Status404NotFound);
        public static readonly ErrorDetail TagAlreadyExistsInTask = new(7003, "Tag Already Exists In Task", StatusCodes.Status404NotFound);
        public static readonly ErrorDetail CannotUpdateTag = new(7004, "Cannot update tag", StatusCodes.Status400BadRequest);
        public static readonly ErrorDetail TagNameAlreadyExists = new(7005, "Tag name already exists", StatusCodes.Status400BadRequest);


        public static readonly ErrorDetail TaskNotFound = new(8001, "Task not found", StatusCodes.Status404NotFound);
        public static readonly ErrorDetail CannotUpdateStatus = new(8002, "Cannot update task status", StatusCodes.Status400BadRequest);
        public static readonly ErrorDetail InvalidEffortPointsDistribution = new(8003, "Invalid effort points distribution. Total assigned points must equal task effort points", StatusCodes.Status400BadRequest);
        public static readonly ErrorDetail DuplicateAssignee = new(8004, "Duplicate assignee found in the request", StatusCodes.Status400BadRequest);


        public static readonly ErrorDetail NoPermission = new(9001, "You do not have permission to perform this action", StatusCodes.Status403Forbidden);
        public static readonly ErrorDetail Unauthorized = new(9002, "Unauthorized access", StatusCodes.Status401Unauthorized);
        public static readonly ErrorDetail NoFile = new(9003, "No file", StatusCodes.Status404NotFound);
        public static readonly ErrorDetail InvalidTermDates = new(9004, "Invalid term dates", StatusCodes.Status400BadRequest);
        public static readonly ErrorDetail TermNotFound = new(9005, "Term not found", StatusCodes.Status404NotFound);
        public static readonly ErrorDetail InvalidRepoOrToken = new(9006, "Invalid repository or token", StatusCodes.Status400BadRequest);
        public static readonly ErrorDetail WebhookCreationFailed = new(9007, "Failed to create webhook", StatusCodes.Status400BadRequest);
        public static readonly ErrorDetail UserGitHubTokenNotFound = new(9008, "User GitHub token not found", StatusCodes.Status404NotFound);
        public static readonly ErrorDetail GitHubTokenNotFound = new(9009, "GitHub token not found", StatusCodes.Status404NotFound);
        public static readonly ErrorDetail NoHaveRepoInProject = new(9010, "No repositories found in the project", StatusCodes.Status404NotFound);
        public static readonly ErrorDetail GitMemberNotFound = new(9011, "Git member not found", StatusCodes.Status404NotFound);
        public static readonly ErrorDetail SprintMeetingCannotUpdate = new(9012, "Sprint meeting cannot be updated", StatusCodes.Status400BadRequest);
        public static readonly ErrorDetail SprintMeetingTaskNotFound = new(9013, "Sprint meeting task not found", StatusCodes.Status404NotFound);
        public static readonly ErrorDetail SprintMeetingTaskItemVersionNotMatch = new(9014, "Sprint meeting task item version does not match", StatusCodes.Status400BadRequest);
        public static readonly ErrorDetail SprintMeetingNotFound = new(9015, "Sprint meeting not found", StatusCodes.Status404NotFound);
        public static readonly ErrorDetail SprintNotFound = new(9016, "Sprint not found", StatusCodes.Status404NotFound);
        public static readonly ErrorDetail SourceEmpty = new(9017, "Source is empty", StatusCodes.Status400BadRequest);
        public static readonly ErrorDetail WebhookDeletionFailed = new(9018, "Failed to delete webhook", StatusCodes.Status400BadRequest);
    }
}
