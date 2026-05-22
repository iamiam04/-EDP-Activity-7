using ClosedXML.Excel;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Data;
using A   = DocumentFormat.OpenXml.Drawing;
using C   = DocumentFormat.OpenXml.Drawing.Charts;
using XDr = DocumentFormat.OpenXml.Drawing.Spreadsheet;

namespace LibraryManagementSystem.Helpers
{
    public static class ExcelExporter
    {
        const string Company = "City Public Library";
        const string Address = "123 Knowledge Ave, Quezon City";


        public static void Export(string mode, System.Data.DataTable dt, string path)
        {
            var cfg = Config(mode);
            using (var wb = new XLWorkbook())
            {
                Sheet1(wb, cfg.title, cfg.cols, cfg.rows(dt));
                Sheet2(wb, cfg.chartTitle, cfg.series, cfg.chartData(dt));
                wb.SaveAs(path);
            }
            InjectChart(path, cfg.chartTitle, cfg.series, cfg.chartData(dt));
        }


        static void Sheet1(XLWorkbook wb, string title, string[] cols, IEnumerable<object?[]> rows)
        {
            var ws = wb.AddWorksheet("Report");

            Cell(ws, 1, 1, cols.Length, Company, 16, "#1a3c5e", bold: true);
            Cell(ws, 2, 1, cols.Length, Address,  10, null,     italic: true);
            Cell(ws, 3, 1, cols.Length, title,    13, "#2e6da4", bold: true);
            Cell(ws, 4, 1, cols.Length, $"Generated: {DateTime.Now:MMMM dd, yyyy HH:mm}  |  By: {Session.FullName}", 9, null, italic: true);

            var logo = ws.Range(1, cols.Length + 1, 4, cols.Length + 2);
            logo.Merge(); logo.FirstCell().Value = "[LIBRARY LOGO]";
            logo.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            logo.Style.Alignment.Vertical   = XLAlignmentVerticalValues.Center;
            logo.Style.Border.OutsideBorder  = XLBorderStyleValues.Thin;
            logo.Style.Font.Italic = true; logo.Style.Font.FontColor = XLColor.Gray;
            ws.Row(5).Height = 5;

            for (int c = 0; c < cols.Length; c++) ws.Cell(6, c + 1).Value = cols[c];
            var hdr = ws.Range(6, 1, 6, cols.Length);
            hdr.Style.Fill.BackgroundColor = XLColor.FromHtml("#1a3c5e");
            hdr.Style.Font.FontColor = XLColor.White; hdr.Style.Font.Bold = true;
            hdr.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            int r = 7;
            foreach (var row in rows)
            {
                for (int c = 0; c < row.Length; c++)
                    ws.Cell(r, c + 1).Value = XLCellValue.FromObject(row[c] ?? "");
                if (r % 2 == 0)
                    ws.Range(r, 1, r, cols.Length).Style.Fill.BackgroundColor = XLColor.FromHtml("#eaf1fb");
                r++;
            }
            int last = r - 1;
            var tbl = ws.Range(6, 1, last, cols.Length);
            tbl.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            tbl.Style.Border.InsideBorder  = XLBorderStyleValues.Hair;
            ws.Columns().AdjustToContents();

            int sig = last + 3;
            ws.Cell(sig,     1).Value = "Prepared by:";
            ws.Cell(sig + 1, 1).Value = Session.FullName; ws.Cell(sig + 1, 1).Style.Font.Bold = true;
            ws.Cell(sig + 2, 1).Value = $"({Session.Role})";
            ws.Range(sig + 3, 1, sig + 3, 3).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            ws.Cell(sig + 4, 1).Value = "Signature over Printed Name"; ws.Cell(sig + 4, 1).Style.Font.Italic = true;
            int ac = Math.Max(1, cols.Length - 1);
            ws.Cell(sig,     ac).Value = "Noted/Approved by:";
            ws.Cell(sig + 1, ac).Value = "_________________________";
            ws.Cell(sig + 2, ac).Value = "(Library Director)"; ws.Cell(sig + 2, ac).Style.Font.Italic = true;
        }


        static void Sheet2(XLWorkbook wb, string chartTitle, string[] series, (string[] cats, double[][] vals) d)
        {
            var ws = wb.AddWorksheet("Chart");
            ws.Cell(1, 1).Value = chartTitle; ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Range(1, 1, 1, series.Length + 1).Merge();
            ws.Cell(2, 1).Value = "Label"; ws.Cell(2, 1).Style.Font.Bold = true;
            for (int s = 0; s < series.Length; s++) { ws.Cell(2, s + 2).Value = series[s]; ws.Cell(2, s + 2).Style.Font.Bold = true; }
            for (int i = 0; i < d.cats.Length; i++)
            {
                ws.Cell(i + 3, 1).Value = d.cats[i];
                for (int s = 0; s < series.Length; s++) ws.Cell(i + 3, s + 2).Value = d.vals[s][i];
            }
            ws.Columns().AdjustToContents();
        }


        static void InjectChart(string path, string chartTitle, string[] series, (string[] cats, double[][] vals) d)
        {
            if (d.cats.Length == 0) return;
            using var doc = SpreadsheetDocument.Open(path, isEditable: true);
            var wbPart    = doc.WorkbookPart!;
            var sheet     = wbPart.Workbook.Descendants<Sheet>().FirstOrDefault(s => s.Name?.Value == "Chart");
            if (sheet?.Id?.Value is null) return;

            var wsPart       = (WorksheetPart)wbPart.GetPartById(sheet.Id.Value);
            var drawingsPart = wsPart.AddNewPart<DrawingsPart>();
            var chartPart    = drawingsPart.AddNewPart<ChartPart>();

            chartPart.ChartSpace = ChartSpace(chartTitle, series, d.cats, d.vals);
            chartPart.ChartSpace.Save();
            drawingsPart.WorksheetDrawing = Drawing(drawingsPart.GetIdOfPart(chartPart));
            drawingsPart.WorksheetDrawing.Save();
            wsPart.Worksheet.Append(new DocumentFormat.OpenXml.Spreadsheet.Drawing { Id = wsPart.GetIdOfPart(drawingsPart) });
            wsPart.Worksheet.Save();
        }

        static C.ChartSpace ChartSpace(string title, string[] series, string[] cats, double[][] vals)
        {
            var cs    = new C.ChartSpace();
            var chart = new C.Chart();
            chart.Append(new C.AutoTitleDeleted { Val = false });
            chart.Append(Title(title));

            var plot = new C.PlotArea();
            plot.Append(new C.Layout());
            var bar = new C.BarChart();
            bar.Append(new C.BarDirection { Val = C.BarDirectionValues.Column });
            bar.Append(new C.BarGrouping  { Val = C.BarGroupingValues.Clustered });
            bar.Append(new C.VaryColors   { Val = false });

            uint[] colors = { 0x1A3C5Eu, 0x2E86C1u, 0x28B463u, 0xE67E22u };
            for (int s = 0; s < series.Length; s++)
            {
                var ser = new C.BarChartSeries();
                ser.Append(new C.Index { Val = (uint)s });
                ser.Append(new C.Order { Val = (uint)s });

                var stx = new C.SeriesText();
                var sr  = new C.StringReference();
                sr.Append(new C.Formula($"\"{series[s]}\""));
                sr.Append(new C.StringCache(new C.PointCount { Val = 1u }, new C.StringPoint { Index = 0u, NumericValue = new C.NumericValue(series[s]) }));
                stx.Append(sr); ser.Append(stx);

                var fill = new A.SolidFill();
                fill.Append(new A.RgbColorModelHex { Val = colors[s % colors.Length].ToString("X6") });
                var sp = new C.ShapeProperties(); sp.Append(fill); ser.Append(sp);

                var catCache = new C.StringCache();
                catCache.Append(new C.PointCount { Val = (uint)cats.Length });
                for (int i = 0; i < cats.Length; i++)
                    catCache.Append(new C.StringPoint { Index = (uint)i, NumericValue = new C.NumericValue(cats[i]) });
                var catRef = new C.StringReference();
                catRef.Append(new C.Formula($"'Chart'!$A$3:$A${cats.Length + 2}"));
                catRef.Append(catCache);
                var catAx = new C.CategoryAxisData(); catAx.Append(catRef); ser.Append(catAx);

                var numCache = new C.NumberingCache();
                numCache.Append(new C.FormatCode("General"));
                numCache.Append(new C.PointCount { Val = (uint)vals[s].Length });
                for (int i = 0; i < vals[s].Length; i++)
                    numCache.Append(new C.NumericPoint { Index = (uint)i, NumericValue = new C.NumericValue(vals[s][i].ToString("G")) });
                var numRef = new C.NumberReference();
                numRef.Append(new C.Formula($"'Chart'!${Col(s + 2)}$3:${Col(s + 2)}${vals[s].Length + 2}"));
                numRef.Append(numCache);
                var v = new C.Values(); v.Append(numRef); ser.Append(v);

                bar.Append(ser);
            }
            bar.Append(new C.AxisId { Val = 1u }); bar.Append(new C.AxisId { Val = 2u });
            plot.Append(bar);

            CatAxis(plot, 1u, C.AxisPositionValues.Bottom, 2u);
            ValAxis(plot, 2u, C.AxisPositionValues.Left, 1u);

            chart.Append(plot);
            chart.Append(new C.Legend(new C.LegendPosition { Val = C.LegendPositionValues.Bottom }));
            chart.Append(new C.PlotVisibleOnly { Val = true });
            cs.Append(new C.EditingLanguage { Val = "en-US" });
            cs.Append(chart);
            return cs;
        }

        static void CatAxis(C.PlotArea plot, uint id, C.AxisPositionValues pos, uint crossId)
        {
            var ax = new C.CategoryAxis();
            ax.Append(new C.AxisId       { Val = id });
            ax.Append(new C.Scaling(new C.Orientation { Val = C.OrientationValues.MinMax }));
            ax.Append(new C.Delete       { Val = false });
            ax.Append(new C.AxisPosition { Val = pos });
            ax.Append(new C.CrossingAxis { Val = crossId });
            plot.Append(ax);
        }

        static void ValAxis(C.PlotArea plot, uint id, C.AxisPositionValues pos, uint crossId)
        {
            var ax = new C.ValueAxis();
            ax.Append(new C.AxisId       { Val = id });
            ax.Append(new C.Scaling(new C.Orientation { Val = C.OrientationValues.MinMax }));
            ax.Append(new C.Delete       { Val = false });
            ax.Append(new C.AxisPosition { Val = pos });
            ax.Append(new C.CrossingAxis { Val = crossId });
            plot.Append(ax);
        }

        static C.Title Title(string text)
        {
            var run = new A.Run();
            run.Append(new A.RunProperties { Language = "en-US", Bold = true });
            run.Append(new A.Text(text));
            var para = new A.Paragraph(); para.Append(run);
            var rich = new C.RichText();
            rich.Append(new A.BodyProperties()); rich.Append(new A.ListStyle()); rich.Append(para);
            var tx = new C.ChartText(); tx.Append(rich);
            var t  = new C.Title(); t.Append(tx); t.Append(new C.Overlay { Val = false });
            return t;
        }

        static XDr.WorksheetDrawing Drawing(string relId)
        {
            var anchor = new XDr.TwoCellAnchor();
            anchor.Append(new XDr.FromMarker(new XDr.ColumnId("0"), new XDr.ColumnOffset("0"), new XDr.RowId("4"),  new XDr.RowOffset("0")));
            anchor.Append(new XDr.ToMarker(  new XDr.ColumnId("9"), new XDr.ColumnOffset("0"), new XDr.RowId("24"), new XDr.RowOffset("0")));
            var gData = new A.GraphicData { Uri = "http://schemas.openxmlformats.org/drawingml/2006/chart" };
            gData.Append(new C.ChartReference { Id = relId });
            var g = new A.Graphic(); g.Append(gData);
            var frame = new XDr.GraphicFrame();
            frame.Append(new XDr.NonVisualGraphicFrameProperties(new XDr.NonVisualDrawingProperties { Id = 2u, Name = "Chart 1" }, new XDr.NonVisualGraphicFrameDrawingProperties()));
            frame.Append(new XDr.Transform(new A.Offset { X = 0L, Y = 0L }, new A.Extents { Cx = 0L, Cy = 0L }));
            frame.Append(g);
            anchor.Append(frame); anchor.Append(new XDr.ClientData());
            var wd = new XDr.WorksheetDrawing(); wd.Append(anchor);
            return wd;
        }


        record Cfg(string title, string[] cols,
            Func<System.Data.DataTable, IEnumerable<object?[]>> rows,
            string chartTitle, string[] series,
            Func<System.Data.DataTable, (string[] cats, double[][] vals)> chartData);

        static Cfg Config(string mode) => mode switch
        {
            "borrowings" => new(
                "Borrowings Transaction Report",
                new[] { "ID", "Book Title", "Borrower", "Borrow Date", "Due Date", "Return Date", "Fine (PHP)" },
                dt => dt.Rows.Cast<DataRow>().Select(r => new object?[]
                {
                    r["borrowing_id"], r["title"], r["borrower"], r["borrow_date"], r["due_date"],
                    string.IsNullOrEmpty(r["return_date"]?.ToString()) ? "Not returned" : r["return_date"],
                    Convert.ToDecimal(r["fine_amount"] ?? 0)
                }),
                "Fine Amounts per Borrower", new[] { "Fine (PHP)" },
                dt => { var rows = dt.Rows.Cast<DataRow>().ToArray(); return (rows.Select(r => r["borrower"]?.ToString() ?? "").ToArray(), new[] { rows.Select(r => Convert.ToDouble(r["fine_amount"] ?? 0)).ToArray() }); }),

            "inventory" => new(
                "Books Inventory Report",
                new[] { "ID", "Title", "Author", "ISBN", "Total", "Available", "Borrowed" },
                dt => dt.Rows.Cast<DataRow>().Select(r => { int t = Convert.ToInt32(r["total_copies"] ?? 0), a = Convert.ToInt32(r["available_copies"] ?? 0); return new object?[] { r["book_id"], r["title"], r["author"], r["isbn"], t, a, t - a }; }),
                "Available vs Borrowed Copies", new[] { "Available", "Borrowed" },
                dt => { var rows = dt.Rows.Cast<DataRow>().ToArray(); return (rows.Select(r => r["title"]?.ToString() ?? "").ToArray(), new[] { rows.Select(r => Convert.ToDouble(r["available_copies"] ?? 0)).ToArray(), rows.Select(r => Convert.ToDouble(r["total_copies"] ?? 0) - Convert.ToDouble(r["available_copies"] ?? 0)).ToArray() }); }),

            "useractivity" => new(
                "User Activity Report",
                new[] { "ID", "Full Name", "Username", "Role", "Status", "Borrowings", "Active Loans", "Total Fines (PHP)" },
                dt => dt.Rows.Cast<DataRow>().Select(r => new object?[] { r["user_id"], r["full_name"], r["username"], r["role"], r["status"], r["total_borrowings"], r["active_loans"], Convert.ToDecimal(r["total_fines"] ?? 0) }),
                "Borrowings & Fines per User", new[] { "Borrowings", "Fines (PHP)" },
                dt => { var rows = dt.Rows.Cast<DataRow>().ToArray(); return (rows.Select(r => r["full_name"]?.ToString() ?? "").ToArray(), new[] { rows.Select(r => Convert.ToDouble(r["total_borrowings"] ?? 0)).ToArray(), rows.Select(r => Convert.ToDouble(r["total_fines"] ?? 0)).ToArray() }); }),

            _ => throw new ArgumentException("Unknown mode: " + mode)
        };


        static void Cell(IXLWorksheet ws, int row, int c1, int cLen, string val, int size, string? hex, bool bold = false, bool italic = false)
        {
            var range = ws.Range(row, c1, row, cLen);
            range.Merge(); range.FirstCell().Value = val;
            range.Style.Font.FontSize = size; range.Style.Font.Bold = bold; range.Style.Font.Italic = italic;
            if (hex != null) range.Style.Font.FontColor = XLColor.FromHtml(hex);
        }

        static string Col(int n) { string s = ""; while (n > 0) { s = (char)('A' + (n - 1) % 26) + s; n = (n - 1) / 26; } return s; }
    }
}