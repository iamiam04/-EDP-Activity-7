using LibraryManagementSystem.Helpers;
using MySql.Data.MySqlClient;

namespace LibraryManagementSystem.Forms
{
    /// <summary>Transaction 1 – Borrow a Book</summary>
    public class BorrowBookForm : Form
    {
        private ComboBox  cmbUser   = null!;
        private ComboBox  cmbBook   = null!;
        private DateTimePicker dtpDue = null!;
        private Button    btnSubmit  = null!;
        private Label     lblStatus  = null!;
        private DataGridView grid     = null!;

        public BorrowBookForm() => InitializeComponent();

        private void InitializeComponent()
        {
            Text      = "Borrow a Book";
            BackColor = Color.FromArgb(245, 248, 252);

            var title = FormBuilder.SectionTitle("➕  Borrow a Book");

            int lx = 30, fx = 180, w = 260;
            var lblU  = FormBuilder.Label2("Member:",   lx, 70);
            cmbUser   = FormBuilder.Combo(fx, 67, w);

            var lblB  = FormBuilder.Label2("Book:",     lx, 110);
            cmbBook   = FormBuilder.Combo(fx, 107, w);

            var lblD  = FormBuilder.Label2("Due Date:", lx, 150);
            dtpDue    = new DateTimePicker
            {
                Location = new Point(fx, 147),
                Size     = new Size(200, 26),
                MinDate  = DateTime.Today.AddDays(1),
                Value    = DateTime.Today.AddDays(14),
                Font     = new Font("Segoe UI", 10)
            };

            btnSubmit = FormBuilder.PrimaryButton("✔  Confirm Borrow", fx, 190);
            btnSubmit.Click += BtnSubmit_Click;

            lblStatus = FormBuilder.StatusLabel(lx, 240, 500);

            var sep = FormBuilder.Separator(260);

            var lblGrid = FormBuilder.Label2("Current Active Borrowings:", lx, 275);
            lblGrid.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            grid = FormBuilder.MakeGrid();
            grid.Location = new Point(lx, 298);
            grid.Size     = new Size(Width - 80, Height - 340);
            grid.Anchor   = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

            Controls.AddRange(new Control[]
            {
                title, lblU, cmbUser, lblB, cmbBook,
                lblD, dtpDue, btnSubmit, lblStatus,
                sep, lblGrid, grid
            });

            Resize += (_, _) => grid.Size = new Size(Width - 80, Height - 340);

            LoadCombos();
            LoadGrid();
        }

        private void LoadCombos()
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();

                // members only
                using var cmd1 = new MySqlCommand(
                    "SELECT user_id, full_name FROM users WHERE status='Active' AND role='Member' ORDER BY full_name", conn);
                cmbUser.DataSource    = null;
                cmbUser.Items.Clear();
                using var r1 = cmd1.ExecuteReader();
                var users = new List<(int id, string name)>();
                while (r1.Read()) users.Add((r1.GetInt32(0), r1.GetString(1)));
                r1.Close();
                cmbUser.DisplayMember = "Display";
                cmbUser.ValueMember   = "Id";
                cmbUser.DataSource    = users.Select(u => new { Id = u.id, Display = u.name }).ToList();

                // available books
                using var cmd2 = new MySqlCommand(
                    "SELECT book_id, CONCAT(title,' [',available_copies,' avail]') FROM books " +
                    "WHERE available_copies > 0 ORDER BY title", conn);
                cmbBook.DataSource    = null;
                cmbBook.Items.Clear();
                using var r2 = cmd2.ExecuteReader();
                var books = new List<(int id, string label)>();
                while (r2.Read()) books.Add((r2.GetInt32(0), r2.GetString(1)));
                r2.Close();
                cmbBook.DisplayMember = "Display";
                cmbBook.ValueMember   = "Id";
                cmbBook.DataSource    = books.Select(b => new { Id = b.id, Display = b.label }).ToList();
            }
            catch (Exception ex) { lblStatus.Text = "Error: " + ex.Message; }
        }

        private void LoadGrid()
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                const string sql =
                    "SELECT b.borrowing_id, bk.title, u.full_name AS borrower, " +
                    "b.borrow_date, b.due_date, b.fine_amount " +
                    "FROM borrowings b " +
                    "JOIN books bk ON b.book_id = bk.book_id " +
                    "JOIN users u  ON b.user_id  = u.user_id " +
                    "WHERE b.return_date IS NULL ORDER BY b.due_date";
                using var da = new MySql.Data.MySqlClient.MySqlDataAdapter(sql, conn);
                var dt = new System.Data.DataTable();
                da.Fill(dt);
                grid.DataSource = dt;
                FormBuilder.StyleGrid(grid);
            }
            catch (Exception ex) { lblStatus.Text = "Error: " + ex.Message; }
        }

        private void BtnSubmit_Click(object? sender, EventArgs e)
        {
            if (cmbUser.SelectedValue == null || cmbBook.SelectedValue == null)
            {
                lblStatus.ForeColor = Color.Crimson;
                lblStatus.Text = "Please select a member and a book.";
                return;
            }

            try
            {
                using var conn = DatabaseHelper.GetConnection();
                const string sql =
                    "INSERT INTO borrowings (book_id, user_id, borrow_date, due_date) " +
                    "VALUES (@bookId, @userId, CURDATE(), @due)";
                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@bookId", cmbBook.SelectedValue);
                cmd.Parameters.AddWithValue("@userId", cmbUser.SelectedValue);
                cmd.Parameters.AddWithValue("@due",    dtpDue.Value.Date);
                cmd.ExecuteNonQuery();

                lblStatus.ForeColor = Color.FromArgb(0, 128, 0);
                lblStatus.Text = "✔  Book borrowed successfully!";
                LoadCombos();
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
