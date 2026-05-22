using LibraryManagementSystem.Helpers;
using MySql.Data.MySqlClient;

namespace LibraryManagementSystem.Forms
{
    /// <summary>Transaction 3 – Manage Books (Add / Edit / Delete inventory)</summary>
    public class ManageBooksForm : Form
    {
        private DataGridView grid       = null!;
        private TextBox      txtTitle   = null!;
        private TextBox      txtAuthor  = null!;
        private TextBox      txtIsbn    = null!;
        private NumericUpDown nudTotal  = null!;
        private Button       btnAdd     = null!;
        private Button       btnUpdate  = null!;
        private Button       btnDelete  = null!;
        private Button       btnClear   = null!;
        private Label        lblStatus  = null!;
        private int          selectedId = -1;

        public ManageBooksForm() => InitializeComponent();

        private void InitializeComponent()
        {
            Text      = "Manage Books";
            BackColor = Color.FromArgb(245, 248, 252);

            var title = FormBuilder.SectionTitle("📋  Manage Books (Inventory)");

            // ── Input panel ──────────────────────────────────────────────────
            var panel = new Panel
            {
                Location  = new Point(20, 55),
                Size      = new Size(460, 220),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            int lx = 12, fx = 130, w = 290;
            panel.Controls.Add(FormBuilder.Label2("Title:",         lx, 15));
            txtTitle  = FormBuilder.TextBox2(fx, 12, w); panel.Controls.Add(txtTitle);

            panel.Controls.Add(FormBuilder.Label2("Author:",        lx, 55));
            txtAuthor = FormBuilder.TextBox2(fx, 52, w); panel.Controls.Add(txtAuthor);

            panel.Controls.Add(FormBuilder.Label2("ISBN:",          lx, 95));
            txtIsbn   = FormBuilder.TextBox2(fx, 92, w); panel.Controls.Add(txtIsbn);

            panel.Controls.Add(FormBuilder.Label2("Total Copies:", lx, 135));
            nudTotal  = new NumericUpDown
            {
                Location = new Point(fx, 132),
                Size     = new Size(100, 26),
                Minimum  = 1,
                Maximum  = 9999,
                Value    = 1,
                Font     = new Font("Segoe UI", 10)
            };
            panel.Controls.Add(nudTotal);

            // Buttons row
            btnAdd    = SmallBtn("➕ Add",    Color.FromArgb(26,  94,  26),  10, 175);
            btnUpdate = SmallBtn("✏ Update",  Color.FromArgb(26,  60,  94),  90, 175);
            btnDelete = SmallBtn("🗑 Delete",  Color.FromArgb(160,  30,  30), 185, 175);
            btnClear  = SmallBtn("✖ Clear",   Color.FromArgb(100, 100, 100), 270, 175);

            btnAdd.Click    += BtnAdd_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnDelete.Click += BtnDelete_Click;
            btnClear.Click  += (_, _) => ClearForm();

            panel.Controls.AddRange(new Control[] { btnAdd, btnUpdate, btnDelete, btnClear });

            lblStatus = FormBuilder.StatusLabel(20, 282, 500);

            // ── Grid ─────────────────────────────────────────────────────────
            grid = FormBuilder.MakeGrid();
            grid.Location      = new Point(20, 308);
            grid.Size          = new Size(Width - 60, Height - 360);
            grid.Anchor        = AnchorStyles.Top | AnchorStyles.Left |
                                 AnchorStyles.Right | AnchorStyles.Bottom;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect   = false;
            grid.CellClick    += Grid_CellClick;

            Controls.AddRange(new Control[] { title, panel, lblStatus, grid });
            Resize += (_, _) => grid.Size = new Size(Width - 60, Height - 360);

            ClearForm();
            LoadGrid();
        }

        private static Button SmallBtn(string text, Color back, int x, int y) =>
            new Button
            {
                Text      = text,
                Location  = new Point(x, y),
                Size      = new Size(75, 32),
                BackColor = back,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 8.5f),
                Cursor    = Cursors.Hand
            };

        private void LoadGrid()
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                const string sql =
                    "SELECT book_id AS `ID`, title AS `Title`, author AS `Author`, " +
                    "isbn AS `ISBN`, total_copies AS `Total`, available_copies AS `Available`, " +
                    "(total_copies - available_copies) AS `Borrowed` " +
                    "FROM books ORDER BY title";
                using var da = new MySqlDataAdapter(sql, conn);
                var dt = new System.Data.DataTable();
                da.Fill(dt);
                grid.DataSource = dt;
                FormBuilder.StyleGrid(grid);
                if (grid.Columns.Contains("ID"))
                    grid.Columns["ID"].Visible = false;
            }
            catch (Exception ex) { ShowError(ex.Message); }
        }

        private void Grid_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = grid.Rows[e.RowIndex];
            selectedId        = Convert.ToInt32(row.Cells["ID"].Value);
            txtTitle.Text     = row.Cells["Title"].Value?.ToString() ?? "";
            txtAuthor.Text    = row.Cells["Author"].Value?.ToString() ?? "";
            txtIsbn.Text      = row.Cells["ISBN"].Value?.ToString() ?? "";
            nudTotal.Value    = Convert.ToDecimal(row.Cells["Total"].Value ?? 1);
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            if (!ValidateInput()) return;
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                const string sql =
                    "INSERT INTO books (title, author, isbn, total_copies, available_copies) " +
                    "VALUES (@t,@a,@i,@tot,@tot)";
                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@t",   txtTitle.Text.Trim());
                cmd.Parameters.AddWithValue("@a",   txtAuthor.Text.Trim());
                cmd.Parameters.AddWithValue("@i",   txtIsbn.Text.Trim());
                cmd.Parameters.AddWithValue("@tot", (int)nudTotal.Value);
                cmd.ExecuteNonQuery();
                ShowSuccess("✔  Book added successfully.");
                ClearForm(); LoadGrid();
            }
            catch (Exception ex) { ShowError(ex.Message); }
        }

        private void BtnUpdate_Click(object? sender, EventArgs e)
        {
            if (selectedId < 0) { ShowError("Select a row first."); return; }
            if (!ValidateInput()) return;
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                const string sql =
                    "UPDATE books SET title=@t, author=@a, isbn=@i, total_copies=@tot " +
                    "WHERE book_id=@id";
                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@t",   txtTitle.Text.Trim());
                cmd.Parameters.AddWithValue("@a",   txtAuthor.Text.Trim());
                cmd.Parameters.AddWithValue("@i",   txtIsbn.Text.Trim());
                cmd.Parameters.AddWithValue("@tot", (int)nudTotal.Value);
                cmd.Parameters.AddWithValue("@id",  selectedId);
                cmd.ExecuteNonQuery();
                ShowSuccess("✔  Book updated.");
                ClearForm(); LoadGrid();
            }
            catch (Exception ex) { ShowError(ex.Message); }
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (selectedId < 0) { ShowError("Select a row first."); return; }
            if (MessageBox.Show("Delete this book?", "Confirm",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                using var cmd  = new MySqlCommand(
                    "DELETE FROM books WHERE book_id=@id", conn);
                cmd.Parameters.AddWithValue("@id", selectedId);
                cmd.ExecuteNonQuery();
                ShowSuccess("✔  Book deleted.");
                ClearForm(); LoadGrid();
            }
            catch (Exception ex) { ShowError(ex.Message); }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            { ShowError("Title is required."); return false; }
            return true;
        }

        private void ClearForm()
        {
            selectedId    = -1;
            txtTitle.Text = txtAuthor.Text = txtIsbn.Text = "";
            nudTotal.Value = 1;
            lblStatus.Text = string.Empty;
        }

        private void ShowError(string msg)   { lblStatus.ForeColor = Color.Crimson;              lblStatus.Text = "✖  " + msg; }
        private void ShowSuccess(string msg) { lblStatus.ForeColor = Color.FromArgb(0, 128, 0);  lblStatus.Text = msg; }
    }
}
