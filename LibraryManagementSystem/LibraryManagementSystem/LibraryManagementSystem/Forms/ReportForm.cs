using LibraryManagementSystem.Helpers;
using MySql.Data.MySqlClient;
using System.Data;

namespace LibraryManagementSystem.Forms
{
    /// <summary>
    /// Generic report viewer.
    /// mode = "borrowings" | "inventory" | "useractivity"
    /// Shows data in a DataGridView and exports to a styled Excel report.
    /// </summary>
    public class ReportForm : Form
    {
        private readonly string   _mode;
        private DataGridView      grid       = null!;
        private Button            btnExport  = null!;
        private Button            btnRefresh = null!;
        private Label             lblStatus  = null!;
        private DataTable         _data      = new();

        public ReportForm(string mode)
        {
            _mode = mode;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text      = ReportTitle();
            BackColor = Color.FromArgb(245, 248, 252);

            var title = FormBuilder.SectionTitle("📊  " + ReportTitle());

            // Toolbar
            btnRefresh = new Button
            {
                Text      = "🔄  Refresh",
                Location  = new Point(20, 55),
                Size      = new Size(110, 34),
                BackColor = Color.FromArgb(26, 60, 94),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 9.5f),
                Cursor    = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (_, _) => LoadData();

            btnExport = new Button
            {
                Text      = "📥  Export to Excel",
                Location  = new Point(140, 55),
                Size      = new Size(160, 34),
                BackColor = Color.FromArgb(21, 128, 61),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
            btnExport.FlatAppearance.BorderSize = 0;
            btnExport.Click += BtnExport_Click;

            lblStatus = FormBuilder.StatusLabel(320, 63, 500);

            grid = FormBuilder.MakeGrid();
            grid.Location = new Point(20, 100);
            grid.Size     = new Size(Width - 55, Height - 150);
            grid.Anchor   = AnchorStyles.Top | AnchorStyles.Left |
                            AnchorStyles.Right | AnchorStyles.Bottom;

            Controls.AddRange(new Control[]
            {
                title, btnRefresh, btnExport, lblStatus, grid
            });

            Resize += (_, _) => grid.Size = new Size(Width - 55, Height - 150);
            LoadData();
        }

        // ── Data loading ─────────────────────────────────────────────────────

        private void LoadData()
        {
            lblStatus.Text = "Loading…";
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                using var da   = new MySqlDataAdapter(GetSql(), conn);
                _data = new DataTable();
                da.Fill(_data);
                grid.DataSource = _data;
                FormBuilder.StyleGrid(grid);
                lblStatus.ForeColor = Color.FromArgb(0, 128, 0);
                lblStatus.Text = $"✔  {_data.Rows.Count} record(s) loaded.";
            }
            catch (Exception ex)
            {
                lblStatus.ForeColor = Color.Crimson;
                lblStatus.Text = "Error: " + ex.Message;
            }
        }

        private string GetSql() => _mode switch
        {
            "borrowings" =>
                "SELECT b.borrowing_id, bk.title, u.full_name AS borrower, " +
                "b.borrow_date, b.due_date, b.return_date, b.fine_amount " +
                "FROM borrowings b " +
                "JOIN books bk ON b.book_id = bk.book_id " +
                "JOIN users u  ON b.user_id  = u.user_id " +
                "ORDER BY b.borrow_date DESC",

            "inventory" =>
                "SELECT book_id, title, author, isbn, " +
                "total_copies, available_copies " +
                "FROM books ORDER BY title",

            "useractivity" =>
                "SELECT u.user_id, u.full_name, u.username, u.role, u.status, " +
                "COUNT(b.borrowing_id)                        AS total_borrowings, " +
                "SUM(b.return_date IS NULL)                   AS active_loans, " +
                "IFNULL(SUM(b.fine_amount), 0)                AS total_fines " +
                "FROM users u " +
                "LEFT JOIN borrowings b ON u.user_id = b.user_id " +
                "GROUP BY u.user_id ORDER BY u.full_name",

            _ => "SELECT 1"
        };

        private string ReportTitle() => _mode switch
        {
            "borrowings"   => "Borrowings Report",
            "inventory"    => "Books Inventory Report",
            "useractivity" => "User Activity Report",
            _              => "Report"
        };

        // ── Export ───────────────────────────────────────────────────────────

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            if (_data.Rows.Count == 0)
            {
                lblStatus.ForeColor = Color.OrangeRed;
                lblStatus.Text = "No data to export. Click Refresh first.";
                return;
            }

            using var dlg = new SaveFileDialog
            {
                Title            = "Save Excel Report",
                Filter           = "Excel Workbook (*.xlsx)|*.xlsx",
                FileName         = $"{_mode}_report_{DateTime.Now:yyyyMMdd_HHmm}.xlsx",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };

            if (dlg.ShowDialog() != DialogResult.OK) return;

            try
            {
                lblStatus.ForeColor = Color.Gray;
                lblStatus.Text = "Generating Excel file…";
                Application.DoEvents();

                string path = _mode switch
                {
                    "borrowings"   => ExcelHelper.GenerateBorrowingsReport(_data, dlg.FileName),
                    "inventory"    => ExcelHelper.GenerateBooksReport(_data, dlg.FileName),
                    "useractivity" => ExcelHelper.GenerateUserActivityReport(_data, dlg.FileName),
                    _              => throw new InvalidOperationException("Unknown report mode")
                };

                lblStatus.ForeColor = Color.FromArgb(0, 128, 0);
                lblStatus.Text = $"✔  Saved: {System.IO.Path.GetFileName(path)}";

                if (MessageBox.Show("Report saved! Open the file now?", "Success",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName        = path,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                lblStatus.ForeColor = Color.Crimson;
                lblStatus.Text = "Export error: " + ex.Message;
            }
        }
    }
}
