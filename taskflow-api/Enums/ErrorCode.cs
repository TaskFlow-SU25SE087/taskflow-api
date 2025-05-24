using taskflow_api.Exceptions;

namespace taskflow_api.Enums
{
    public static class ErrorCode
    {
        // General error codes uncategorized
        public static readonly ErrorDetail UncategorizedException = new(9999, "Uncategorized error", StatusCodes.Status500InternalServerError);

        // User related error codes(1)
        public static readonly ErrorDetail InvalidEmail = new(1001, "Invalid email address", StatusCodes.Status400BadRequest);
        public static readonly ErrorDetail EmailExists = new(1002, "Email already exists", StatusCodes.Status400BadRequest);
        public static readonly ErrorDetail AccountBanned = new(1003, "Account is banned", StatusCodes.Status403Forbidden);
        public static readonly ErrorDetail InvalidPassword = new(1004, "Invalid password", StatusCodes.Status400BadRequest);

        //Image related error codes(2)
        public static readonly ErrorDetail ImageExists = new(2001, "Image Exists", StatusCodes.Status404NotFound);

        // Authorization related error codes(9)
        public static readonly ErrorDetail NoPermission = new(9001, "You do not have permission to perform this action", StatusCodes.Status403Forbidden);

    }
}
