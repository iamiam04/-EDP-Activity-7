using MySql.Data.MySqlClient;

namespace LibraryManagementSystem.Helpers
{
    public static class DatabaseHelper
    {
        // ── Change these to match your local MySQL setup ──────────────────────
        private const string Server   = "localhost";
        private const string Database = "library_db";
        private const string User     = "root";
        private const string Password  = "";          // your MySQL root password
        // ─────────────────────────────────────────────────────────────────────

        private static readonly string ConnectionString =
            $"Server={Server};Database={Database};Uid={User};Pwd={Password};";

        public static MySqlConnection GetConnection()
        {
            var conn = new MySqlConnection(ConnectionString);
            conn.Open();
            return conn;
        }

        /// <summary>Quick test – returns true when the DB is reachable.</summary>
        public static bool TestConnection()
        {
            try
            {
                using var conn = GetConnection();
                return conn.State == System.Data.ConnectionState.Open;
            }
            catch { return false; }
        }
    }
}
