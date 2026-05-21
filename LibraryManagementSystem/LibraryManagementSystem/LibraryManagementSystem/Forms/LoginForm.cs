using LibraryManagementSystem.Helpers;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using System.Text;

namespace LibraryManagementSystem.Forms
{
    public class LoginForm : Form
    {
        private TextBox txtUsername = null!;
        private TextBox txtPassword = null!;
        private Button  btnLogin    = null!;
        private Label   lblError    = null!;

        public LoginForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text            = "Library Management System – Login";
            Size            = new Size(420, 340);
            StartPosition   = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            BackColor       = Color.FromArgb(245, 248, 252);

            // ── Banner ──────────────────────────────────────────────────────
            var banner = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 70,
                BackColor = Color.FromArgb(26, 60, 94)
            };
            var lblTitle = new Label
            {
                Text      = "📚  City Public Library",
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 16, FontStyle.Bold),
                AutoSize  = false,
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            banner.Controls.Add(lblTitle);

            // ── Form fields ─────────────────────────────────────────────────
            int lx = 50, fx = 160, w = 200, lw = 100;

            var lblUser = MakeLabel("Username:", lx, 100, lw);
            txtUsername = MakeTextBox(fx, 97, w);

            var lblPass = MakeLabel("Password:", lx, 140, lw);
            txtPassword = MakeTextBox(fx, 137, w);
            txtPassword.PasswordChar = '●';

            lblError = new Label
            {
                Text      = string.Empty,
                ForeColor = Color.Crimson,
                Font      = new Font("Segoe UI", 9),
                Location  = new Point(lx, 173),
                Size      = new Size(340, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };

            btnLogin = new Button
            {
                Text      = "Login",
                Location  = new Point(fx, 200),
                Size      = new Size(200, 38),
                BackColor = Color.FromArgb(26, 60, 94),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += BtnLogin_Click;

            Controls.AddRange(new Control[]
            {
                banner, lblUser, txtUsername,
                lblPass, txtPassword,
                lblError, btnLogin
            });

            AcceptButton = btnLogin;
        }

        private static Label MakeLabel(string text, int x, int y, int w) =>
            new Label
            {
                Text     = text,
                Location = new Point(x, y),
                Size     = new Size(w, 26),
                Font     = new Font("Segoe UI", 10),
                TextAlign = ContentAlignment.MiddleRight
            };

        private static TextBox MakeTextBox(int x, int y, int w) =>
            new TextBox
            {
                Location = new Point(x, y),
                Size     = new Size(w, 26),
                Font     = new Font("Segoe UI", 10)
            };

        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            lblError.Text = string.Empty;

            if (string.IsNullOrWhiteSpace(txtUsername.Text) ||
                string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                lblError.Text = "Please enter username and password.";
                return;
            }

            try
            {
                string md5Pass = GetMd5(txtPassword.Text);
                using var conn = DatabaseHelper.GetConnection();
                const string sql =
                    "SELECT user_id, full_name, username, role " +
                    "FROM users " +
                    "WHERE username=@u AND password=@p AND status='Active'";
                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@u", txtUsername.Text.Trim());
                cmd.Parameters.AddWithValue("@p", md5Pass);
                using var rdr = cmd.ExecuteReader();
                if (rdr.Read())
                {
                    SessionHelper.UserId   = rdr.GetInt32("user_id");
                    SessionHelper.FullName = rdr.GetString("full_name");
                    SessionHelper.Username = rdr.GetString("username");
                    SessionHelper.Role     = rdr.GetString("role");

                    Hide();
                    new MainForm().ShowDialog();
                    Close();
                }
                else
                {
                    lblError.Text = "Invalid credentials or inactive account.";
                }
            }
            catch (Exception ex)
            {
                lblError.Text = "DB error: " + ex.Message;
            }
        }

        private static string GetMd5(string input)
        {
            var bytes = MD5.HashData(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
        }
    }
}
