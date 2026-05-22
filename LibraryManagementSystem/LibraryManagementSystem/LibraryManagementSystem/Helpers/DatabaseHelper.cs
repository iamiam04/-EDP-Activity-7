using MySql.Data.MySqlClient;

namespace LibraryManagementSystem.Helpers
{
    public static class DB
    {
        const string CS = "Server=localhost;Database=library_db;Uid=root;Pwd=ChristisKing7;";

        public static MySqlConnection Open()
        {
            var c = new MySqlConnection(CS);
            c.Open();
            return c;
        }
    }
}