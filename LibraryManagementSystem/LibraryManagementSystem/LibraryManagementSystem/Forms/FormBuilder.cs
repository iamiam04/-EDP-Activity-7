namespace LibraryManagementSystem.Forms
{
    /// <summary>
    /// Shared UI factory – keeps every form visually consistent
    /// without repeating boilerplate styling code.
    /// </summary>
    internal static class FormBuilder
    {
        // ── Typography ───────────────────────────────────────────────────────

        public static Label SectionTitle(string text) => new Label
        {
            Text      = text,
            Location  = new Point(20, 12),
            AutoSize  = true,
            Font      = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Color.FromArgb(26, 60, 94)
        };

        public static Label Label2(string text, int x, int y, int width = 120) => new Label
        {
            Text      = text,
            Location  = new Point(x, y),
            Size      = new Size(width, 26),
            Font      = new Font("Segoe UI", 10),
            TextAlign = ContentAlignment.MiddleRight
        };

        public static Label StatusLabel(int x, int y, int width) => new Label
        {
            Text      = string.Empty,
            Location  = new Point(x, y),
            Size      = new Size(width, 24),
            Font      = new Font("Segoe UI", 9.5f),
            AutoSize  = false
        };

        // ── Inputs ───────────────────────────────────────────────────────────

        public static TextBox TextBox2(int x, int y, int width) => new TextBox
        {
            Location = new Point(x, y),
            Size     = new Size(width, 26),
            Font     = new Font("Segoe UI", 10)
        };

        public static ComboBox Combo(int x, int y, int width) => new ComboBox
        {
            Location     = new Point(x, y),
            Size         = new Size(width, 26),
            Font         = new Font("Segoe UI", 10),
            DropDownStyle = ComboBoxStyle.DropDownList
        };

        // ── Buttons ──────────────────────────────────────────────────────────

        public static Button PrimaryButton(string text, int x, int y) => new Button
        {
            Text      = text,
            Location  = new Point(x, y),
            Size      = new Size(200, 36),
            BackColor = Color.FromArgb(26, 60, 94),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font      = new Font("Segoe UI", 10, FontStyle.Bold),
            Cursor    = Cursors.Hand
        };

        // ── Separator ────────────────────────────────────────────────────────

        public static Panel Separator(int y) => new Panel
        {
            Location  = new Point(0, y),
            Height    = 1,
            Dock      = DockStyle.None,
            BackColor = Color.FromArgb(200, 210, 225),
            Width     = 2000
        };

        // ── DataGridView ─────────────────────────────────────────────────────

        public static DataGridView MakeGrid() => new DataGridView
        {
            ReadOnly          = true,
            AllowUserToAddRows    = false,
            AllowUserToDeleteRows = false,
            AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill,
            BorderStyle           = BorderStyle.None,
            BackgroundColor       = Color.White,
            GridColor             = Color.FromArgb(220, 228, 240),
            RowHeadersVisible     = false,
            Font                  = new Font("Segoe UI", 9.5f),
            SelectionMode         = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect           = false,
            CellBorderStyle       = DataGridViewCellBorderStyle.SingleHorizontal
        };

        /// <summary>Applies header + alternating-row colours to an existing grid.</summary>
        public static void StyleGrid(DataGridView grid)
        {
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersDefaultCellStyle.BackColor  = Color.FromArgb(26, 60, 94);
            grid.ColumnHeadersDefaultCellStyle.ForeColor  = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font       = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            grid.ColumnHeadersDefaultCellStyle.Padding    = new Padding(4);
            grid.ColumnHeadersHeight = 34;

            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(234, 241, 251);
            grid.DefaultCellStyle.SelectionBackColor       = Color.FromArgb(46, 109, 164);
            grid.DefaultCellStyle.SelectionForeColor       = Color.White;
            grid.DefaultCellStyle.Padding                  = new Padding(3);
            grid.RowTemplate.Height = 28;
        }
    }
}
