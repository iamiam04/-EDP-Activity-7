namespace LibraryManagementSystem.Helpers
{
    /// <summary>Holds the authenticated user for the current session.</summary>
    public static class SessionHelper
    {
        public static int    UserId   { get; set; }
        public static string FullName { get; set; } = string.Empty;
        public static string Username { get; set; } = string.Empty;
        public static string Role     { get; set; } = string.Empty;

        public static void Clear()
        {
            UserId   = 0;
            FullName = string.Empty;
            Username = string.Empty;
            Role     = string.Empty;
        }
    }
}
