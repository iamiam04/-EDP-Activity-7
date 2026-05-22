using LibraryManagementSystem.Helpers;
using MySql.Data.MySqlClient;

namespace LibraryManagementSystem.Forms
{
    public class BorrowForm : Form
    {
        readonly ComboBox      cmbMember = UI.Cmb(145, 60);
        readonly ComboBox      cmbBook   = UI.Cmb(145, 96);
        readonly DateTimePicker dtpDue   = new() { Location = new(145, 132), Size = new(190, 26), Font = UI.Body };
        readonly Label         lblMsg    = UI.Msg(20, 182);
        readonly DataGridView  grid      = UI.Grid();

        public BorrowForm()
        {
            Controls.Add(UI.Title("➕  Borrow a Book"));
            Controls.Add(UI.Lbl("Member:",   20, 63)); Controls.Add(cmbMember);
            Controls.Add(UI.Lbl("Book:",     20, 99)); Controls.Add(cmbBook);
            Controls.Add(UI.Lbl("Due Date:", 20, 135)); Controls.Add(dtpDue);
            Controls.Add(lblMsg);

            var btn = UI.Btn("✔  Confirm Borrow", 145, 214, 170);
            btn.Click += Borrow;
            Controls.Add(btn);
            Controls.Add(UI.Line(262));
            Controls.Add(new Label { Text = "Active Borrowings", Location = new(20, 272), AutoSize = true, Font = UI.H2, ForeColor = UI.Navy });

            grid.Location = new(20, 300);
            grid.Anchor   = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            Controls.Add(grid);
            Resize += (_, _) => grid.Size = new(Width - 55, Height - 315);

            dtpDue.MinDate = DateTime.Today.AddDays(1);
            dtpDue.Value   = DateTime.Today.AddDays(14);

            LoadCombos(); LoadGrid();
        }

        void LoadCombos()
        {
            try
            {
                using var conn = DB.Open();
                cmbMember.DataSource = null; cmbMember.DisplayMember = "Name"; cmbMember.ValueMember = "Id";
                using var c1 = new MySqlCommand("SELECT user_id,full_name FROM users WHERE role='Member' AND status='Active' ORDER BY full_name", conn);
                var r1 = c1.ExecuteReader();
                var ul = new List<dynamic>(); while (r1.Read()) ul.Add(new { Id = r1.GetInt32(0), Name = r1.GetString(1) }); r1.Close();
                cmbMember.DataSource = ul;

                cmbBook.DataSource = null; cmbBook.DisplayMember = "Name"; cmbBook.ValueMember = "Id";
                using var c2 = new MySqlCommand("SELECT book_id,CONCAT(title,' [',available_copies,' avail]') FROM books WHERE available_copies>0 ORDER BY title", conn);
                var r2 = c2.ExecuteReader();
                var bl = new List<dynamic>(); while (r2.Read()) bl.Add(new { Id = r2.GetInt32(0), Name = r2.GetString(1) }); r2.Close();
                cmbBook.DataSource = bl;
            }
            catch (Exception ex) { Msg(ex.Message, true); }
        }

        void LoadGrid()
        {
            try
            {
                using var conn = DB.Open();
                using var da = new MySqlDataAdapter(
                    "SELECT b.borrowing_id AS ID, bk.title AS Book, u.full_name AS Member, " +
                    "b.borrow_date AS Borrowed, b.due_date AS Due " +
                    "FROM borrowings b JOIN books bk ON b.book_id=bk.book_id JOIN users u ON b.user_id=u.user_id " +
                    "WHERE b.return_date IS NULL ORDER BY b.due_date", conn);
                var dt = new System.Data.DataTable(); da.Fill(dt);
                grid.DataSource = dt; UI.StyleGrid(grid);
            }
            catch (Exception ex) { Msg(ex.Message, true); }
        }

        void Borrow(object? s, EventArgs e)
        {
            if (cmbMember.SelectedValue == null || cmbBook.SelectedValue == null)
            { Msg("Please select a member and a book.", true); return; }
            try
            {
                using var conn = DB.Open();
                using var cmd  = new MySqlCommand("INSERT INTO borrowings(book_id,user_id,borrow_date,due_date) VALUES(@b,@u,CURDATE(),@d)", conn);
                cmd.Parameters.AddWithValue("@b", cmbBook.SelectedValue);
                cmd.Parameters.AddWithValue("@u", cmbMember.SelectedValue);
                cmd.Parameters.AddWithValue("@d", dtpDue.Value.Date);
                cmd.ExecuteNonQuery();
                Msg("✔  Book borrowed successfully.", false);
                LoadCombos(); LoadGrid();
            }
            catch (Exception ex) { Msg(ex.Message, true); }
        }

        void Msg(string m, bool err) { lblMsg.ForeColor = err ? Color.Crimson : UI.Green; lblMsg.Text = m; }
    }
}