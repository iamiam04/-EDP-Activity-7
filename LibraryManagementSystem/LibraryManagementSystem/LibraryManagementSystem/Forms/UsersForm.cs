using LibraryManagementSystem.Helpers;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using System.Text;

namespace LibraryManagementSystem.Forms
{
    /// <summary>Activity 5 – User Management Module</summary>
    public class UsersForm : Form
    {
        readonly TextBox      txtName   = UI.Txt(145, 55, 260);
        readonly TextBox      txtUser   = UI.Txt(145, 91, 180);
        readonly TextBox      txtEmail  = UI.Txt(145, 127, 260);
        readonly TextBox      txtPass   = UI.Txt(145, 163, 180);
        readonly ComboBox     cmbRole   = UI.Cmb(145, 199, 130);
        readonly ComboBox     cmbStatus = UI.Cmb(300, 199, 110);
        readonly Label        lblMsg    = UI.Msg(20, 242);
        readonly DataGridView grid      = UI.Grid();
        int selId = -1;

        public UsersForm()
        {
            Controls.Add(UI.Title("👥  Manage Users"));
            Controls.Add(UI.Lbl("Full Name:", 20, 58));  Controls.Add(txtName);
            Controls.Add(UI.Lbl("Username:",  20, 94));  Controls.Add(txtUser);
            Controls.Add(UI.Lbl("Email:",     20, 130)); Controls.Add(txtEmail);
            Controls.Add(UI.Lbl("Password:",  20, 166)); Controls.Add(txtPass);
            Controls.Add(new Label { Text = "(blank = keep current)", Location = new(333, 167), AutoSize = true, Font = UI.Small, ForeColor = Color.Gray });
            Controls.Add(UI.Lbl("Role:", 20, 202)); Controls.Add(cmbRole);
            Controls.Add(UI.Lbl("Status:", 253, 202, 45)); Controls.Add(cmbStatus);
            txtPass.PasswordChar = '●';
            cmbRole.Items.AddRange(new object[] { "Member", "Librarian", "Admin" }); cmbRole.SelectedIndex = 0;
            cmbStatus.Items.AddRange(new object[] { "Active", "Inactive" }); cmbStatus.SelectedIndex = 0;
            Controls.Add(lblMsg);

            bool isAdmin = Session.Role == "Admin";
            var bAdd = UI.Btn("Add",    20,  276, 80, UI.Green);
            var bUpd = UI.Btn("Update", 108, 276, 80, UI.Navy);
            var bClr = UI.Btn("Clear",  196, 276, 80, Color.Gray);
            bAdd.Enabled = bUpd.Enabled = isAdmin;
            bAdd.Click += (_, _) => Save();
            bUpd.Click += (_, _) => SaveUpdate();
            bClr.Click += (_, _) => Clear();
            Controls.AddRange(new Control[] { bAdd, bUpd, bClr });

            if (!isAdmin)
                Controls.Add(new Label { Text = "⚠  Admin access required to add or modify users.", Location = new(20, 322), AutoSize = true, ForeColor = Color.OrangeRed, Font = UI.Body });

            Controls.Add(UI.Line(326));

            grid.Location  = new(20, 336);
            grid.Anchor    = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            grid.CellClick += GridClick;
            Controls.Add(grid);
            Resize += (_, _) => grid.Size = new(Width - 55, Height - 351);

            LoadGrid();
        }

        void LoadGrid()
        {
            try
            {
                using var conn = DB.Open();
                using var da = new MySqlDataAdapter(
                    "SELECT user_id AS ID, full_name AS Name, username AS Username, " +
                    "email AS Email, role AS Role, status AS Status FROM users ORDER BY full_name", conn);
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
            txtName.Text = r.Cells["Name"].Value?.ToString() ?? "";
            txtUser.Text = r.Cells["Username"].Value?.ToString() ?? "";
            txtEmail.Text = r.Cells["Email"].Value?.ToString() ?? "";
            txtPass.Text = "";
            cmbRole.Text = r.Cells["Role"].Value?.ToString() ?? "Member";
            cmbStatus.Text = r.Cells["Status"].Value?.ToString() ?? "Active";
        }

        void Save()
        {
            if (!Valid()) return;
            if (string.IsNullOrWhiteSpace(txtPass.Text)) { Msg("Password is required for new users.", true); return; }
            try
            {
                using var conn = DB.Open();
                using var cmd = new MySqlCommand("INSERT INTO users(full_name,username,email,password,role,status) VALUES(@n,@u,@e,@p,@r,@s)", conn);
                SetParams(cmd, true); cmd.ExecuteNonQuery();
                Msg("✔  User added.", false); Clear(); LoadGrid();
            }
            catch (Exception ex) { Msg(ex.Message, true); }
        }

        void SaveUpdate()
        {
            if (selId < 0) { Msg("Select a user first.", true); return; }
            if (!Valid()) return;
            try
            {
                using var conn = DB.Open();
                bool cp = !string.IsNullOrWhiteSpace(txtPass.Text);
                string sql = cp
                    ? "UPDATE users SET full_name=@n,username=@u,email=@e,password=@p,role=@r,status=@s WHERE user_id=@id"
                    : "UPDATE users SET full_name=@n,username=@u,email=@e,role=@r,status=@s WHERE user_id=@id";
                using var cmd = new MySqlCommand(sql, conn);
                SetParams(cmd, cp); cmd.Parameters.AddWithValue("@id", selId);
                cmd.ExecuteNonQuery();
                Msg("✔  User updated.", false); Clear(); LoadGrid();
            }
            catch (Exception ex) { Msg(ex.Message, true); }
        }

        void SetParams(MySqlCommand cmd, bool withPass)
        {
            cmd.Parameters.AddWithValue("@n", txtName.Text.Trim());
            cmd.Parameters.AddWithValue("@u", txtUser.Text.Trim());
            cmd.Parameters.AddWithValue("@e", txtEmail.Text.Trim());
            if (withPass) cmd.Parameters.AddWithValue("@p", Md5(txtPass.Text));
            cmd.Parameters.AddWithValue("@r", cmbRole.Text);
            cmd.Parameters.AddWithValue("@s", cmbStatus.Text);
        }

        bool Valid() { if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtUser.Text) || string.IsNullOrWhiteSpace(txtEmail.Text)) { Msg("Name, Username, and Email are required.", true); return false; } return true; }
        void Clear() { selId = -1; txtName.Text = txtUser.Text = txtEmail.Text = txtPass.Text = ""; cmbRole.SelectedIndex = 0; cmbStatus.SelectedIndex = 0; lblMsg.Text = ""; }
        void Msg(string m, bool err) { lblMsg.ForeColor = err ? Color.Crimson : UI.Green; lblMsg.Text = m; }
        static string Md5(string s) { var b = MD5.HashData(Encoding.UTF8.GetBytes(s)); return BitConverter.ToString(b).Replace("-", "").ToLower(); }
    }
}