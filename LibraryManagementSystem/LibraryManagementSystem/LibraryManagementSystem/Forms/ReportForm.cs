using LibraryManagementSystem.Helpers;
using MySql.Data.MySqlClient;
using System.Data;

namespace LibraryManagementSystem.Forms
{
    public class ReportForm : Form
    {
        readonly string       _mode;
        readonly DataGridView grid   = UI.Grid();
        readonly Label        lblMsg = UI.Msg(310, 63);
        DataTable _data = new();

        static readonly Dictionary<string, string> Titles = new()
        {
            ["borrowings"]   = "📊  Borrowings Report",
            ["inventory"]    = "📦  Books Inventory Report",
            ["useractivity"] = "👤  User Activity Report"
        };

        public ReportForm(string mode)
        {
            _mode = mode;
            Controls.Add(UI.Title(Titles[mode]));

            var btnRefresh = UI.Btn("🔄  Refresh",        20,  55, 115);
            var btnExport  = UI.Btn("📥  Export to Excel", 145, 55, 165, UI.Green);
            btnRefresh.Click += (_, _) => LoadData();
            btnExport.Click  += Export;
            Controls.AddRange(new Control[] { btnRefresh, btnExport, lblMsg });

            grid.Location = new(20, 105);
            grid.Anchor   = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            Controls.Add(grid);
            Resize += (_, _) => grid.Size = new(Width - 55, Height - 120);

            LoadData();
        }

        void LoadData()
        {
            Msg("Loading…", false);
            try
            {
                using var conn = DB.Open();
                using var da   = new MySqlDataAdapter(Sql(), conn);
                _data = new DataTable(); da.Fill(_data);
                grid.DataSource = _data; UI.StyleGrid(grid);
                Msg($"✔  {_data.Rows.Count} record(s) loaded.", false);
            }
            catch (Exception ex) { Msg("Error: " + ex.Message, true); }
        }

        string Sql() => _mode switch
        {
            "borrowings" =>
                "SELECT b.borrowing_id,bk.title,u.full_name AS borrower," +
                "b.borrow_date,b.due_date,b.return_date,b.fine_amount " +
                "FROM borrowings b JOIN books bk ON b.book_id=bk.book_id " +
                "JOIN users u ON b.user_id=u.user_id ORDER BY b.borrow_date DESC",
            "inventory" =>
                "SELECT book_id,title,author,isbn,total_copies,available_copies FROM books ORDER BY title",
            "useractivity" =>
                "SELECT u.user_id,u.full_name,u.username,u.role,u.status," +
                "COUNT(b.borrowing_id) AS total_borrowings," +
                "SUM(b.return_date IS NULL) AS active_loans," +
                "IFNULL(SUM(b.fine_amount),0) AS total_fines " +
                "FROM users u LEFT JOIN borrowings b ON u.user_id=b.user_id " +
                "GROUP BY u.user_id ORDER BY u.full_name",
            _ => "SELECT 1"
        };

        void Export(object? s, EventArgs e)
        {
            if (_data.Rows.Count == 0) { Msg("No data – click Refresh first.", true); return; }
            using var dlg = new SaveFileDialog
            {
                Title    = "Save Excel Report",
                Filter   = "Excel Workbook (*.xlsx)|*.xlsx",
                FileName = $"{_mode}_{DateTime.Now:yyyyMMdd_HHmm}.xlsx",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;
            try
            {
                Msg("Generating Excel file…", false);
                Application.DoEvents();
                ExcelExporter.Export(_mode, _data, dlg.FileName);
                Msg($"✔  Saved: {Path.GetFileName(dlg.FileName)}", false);
                if (MessageBox.Show("Report saved! Open it now?", "Done", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = dlg.FileName, UseShellExecute = true });
            }
            catch (Exception ex) { Msg("Export error: " + ex.Message, true); }
        }

        void Msg(string m, bool err) { lblMsg.ForeColor = err ? Color.Crimson : UI.Green; lblMsg.Text = m; }
    }
}