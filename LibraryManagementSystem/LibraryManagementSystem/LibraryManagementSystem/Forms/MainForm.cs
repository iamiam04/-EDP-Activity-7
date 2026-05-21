using LibraryManagementSystem.Helpers;

namespace LibraryManagementSystem.Forms
{
    public class MainForm : Form
    {
        private Panel  sidebar   = null!;
        private Panel  content   = null!;
        private Label  lblWelcome = null!;

        public MainForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text            = "Library Management System";
            Size            = new Size(1180, 700);
            StartPosition   = FormStartPosition.CenterScreen;
            MinimumSize     = new Size(900, 600);
            BackColor       = Color.FromArgb(245, 248, 252);

            // ── Top bar ─────────────────────────────────────────────────────
            var topBar = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 52,
                BackColor = Color.FromArgb(26, 60, 94)
            };
            var lblApp = new Label
            {
                Text      = "📚  City Public Library – Management System",
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 13, FontStyle.Bold),
                Dock      = DockStyle.Left,
                Width     = 500,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(12, 0, 0, 0)
            };
            lblWelcome = new Label
            {
                Text      = $"👤  {SessionHelper.FullName}  ({SessionHelper.Role})",
                ForeColor = Color.FromArgb(180, 210, 240),
                Font      = new Font("Segoe UI", 9),
                Dock      = DockStyle.Right,
                Width     = 280,
                TextAlign = ContentAlignment.MiddleRight,
                Padding   = new Padding(0, 0, 12, 0)
            };
            var btnLogout = new Button
            {
                Text      = "Logout",
                Dock      = DockStyle.Right,
                Width     = 80,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(180, 0, 0),
                Font      = new Font("Segoe UI", 9),
                Cursor    = Cursors.Hand
            };
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.Click += (_, _) => { SessionHelper.Clear(); Close(); };
            topBar.Controls.AddRange(new Control[] { lblApp, lblWelcome, btnLogout });

            // ── Sidebar ─────────────────────────────────────────────────────
            sidebar = new Panel
            {
                Dock      = DockStyle.Left,
                Width     = 190,
                BackColor = Color.FromArgb(36, 75, 115),
                Padding   = new Padding(0, 10, 0, 0)
            };

            // ── Content area ────────────────────────────────────────────────
            content = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = Color.FromArgb(245, 248, 252),
                Padding   = new Padding(12)
            };

            // Navigation buttons
            (string Label, Action Action)[] navItems =
            {
                ("🏠  Dashboard",         ShowDashboard),
                ("➕  Borrow a Book",     () => LoadChild(new BorrowBookForm())),
                ("↩️  Return a Book",     () => LoadChild(new ReturnBookForm())),
                ("📋  Manage Books",      () => LoadChild(new ManageBooksForm())),
                ("👥  Manage Users",      () => LoadChild(new ManageUsersForm())),
                ("─────────────────────", null!),
                ("📊  Borrowings Report", () => LoadChild(new ReportForm("borrowings"))),
                ("📦  Inventory Report",  () => LoadChild(new ReportForm("inventory"))),
                ("👤  User Activity",     () => LoadChild(new ReportForm("useractivity")))
            };

            foreach (var (label, action) in navItems)
            {
                if (action == null)
                {
                    sidebar.Controls.Add(new Label
                    {
                        Text      = "──────────────────",
                        ForeColor = Color.FromArgb(100, 140, 180),
                        Font      = new Font("Segoe UI", 8),
                        Dock      = DockStyle.Top,
                        Height    = 20,
                        TextAlign = ContentAlignment.MiddleCenter
                    });
                    continue;
                }
                var captured = action;
                var btn = new Button
                {
                    Text      = label,
                    Dock      = DockStyle.Top,
                    Height    = 42,
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.White,
                    BackColor = Color.FromArgb(36, 75, 115),
                    Font      = new Font("Segoe UI", 9.5f),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding   = new Padding(10, 0, 0, 0),
                    Cursor    = Cursors.Hand
                };
                btn.FlatAppearance.BorderSize   = 0;
                btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(50, 100, 155);
                btn.Click += (_, _) => captured();
                sidebar.Controls.Add(btn);
            }

            // Reverse dock order so buttons appear top-to-bottom
            var btns = sidebar.Controls.Cast<Control>().ToList();
            sidebar.Controls.Clear();
            btns.Reverse();
            foreach (var c in btns) sidebar.Controls.Add(c);

            Controls.AddRange(new Control[] { content, sidebar, topBar });

            ShowDashboard();
        }

        private void LoadChild(Form child)
        {
            content.Controls.Clear();
            child.TopLevel    = false;
            child.FormBorderStyle = FormBorderStyle.None;
            child.Dock        = DockStyle.Fill;
            content.Controls.Add(child);
            child.Show();
        }

        private void ShowDashboard()
        {
            content.Controls.Clear();
            var lbl = new Label
            {
                Text      = $"Welcome back, {SessionHelper.FullName}!\n\nSelect a module from the sidebar.",
                Font      = new Font("Segoe UI", 15),
                ForeColor = Color.FromArgb(26, 60, 94),
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            content.Controls.Add(lbl);
        }
    }
}
