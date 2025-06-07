using Microsoft.AspNetCore.Http.HttpResults;
using taskflow_api.TaskFlow.Shared.Exceptions;

namespace taskflow_api.TaskFlow.Domain.Common.Enums
{
    public static class ErrorCode
    {
        // General error codes uncategorized
        public static readonly ErrorDetail UncategorizedException = new(9999, "Uncategorized error", StatusCodes.Status500InternalServerError);

        // User related error codes(1)
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

        //Image related error codes(2)
        public static readonly ErrorDetail ImageExists = new(2001, "Image Exists", StatusCodes.Status404NotFound);
        public static readonly ErrorDetail ImageNotCanSave = new(2002, "Image cannot be saved", StatusCodes.Status400BadRequest);

        // Project related error codes(3)
        public static readonly ErrorDetail CannotCreateProject = new(3001, "Cannot create project", StatusCodes.Status400BadRequest);
        public static readonly ErrorDetail CannotLeaveProjectAsPM = new(3002, "Cannot leave project as project manager", StatusCodes.Status400BadRequest);
        public static readonly ErrorDetail ProjectNotFound = new(3003, "Project not found", StatusCodes.Status404NotFound);

        //Token related error codes(4)
        public static readonly ErrorDetail InvalidToken = new(4001, "Invalid token", StatusCodes.Status401Unauthorized);
        public static readonly ErrorDetail RefreshTokenExpired = new(4002, "Refresh token expired", StatusCodes.Status401Unauthorized);
        public static readonly ErrorDetail InvalidRefreshToken = new(4003, "Invalid refresh token", StatusCodes.Status401Unauthorized);
        public static readonly ErrorDetail TooManyAttempts = new(4004, "Too many attempts, please try again later", StatusCodes.Status429TooManyRequests);

        // Board related error codes(5)
        public static readonly ErrorDetail CannotCreateBoard = new(5001, "Cannot create board", StatusCodes.Status400BadRequest);
        public static readonly ErrorDetail BoardNotFound = new(5002, "Board not found", StatusCodes.Status404NotFound);
        public static readonly ErrorDetail CannotUpdateBoard = new(5003, "Cannot update board", StatusCodes.Status400BadRequest);
        public static readonly ErrorDetail CannotDeleteBoard = new(5004, "Cannot delete board", StatusCodes.Status400BadRequest);

        // Project related error codes(6)
        public static readonly ErrorDetail NoProjectsFound = new(6001, "No projects found", StatusCodes.Status404NotFound);

        // Authorization related error codes(9)
        public static readonly ErrorDetail NoPermission = new(9001, "You do not have permission to perform this action", StatusCodes.Status403Forbidden);
        public static readonly ErrorDetail Unauthorized = new(9002, "Unauthorized access", StatusCodes.Status401Unauthorized);

    }
}
