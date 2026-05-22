using LibraryManagementSystem.Helpers;
using MySql.Data.MySqlClient;

namespace LibraryManagementSystem.Forms
{
    /// <summary>Transaction 3 – Manage Books (Inventory CRUD)</summary>
    public class BooksForm : Form
    {
        readonly TextBox       txtTitle  = UI.Txt(145, 55, 270);
        readonly TextBox       txtAuthor = UI.Txt(145, 91, 270);
        readonly TextBox       txtIsbn   = UI.Txt(145, 127, 180);
        readonly NumericUpDown nudCopies = new() { Location = new(145, 163), Size = new(80, 26), Font = UI.Body, Minimum = 1, Maximum = 9999, Value = 1 };
        readonly Label         lblMsg    = UI.Msg(20, 205);
        readonly DataGridView  grid      = UI.Grid();
        int selId = -1;

        public BooksForm()
        {
            Controls.Add(UI.Title("📋  Manage Books"));
            Controls.Add(UI.Lbl("Title:",  20, 58));  Controls.Add(txtTitle);
            Controls.Add(UI.Lbl("Author:", 20, 94));  Controls.Add(txtAuthor);
            Controls.Add(UI.Lbl("ISBN:",   20, 130)); Controls.Add(txtIsbn);
            Controls.Add(UI.Lbl("Copies:", 20, 166)); Controls.Add(nudCopies);
            Controls.Add(lblMsg);

            var bAdd = UI.Btn("Add",    20,  240, 80, UI.Green);
            var bUpd = UI.Btn("Update", 108, 240, 80, UI.Navy);
            var bDel = UI.Btn("Delete", 196, 240, 80, UI.Red);
            var bClr = UI.Btn("Clear",  284, 240, 80, Color.Gray);
            bAdd.Click += (_, _) => Save();
            bUpd.Click += (_, _) => Update();
            bDel.Click += (_, _) => Delete();
            bClr.Click += (_, _) => Clear();
            Controls.AddRange(new Control[] { bAdd, bUpd, bDel, bClr });
            Controls.Add(UI.Line(288));

            grid.Location  = new(20, 298);
            grid.Anchor    = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            grid.CellClick += GridClick;
            Controls.Add(grid);
            Resize += (_, _) => grid.Size = new(Width - 55, Height - 313);

            LoadGrid();
        }

        void LoadGrid()
        {
            try
            {
                using var conn = DB.Open();
                using var da = new MySqlDataAdapter(
                    "SELECT book_id AS ID, title AS Title, author AS Author, isbn AS ISBN, " +
                    "total_copies AS Total, available_copies AS Available, (total_copies-available_copies) AS Borrowed " +
                    "FROM books ORDER BY title", conn);
                var dt = new System.Data.DataTable(); da.Fill(dt);
                grid.DataSource = dt; UI.StyleGrid(grid);
                if (grid.Columns.Contains("ID")) grid.Columns["ID"].Visible = false;
            }
            catch (Exception ex) { Msg(ex.Message, true); }
        }

        void GridClick(object? s, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var r = grid.Rows[e.RowIndex];
            selId = Convert.ToInt32(r.Cells["ID"].Value);
            txtTitle.Text  = r.Cells["Title"].Value?.ToString()  ?? "";
            txtAuthor.Text = r.Cells["Author"].Value?.ToString() ?? "";
            txtIsbn.Text   = r.Cells["ISBN"].Value?.ToString()   ?? "";
            nudCopies.Value = Convert.ToDecimal(r.Cells["Total"].Value ?? 1);
        }

        void Save()
        {
            if (!Valid()) return;
            try
            {
                using var conn = DB.Open();
                using var cmd  = new MySqlCommand("INSERT INTO books(title,author,isbn,total_copies,available_copies) VALUES(@t,@a,@i,@c,@c)", conn);
                AddParams(cmd); cmd.ExecuteNonQuery();
                Msg("✔  Book added.", false); Clear(); LoadGrid();
            }
            catch (Exception ex) { Msg(ex.Message, true); }
        }

        void Update()
        {
            if (selId < 0) { Msg("Select a row first.", true); return; }
            if (!Valid()) return;
            try
            {
                using var conn = DB.Open();
                using var cmd  = new MySqlCommand("UPDATE books SET title=@t,author=@a,isbn=@i,total_copies=@c WHERE book_id=@id", conn);
                AddParams(cmd); cmd.Parameters.AddWithValue("@id", selId);
                cmd.ExecuteNonQuery();
                Msg("✔  Book updated.", false); Clear(); LoadGrid();
            }
            catch (Exception ex) { Msg(ex.Message, true); }
        }

        void Delete()
        {
            if (selId < 0) { Msg("Select a row first.", true); return; }
            if (MessageBox.Show("Delete this book?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            try
            {
                using var conn = DB.Open();
                using var cmd  = new MySqlCommand("DELETE FROM books WHERE book_id=@id", conn);
                cmd.Parameters.AddWithValue("@id", selId); cmd.ExecuteNonQuery();
                Msg("✔  Book deleted.", false); Clear(); LoadGrid();
            }
            catch (Exception ex) { Msg(ex.Message, true); }
        }

        void AddParams(MySqlCommand cmd)
        {
            cmd.Parameters.AddWithValue("@t", txtTitle.Text.Trim());
            cmd.Parameters.AddWithValue("@a", txtAuthor.Text.Trim());
            cmd.Parameters.AddWithValue("@i", txtIsbn.Text.Trim());
            cmd.Parameters.AddWithValue("@c", (int)nudCopies.Value);
        }

        bool Valid() { if (string.IsNullOrWhiteSpace(txtTitle.Text)) { Msg("Title is required.", true); return false; } return true; }
        void Clear() { selId = -1; txtTitle.Text = txtAuthor.Text = txtIsbn.Text = ""; nudCopies.Value = 1; lblMsg.Text = ""; }
        void Msg(string m, bool err) { lblMsg.ForeColor = err ? Color.Crimson : UI.Green; lblMsg.Text = m; }
    }
}