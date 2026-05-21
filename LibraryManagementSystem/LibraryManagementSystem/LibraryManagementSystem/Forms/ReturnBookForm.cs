using LibraryManagementSystem.Helpers;
using MySql.Data.MySqlClient;

namespace LibraryManagementSystem.Forms
{
    /// <summary>Transaction 2 – Return a Book (with fine computation via DB trigger)</summary>
    public class ReturnBookForm : Form
    {
        private DataGridView grid      = null!;
        private Button       btnReturn = null!;
        private Label        lblStatus = null!;

        public ReturnBookForm() => InitializeComponent();

        private void InitializeComponent()
        {
            Text      = "Return a Book";
            BackColor = Color.FromArgb(245, 248, 252);

            var title = FormBuilder.SectionTitle("↩️  Return a Book");

            var lblHint = FormBuilder.Label2(
                "Select a borrowing record below, then click Return.", 30, 65);
            lblHint.AutoSize = true;

            grid = FormBuilder.MakeGrid();
            grid.Location       = new Point(30, 95);
            grid.Size           = new Size(Width - 80, Height - 220);
            grid.Anchor         = AnchorStyles.Top | AnchorStyles.Left |
                                  AnchorStyles.Right | AnchorStyles.Bottom;
            grid.SelectionMode  = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect    = false;

            btnReturn = FormBuilder.PrimaryButton("↩  Mark as Returned", 30, 0);
            btnReturn.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnReturn.Click += BtnReturn_Click;

            lblStatus = FormBuilder.StatusLabel(30, 0, 550);
            lblStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

            Controls.AddRange(new Control[] { title, lblHint, grid, btnReturn, lblStatus });

            Resize += (_, _) =>
            {
                grid.Size        = new Size(Width - 80, Height - 220);
                btnReturn.Top    = Height - 115;
                lblStatus.Top    = Height - 80;
            };

            LoadGrid();
        }

        private void LoadGrid()
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                const string sql =
                    "SELECT b.borrowing_id AS `ID`, bk.title AS `Book Title`, " +
                    "u.full_name AS `Borrower`, " +
                    "b.borrow_date AS `Borrowed`, b.due_date AS `Due`, " +
                    "CASE WHEN CURDATE() > b.due_date " +
                    "     THEN CONCAT('⚠ ', DATEDIFF(CURDATE(), b.due_date),' days overdue') " +
                    "     ELSE 'On time' END AS `Status` " +
                    "FROM borrowings b " +
                    "JOIN books bk ON b.book_id = bk.book_id " +
                    "JOIN users u  ON b.user_id  = u.user_id " +
                    "WHERE b.return_date IS NULL ORDER BY b.due_date";
                using var da = new MySqlDataAdapter(sql, conn);
                var dt = new System.Data.DataTable();
                da.Fill(dt);
                grid.DataSource = dt;
                FormBuilder.StyleGrid(grid);
            }
            catch (Exception ex) { lblStatus.Text = "Error: " + ex.Message; }
        }

        private void BtnReturn_Click(object? sender, EventArgs e)
        {
            if (grid.SelectedRows.Count == 0)
            {
                lblStatus.ForeColor = Color.Crimson;
                lblStatus.Text = "Please select a row.";
                return;
            }

            int borrowingId = Convert.ToInt32(grid.SelectedRows[0].Cells["ID"].Value);

            try
            {
                using var conn = DatabaseHelper.GetConnection();
                const string sql =
                    "UPDATE borrowings SET return_date = CURDATE() WHERE borrowing_id = @id";
                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", borrowingId);
                cmd.ExecuteNonQuery();

                // read back fine (computed by trigger)
                using var cmd2 = new MySqlCommand(
                    "SELECT fine_amount FROM borrowings WHERE borrowing_id=@id", conn);
                cmd2.Parameters.AddWithValue("@id", borrowingId);
                var fine = Convert.ToDecimal(cmd2.ExecuteScalar() ?? 0);

                lblStatus.ForeColor = Color.FromArgb(0, 128, 0);
                lblStatus.Text = fine > 0
                    ? $"✔  Returned. Fine applied: ₱{fine:N2}"
                    : "✔  Book returned successfully. No fine.";

                LoadGrid();
            }
            catch (Exception ex)
            {
                lblStatus.ForeColor = Color.Crimson;
                lblStatus.Text = "Error: " + ex.Message;
            }
        }
    }
}
