using LibraryManagementSystem.Helpers;
using MySql.Data.MySqlClient;

namespace LibraryManagementSystem.Forms
{
    /// <summary>Transaction 2 – Return a Book (fine auto-computed by DB trigger)</summary>
    public class ReturnForm : Form
    {
        readonly DataGridView grid   = UI.Grid();
        readonly Label        lblMsg = UI.Msg(20, 0);

        public ReturnForm()
        {
            Controls.Add(UI.Title("↩️  Return a Book"));
            Controls.Add(new Label { Text = "Select a row then click Return.", Location = new(20, 52), AutoSize = true, Font = UI.Body, ForeColor = Color.Gray });

            grid.Location = new(20, 80);
            grid.Anchor   = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            Controls.Add(grid);

            var btn = UI.Btn("↩  Mark as Returned", 20, 0, 180);
            btn.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btn.Click += Return;
            Controls.Add(btn);

            lblMsg.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            Controls.Add(lblMsg);

            Resize += (_, _) => { grid.Size = new(Width - 55, Height - 195); btn.Top = Height - 112; lblMsg.Top = Height - 68; lblMsg.Width = Width - 55; };
            LoadGrid();
        }

        void LoadGrid()
        {
            try
            {
                using var conn = DB.Open();
                using var da = new MySqlDataAdapter(
                    "SELECT b.borrowing_id AS ID, bk.title AS Book, u.full_name AS Member, " +
                    "b.borrow_date AS Borrowed, b.due_date AS Due, " +
                    "CASE WHEN CURDATE()>b.due_date THEN CONCAT('⚠ ',DATEDIFF(CURDATE(),b.due_date),' days overdue') ELSE '✔ On time' END AS Status " +
                    "FROM borrowings b JOIN books bk ON b.book_id=bk.book_id JOIN users u ON b.user_id=u.user_id " +
                    "WHERE b.return_date IS NULL ORDER BY b.due_date", conn);
                var dt = new System.Data.DataTable(); da.Fill(dt);
                grid.DataSource = dt; UI.StyleGrid(grid);
            }
            catch (Exception ex) { Msg(ex.Message, true); }
        }

        void Return(object? s, EventArgs e)
        {
            if (grid.SelectedRows.Count == 0) { Msg("Select a row first.", true); return; }
            int id = Convert.ToInt32(grid.SelectedRows[0].Cells["ID"].Value);
            try
            {
                using var conn = DB.Open();
                using var cmd  = new MySqlCommand("UPDATE borrowings SET return_date=CURDATE() WHERE borrowing_id=@id", conn);
                cmd.Parameters.AddWithValue("@id", id); cmd.ExecuteNonQuery();

                using var cmd2 = new MySqlCommand("SELECT fine_amount FROM borrowings WHERE borrowing_id=@id", conn);
                cmd2.Parameters.AddWithValue("@id", id);
                var fine = Convert.ToDecimal(cmd2.ExecuteScalar() ?? 0);
                Msg(fine > 0 ? $"✔  Returned. Fine applied: ₱{fine:N2}" : "✔  Returned successfully. No fine.", false);
                LoadGrid();
            }
            catch (Exception ex) { Msg(ex.Message, true); }
        }

        void Msg(string m, bool err) { lblMsg.ForeColor = err ? Color.Crimson : UI.Green; lblMsg.Text = m; }
    }
}