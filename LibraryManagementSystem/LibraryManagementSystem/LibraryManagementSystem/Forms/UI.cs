namespace LibraryManagementSystem.Forms
{
    /// <summary>Shared colours, fonts, and control factories for every form.</summary>
    internal static class UI
    {
        // ── Palette ───────────────────────────────────────────────────────────
        public static readonly Color Navy   = Color.FromArgb(26,  60,  94);
        public static readonly Color Teal   = Color.FromArgb(36,  75, 115);
        public static readonly Color Accent = Color.FromArgb(46, 109, 164);
        public static readonly Color Light  = Color.FromArgb(245, 248, 252);
        public static readonly Color Green  = Color.FromArgb(21, 128, 61);
        public static readonly Color Red    = Color.FromArgb(160, 30,  30);

        // ── Fonts ─────────────────────────────────────────────────────────────
        public static readonly Font Body  = new("Segoe UI", 10);
        public static readonly Font Bold  = new("Segoe UI", 10, FontStyle.Bold);
        public static readonly Font Small = new("Segoe UI",  8);
        public static readonly Font H1    = new("Segoe UI", 14, FontStyle.Bold);
        public static readonly Font H2    = new("Segoe UI", 11, FontStyle.Bold);

        // ── Control factories ─────────────────────────────────────────────────
        public static Label Title(string text) => new()
        {
            Text = text, Location = new(20, 14), AutoSize = true, Font = H1, ForeColor = Navy
        };

        public static Label Lbl(string text, int x, int y, int w = 115) => new()
        {
            Text = text, Location = new(x, y), Size = new(w, 26),
            Font = Body, TextAlign = ContentAlignment.MiddleRight
        };

        public static TextBox Txt(int x, int y, int w = 230) => new()
        {
            Location = new(x, y), Size = new(w, 26), Font = Body
        };

        public static ComboBox Cmb(int x, int y, int w = 190) => new()
        {
            Location = new(x, y), Size = new(w, 26), Font = Body,
            DropDownStyle = ComboBoxStyle.DropDownList
        };

        public static Button Btn(string text, int x, int y, int w = 120, Color? bg = null) => new()
        {
            Text = text, Location = new(x, y), Size = new(w, 34),
            Font = Bold, FlatStyle = FlatStyle.Flat,
            BackColor = bg ?? Navy, ForeColor = Color.White, Cursor = Cursors.Hand,
        };

        public static Label Msg(int x, int y, int w = 500) => new()
        {
            Location = new(x, y), Size = new(w, 24), Font = Body
        };

        public static Panel Line(int y) => new()
        {
            Location = new(0, y), Size = new(4000, 1), BackColor = Color.FromArgb(210, 220, 235)
        };

        // ── Grid ──────────────────────────────────────────────────────────────
        public static DataGridView Grid() => new()
        {
            ReadOnly              = true,
            AllowUserToAddRows    = false,
            AllowUserToDeleteRows = false,
            AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode         = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect           = false,
            BackgroundColor       = Color.White,
            BorderStyle           = BorderStyle.None,
            Font                  = new Font("Segoe UI", 9.5f),
            RowHeadersVisible     = false,
            CellBorderStyle       = DataGridViewCellBorderStyle.SingleHorizontal,
            GridColor             = Color.FromArgb(220, 230, 242)
        };

        public static void StyleGrid(DataGridView g)
        {
            g.EnableHeadersVisualStyles = false;
            g.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Navy, ForeColor = Color.White, Font = Bold,
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Padding   = new Padding(4)
            };
            g.ColumnHeadersHeight = 36;
            g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(234, 241, 251);
            g.DefaultCellStyle.SelectionBackColor       = Accent;
            g.DefaultCellStyle.SelectionForeColor       = Color.White;
            g.DefaultCellStyle.Padding                  = new Padding(2, 0, 2, 0);
            g.RowTemplate.Height = 28;
        }
    }
}