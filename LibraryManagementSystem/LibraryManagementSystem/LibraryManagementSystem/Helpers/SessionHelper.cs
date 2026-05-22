namespace LibraryManagementSystem.Helpers
{
    public static class Session
    {
        public static int    UserId   { get; set; }
        public static string FullName { get; set; } = "";
        public static string Username { get; set; } = "";
        public static string Role     { get; set; } = "";
    }
}