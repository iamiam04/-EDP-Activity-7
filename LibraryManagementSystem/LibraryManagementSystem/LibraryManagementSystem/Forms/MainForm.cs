using LibraryManagementSystem.Helpers;

namespace LibraryManagementSystem.Forms
{
    public class MainForm : Form
    {
        readonly Panel content = new() { Dock = DockStyle.Fill, BackColor = UI.Light, Padding = new(10) };

        public MainForm()
        {
            Text = "City Public Library – Management System";
            Size = new(1100, 680); StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new(900, 580);

            var top = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = UI.Navy };
            top.Controls.Add(new Label
            {
                Text = "📚  City Public Library", Dock = DockStyle.Left, Width = 380,
                Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleLeft, Padding = new(14, 0, 0, 0)
            });
            top.Controls.Add(new Label
            {
                Text = $"{Session.FullName}  |  {Session.Role}",
                Dock = DockStyle.Right, Width = 260, Font = UI.Body,
                ForeColor = Color.FromArgb(170, 205, 235),
                TextAlign = ContentAlignment.MiddleRight, Padding = new(0, 0, 12, 0)
            });
            var logout = UI.Btn("Logout", 0, 0, 80, UI.Red);
            logout.Dock = DockStyle.Right; logout.Click += (_, _) => Close();
            top.Controls.Add(logout);

            var side = new Panel { Dock = DockStyle.Left, Width = 182, BackColor = UI.Teal };

            (string Text, Func<Form>? Make)[] nav =
            {
                ("➕  Borrow a Book",    () => new BorrowForm()),
                ("↩️  Return a Book",    () => new ReturnForm()),
                ("📋  Manage Books",     () => new BooksForm()),
                ("👥  Manage Users",     () => new UsersForm()),
                ("",                     null),
                ("📊  Borrowings",       () => new ReportForm("borrowings")),
                ("📦  Inventory",        () => new ReportForm("inventory")),
                ("👤  User Activity",    () => new ReportForm("useractivity")),
            };

            foreach (var (text, make) in nav)
            {
                if (make == null)
                {
                    side.Controls.Add(new Label
                    {
                        Text = "── REPORTS ──", Dock = DockStyle.Top, Height = 26,
                        Font = UI.Small, ForeColor = Color.FromArgb(120, 160, 200),
                        TextAlign = ContentAlignment.MiddleCenter
                    });
                    continue;
                }
                var m   = make;
                var btn = new Button
                {
                    Text = text, Dock = DockStyle.Top, Height = 44,
                    FlatStyle = FlatStyle.Flat, ForeColor = Color.White,
                    BackColor = UI.Teal, Font = new Font("Segoe UI", 9.5f),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new(12, 0, 0, 0), Cursor = Cursors.Hand
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(55, 105, 160);
                btn.Click += (_, _) => Show(m());
                side.Controls.Add(btn);
            }

            var items = side.Controls.Cast<Control>().ToList();
            side.Controls.Clear(); items.Reverse();
            foreach (var c in items) side.Controls.Add(c);

            Controls.Add(content); Controls.Add(side); Controls.Add(top);
            Show(new BorrowForm());
        }

        void Show(Form f)
        {
            content.Controls.Clear();
            f.TopLevel = false; f.FormBorderStyle = FormBorderStyle.None;
            f.Dock = DockStyle.Fill; f.BackColor = UI.Light;
            content.Controls.Add(f); f.Show();
        }
    }
}