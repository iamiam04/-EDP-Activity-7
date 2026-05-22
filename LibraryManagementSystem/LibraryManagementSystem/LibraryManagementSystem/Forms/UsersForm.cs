using LibraryManagementSystem.Helpers;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using System.Text;

namespace LibraryManagementSystem.Forms
{
    /// <summary>
    /// User Management – integrates Activity 5 module.
    /// Admin can add / edit / deactivate users.
    /// </summary>
    public class ManageUsersForm : Form
    {
        private DataGridView grid      = null!;
        private TextBox      txtName   = null!;
        private TextBox      txtUser   = null!;
        private TextBox      txtEmail  = null!;
        private TextBox      txtPass   = null!;
        private ComboBox     cmbRole   = null!;
        private ComboBox     cmbStatus = null!;
        private Button       btnAdd    = null!;
        private Button       btnUpdate = null!;
        private Button       btnClear  = null!;
        private Label        lblStatus = null!;
        private int          selId     = -1;

        public ManageUsersForm() => InitializeComponent();

        private void InitializeComponent()
        {
            Text      = "Manage Users";
            BackColor = Color.FromArgb(245, 248, 252);

            var title = FormBuilder.SectionTitle("👥  Manage Users  (Activity 5 Module)");

            // ── Input panel ──────────────────────────────────────────────────
            var panel = new Panel
            {
                Location    = new Point(20, 55),
                Size        = new Size(530, 240),
                BackColor   = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            int lx = 12, fx = 130, w = 350;
            panel.Controls.Add(FormBuilder.Label2("Full Name:", lx, 15));
            txtName  = FormBuilder.TextBox2(fx, 12, w); panel.Controls.Add(txtName);

            panel.Controls.Add(FormBuilder.Label2("Username:", lx, 52));
            txtUser  = FormBuilder.TextBox2(fx, 49, w); panel.Controls.Add(txtUser);

            panel.Controls.Add(FormBuilder.Label2("Email:", lx, 89));
            txtEmail = FormBuilder.TextBox2(fx, 86, w); panel.Controls.Add(txtEmail);

            panel.Controls.Add(FormBuilder.Label2("Password:", lx, 126));
            txtPass  = FormBuilder.TextBox2(fx, 123, 200);
            txtPass.PasswordChar = '●';
            panel.Controls.Add(txtPass);
            var lblPassHint = new Label
            {
                Text     = "(leave blank to keep)",
                Location = new Point(fx + 208, 126),
                Size     = new Size(140, 26),
                Font     = new Font("Segoe UI", 8, FontStyle.Italic),
                ForeColor = Color.Gray
            };
            panel.Controls.Add(lblPassHint);

            panel.Controls.Add(FormBuilder.Label2("Role:", lx, 163));
            cmbRole = new ComboBox
            {
                Location     = new Point(fx, 160),
                Size         = new Size(160, 26),
                Font         = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbRole.Items.AddRange(new object[] { "Member", "Librarian", "Admin" });
            cmbRole.SelectedIndex = 0;
            panel.Controls.Add(cmbRole);

            panel.Controls.Add(FormBuilder.Label2("Status:", lx, 200));
            cmbStatus = new ComboBox
            {
                Location     = new Point(fx, 197),
                Size         = new Size(120, 26),
                Font         = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbStatus.Items.AddRange(new object[] { "Active", "Inactive" });
            cmbStatus.SelectedIndex = 0;
            panel.Controls.Add(cmbStatus);

            // Buttons
            btnAdd    = SmBtn("➕ Add",    Color.FromArgb(26, 94,  26),  10, 232);
            btnUpdate = SmBtn("✏ Update",  Color.FromArgb(26, 60,  94), 100, 232);
            btnClear  = SmBtn("✖ Clear",   Color.FromArgb(100,100,100), 190, 232);
            btnAdd.Click    += BtnAdd_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnClear.Click  += (_, _) => ClearForm();
            panel.Controls.AddRange(new Control[] { btnAdd, btnUpdate, btnClear });
            panel.Height = 272;

            lblStatus = FormBuilder.StatusLabel(20, 302, 550);

            grid = FormBuilder.MakeGrid();
            grid.Location      = new Point(20, 325);
            grid.Size          = new Size(Width - 60, Height - 375);
            grid.Anchor        = AnchorStyles.Top | AnchorStyles.Left |
                                 AnchorStyles.Right | AnchorStyles.Bottom;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect   = false;
            grid.CellClick    += Grid_CellClick;

            Controls.AddRange(new Control[] { title, panel, lblStatus, grid });
            Resize += (_, _) => grid.Size = new Size(Width - 60, Height - 375);

            if (SessionHelper.Role != "Admin")
            {
                btnAdd.Enabled    = false;
                btnUpdate.Enabled = false;
                var warn = FormBuilder.Label2("⚠  Admin access required to modify users.", 20, 310);
                warn.ForeColor = Color.OrangeRed;
                Controls.Add(warn);
            }

            ClearForm();
            LoadGrid();
        }

        private static Button SmBtn(string text, Color back, int x, int y) =>
            new Button
            {
                Text      = text,
                Location  = new Point(x, y),
                Size      = new Size(84, 32),
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
                    "SELECT user_id AS `ID`, full_name AS `Full Name`, " +
                    "username AS `Username`, email AS `Email`, " +
                    "role AS `Role`, status AS `Status`, created_at AS `Created` " +
                    "FROM users ORDER BY full_name";
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
            selId          = Convert.ToInt32(row.Cells["ID"].Value);
            txtName.Text   = row.Cells["Full Name"].Value?.ToString() ?? "";
            txtUser.Text   = row.Cells["Username"].Value?.ToString() ?? "";
            txtEmail.Text  = row.Cells["Email"].Value?.ToString() ?? "";
            txtPass.Text   = "";
            cmbRole.Text   = row.Cells["Role"].Value?.ToString() ?? "Member";
            cmbStatus.Text = row.Cells["Status"].Value?.ToString() ?? "Active";
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            if (!Validate2()) return;
            if (string.IsNullOrWhiteSpace(txtPass.Text))
            { ShowError("Password is required for new users."); return; }
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                const string sql =
                    "INSERT INTO users (full_name, username, email, password, role, status) " +
                    "VALUES (@n,@u,@e,@p,@r,@s)";
                using var cmd = new MySqlCommand(sql, conn);
                SetParams(cmd);
                cmd.Parameters.AddWithValue("@p", GetMd5(txtPass.Text));
                cmd.ExecuteNonQuery();
                ShowSuccess("✔  User added.");
                ClearForm(); LoadGrid();
            }
            catch (Exception ex) { ShowError(ex.Message); }
        }

        private void BtnUpdate_Click(object? sender, EventArgs e)
        {
            if (selId < 0) { ShowError("Select a user first."); return; }
            if (!Validate2()) return;
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                string sql = string.IsNullOrWhiteSpace(txtPass.Text)
                    ? "UPDATE users SET full_name=@n, username=@u, email=@e, role=@r, status=@s WHERE user_id=@id"
                    : "UPDATE users SET full_name=@n, username=@u, email=@e, password=@p, role=@r, status=@s WHERE user_id=@id";
                using var cmd = new MySqlCommand(sql, conn);
                SetParams(cmd);
                if (!string.IsNullOrWhiteSpace(txtPass.Text))
                    cmd.Parameters.AddWithValue("@p", GetMd5(txtPass.Text));
                cmd.Parameters.AddWithValue("@id", selId);
                cmd.ExecuteNonQuery();
                ShowSuccess("✔  User updated.");
                ClearForm(); LoadGrid();
            }
            catch (Exception ex) { ShowError(ex.Message); }
        }

        private void SetParams(MySqlCommand cmd)
        {
            cmd.Parameters.AddWithValue("@n", txtName.Text.Trim());
            cmd.Parameters.AddWithValue("@u", txtUser.Text.Trim());
            cmd.Parameters.AddWithValue("@e", txtEmail.Text.Trim());
            cmd.Parameters.AddWithValue("@r", cmbRole.Text);
            cmd.Parameters.AddWithValue("@s", cmbStatus.Text);
        }

        private bool Validate2()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text)  ||
                string.IsNullOrWhiteSpace(txtUser.Text)  ||
                string.IsNullOrWhiteSpace(txtEmail.Text))
            { ShowError("Name, Username, and Email are required."); return false; }
            return true;
        }

        private void ClearForm()
        {
            selId = -1;
            txtName.Text = txtUser.Text = txtEmail.Text = txtPass.Text = "";
            cmbRole.SelectedIndex   = 0;
            cmbStatus.SelectedIndex = 0;
            lblStatus.Text = string.Empty;
        }

        private static string GetMd5(string s)
        {
            var b = MD5.HashData(Encoding.UTF8.GetBytes(s));
            return BitConverter.ToString(b).Replace("-", "").ToLowerInvariant();
        }

        private void ShowError(string m)   { lblStatus.ForeColor = Color.Crimson;             lblStatus.Text = "✖  " + m; }
        private void ShowSuccess(string m) { lblStatus.ForeColor = Color.FromArgb(0, 128, 0); lblStatus.Text = m; }
    }
}
