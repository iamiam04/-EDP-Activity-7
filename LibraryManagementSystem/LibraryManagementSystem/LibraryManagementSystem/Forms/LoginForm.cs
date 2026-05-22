using LibraryManagementSystem.Helpers;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using System.Text;

namespace LibraryManagementSystem.Forms
{
    public class LoginForm : Form
    {
        readonly TextBox txtUser = new() { Location = new(150, 100), Size = new(200, 26), Font = UI.Body };
        readonly TextBox txtPass = new() { Location = new(150, 140), Size = new(200, 26), Font = UI.Body, PasswordChar = '●' };
        readonly Label   lblMsg  = new() { Location = new(40, 178),  Size = new(330, 22), Font = UI.Body, ForeColor = Color.Crimson, TextAlign = ContentAlignment.MiddleCenter };

        public LoginForm()
        {
            Text = "City Public Library – Login";
            Size = new(420, 300); StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false;
            BackColor = Color.White;

            // Banner
            var banner = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = UI.Navy };
            banner.Controls.Add(new Label
            {
                Text = "📚  Grace Library", Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White, TextAlign = ContentAlignment.MiddleCenter
            });

            Controls.Add(banner);
            Controls.Add(UI.Lbl("Username:", 40, 103)); Controls.Add(txtUser);
            Controls.Add(UI.Lbl("Password:", 40, 143)); Controls.Add(txtPass);
            Controls.Add(lblMsg);

            var btn = UI.Btn("Login", 150, 208, 200);
            btn.Height = 38;
            btn.Click += Login;
            Controls.Add(btn);
            AcceptButton = btn;
        }

        void Login(object? s, EventArgs e)
        {
            lblMsg.Text = "";
            try
            {
                using var conn = DB.Open();
                using var cmd  = new MySqlCommand(
                    "SELECT user_id,full_name,username,role FROM users " +
                    "WHERE username=@u AND password=@p AND status='Active'", conn);
                cmd.Parameters.AddWithValue("@u", txtUser.Text.Trim());
                cmd.Parameters.AddWithValue("@p", Md5(txtPass.Text));
                using var r = cmd.ExecuteReader();
                if (r.Read())
                {
                    Session.UserId = r.GetInt32(0); Session.FullName = r.GetString(1);
                    Session.Username = r.GetString(2); Session.Role = r.GetString(3);
                    Hide(); new MainForm().ShowDialog(); Close();
                }
                else lblMsg.Text = "Invalid credentials or inactive account.";
            }
            catch (Exception ex) { lblMsg.Text = "DB Error: " + ex.Message; }
        }

        static string Md5(string s)
        {
            var b = MD5.HashData(Encoding.UTF8.GetBytes(s));
            return BitConverter.ToString(b).Replace("-", "").ToLower();
        }
    }
}