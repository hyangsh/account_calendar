using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace account_calendar
{
    public class CustomCalendar : Control
    {
        private Rectangle leftArrowRect;
        private Rectangle rightArrowRect;
        private int headerHeight = 36;
        private int weekdayHeight = 22;
        private int padding = 8;
        private int rows = 6;
        private int cols = 7;

        public DateTime DisplayDate { get; private set; }
        public DateTime SelectedDate { get; private set; }

        public HashSet<DateTime> DatesWithData { get; } = new HashSet<DateTime>();

        public event EventHandler<DateSelectedEventArgs> DateSelected;

        public CustomCalendar()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);

            BackColor = Color.White;
            ForeColor = Color.Black;
            DisplayDate = DateTime.Today;
            SelectedDate = DateTime.Today;
            Font = new Font("Segoe UI", 9F);
            MinimumSize = new Size(240, 180);
        }

        public void AddDataDate(DateTime dt)
        {
            DatesWithData.Add(dt.Date);
            Invalidate();
        }

        public void RemoveDataDate(DateTime dt)
        {
            DatesWithData.Remove(dt.Date);
            Invalidate();
        }

        public void ClearDataDates()
        {
            DatesWithData.Clear();
            Invalidate();
        }

        public void PrevMonth()
        {
            DisplayDate = DisplayDate.AddMonths(-1);
            Invalidate();
        }

        public void NextMonth()
        {
            DisplayDate = DisplayDate.AddMonths(1);
            Invalidate();
        }

        public void GoToToday()
        {
            DisplayDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            SelectedDate = DateTime.Today;
            Invalidate();
        }

        public void SetDate(DateTime date)
        {
            DisplayDate = new DateTime(date.Year, date.Month, 1);
            SelectedDate = date.Date;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.Clear(BackColor);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            int w = ClientSize.Width;
            int h = ClientSize.Height;
            int gridY = padding + headerHeight + weekdayHeight;

            int cellWidth = Math.Max(24, (w - padding * 2) / cols);
            int cellHeight = Math.Max(24, (h - gridY - padding) / rows);

            Rectangle headerRect = new Rectangle(padding, padding, w - padding * 2, headerHeight);
            leftArrowRect = new Rectangle(headerRect.Left, headerRect.Top + (headerHeight - 24) / 2, 24, 24);
            rightArrowRect = new Rectangle(headerRect.Right - 24, leftArrowRect.Top, 24, 24);

            // Month title
            string monthTitle = DisplayDate.ToString("yyyy MMMM", CultureInfo.CurrentCulture);
            using (var font = new Font(Font.FontFamily, Font.Size + 1.5f, FontStyle.Bold))
            using (var brush = new SolidBrush(ForeColor))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                g.DrawString(monthTitle, font, brush, headerRect, sf);
            }

            // arrows
            using (var arrowBrush = new SolidBrush(Color.FromArgb(80, 80, 80)))
            {
                DrawLeftArrow(g, leftArrowRect, arrowBrush);
                DrawRightArrow(g, rightArrowRect, arrowBrush);
            }

            // weekday header
            var dtfi = CultureInfo.CurrentCulture.DateTimeFormat;
            string[] dayNames = dtfi.AbbreviatedDayNames;
            int firstDayOfWeek = (int)dtfi.FirstDayOfWeek;
            for (int c = 0; c < cols; c++)
            {
                int dayIndex = (firstDayOfWeek + c) % 7;
                string dname = dayNames[dayIndex];
                Rectangle rect = new Rectangle(padding + c * cellWidth, padding + headerHeight, cellWidth, weekdayHeight);
                using (var brush = new SolidBrush(Color.FromArgb(100, 100, 100)))
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                using (var f = new Font(Font.FontFamily, Font.Size - 1, FontStyle.Regular))
                {
                    g.DrawString(dname, f, brush, rect, sf);
                }
            }

            // days
            DateTime firstOfMonth = new DateTime(DisplayDate.Year, DisplayDate.Month, 1);
            int firstIndex = (int)firstOfMonth.DayOfWeek;
            int offset = (firstIndex - firstDayOfWeek + 7) % 7;
            int days = DateTime.DaysInMonth(DisplayDate.Year, DisplayDate.Month);

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    int idx = r * cols + c;
                    int dayNumber = idx - offset + 1;
                    Rectangle cellRect = new Rectangle(padding + c * cellWidth, gridY + r * cellHeight, cellWidth, cellHeight);

                    if (dayNumber >= 1 && dayNumber <= days)
                    {
                        DateTime cellDate = new DateTime(DisplayDate.Year, DisplayDate.Month, dayNumber);
                        bool isSelected = cellDate.Date == SelectedDate.Date;
                        bool isToday = cellDate.Date == DateTime.Today;
                        bool hasData = DatesWithData.Contains(cellDate.Date);

                        if (isSelected)
                        {
                            using (var selBrush = new SolidBrush(Color.FromArgb(42, 130, 210)))
                            {
                                g.FillRectangle(selBrush, cellRect);
                            }
                        }

                        using (var numFont = new Font(Font.FontFamily, Font.Size, isSelected ? FontStyle.Bold : FontStyle.Regular))
                        using (var numBrush = new SolidBrush(isSelected ? Color.White : (isToday ? Color.FromArgb(42, 130, 210) : ForeColor)))
                        using (var sf = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near })
                        {
                            var numRect = new Rectangle(cellRect.Left + 4, cellRect.Top + 4, cellRect.Width - 8, 18);
                            g.DrawString(dayNumber.ToString(), numFont, numBrush, numRect, sf);
                        }

                        if (hasData)
                        {
                            int dotSize = 6;
                            int cx = cellRect.Left + cellRect.Width / 2;
                            int cy = cellRect.Bottom - 8;
                            Rectangle dotRect = new Rectangle(cx - dotSize / 2, cy - dotSize / 2, dotSize, dotSize);
                            using (var dotBrush = new SolidBrush(Color.FromArgb(220, 60, 60)))
                            {
                                g.FillEllipse(dotBrush, dotRect);
                            }
                        }

                        if (isToday && !isSelected)
                        {
                            int circleSize = 20;
                            Rectangle circleRect = new Rectangle(cellRect.Left + 4, cellRect.Top + 4, circleSize, circleSize);
                            using (var pen = new Pen(Color.FromArgb(42, 130, 210), 1.4f))
                            {
                                g.DrawEllipse(pen, circleRect);
                            }
                        }
                    }

                    using (var pen = new Pen(Color.FromArgb(230, 230, 230)))
                    {
                        g.DrawRectangle(pen, cellRect);
                    }
                }
            }
        }

        private void DrawLeftArrow(Graphics g, Rectangle r, Brush b)
        {
            PointF p1 = new PointF(r.Right - 6, r.Top + 4);
            PointF p2 = new PointF(r.Left + 6, r.Top + r.Height / 2f);
            PointF p3 = new PointF(r.Right - 6, r.Bottom - 4);
            g.FillPolygon(b, new[] { p1, p2, p3 });
        }

        private void DrawRightArrow(Graphics g, Rectangle r, Brush b)
        {
            PointF p1 = new PointF(r.Left + 6, r.Top + 4);
            PointF p2 = new PointF(r.Right - 6, r.Top + r.Height / 2f);
            PointF p3 = new PointF(r.Left + 6, r.Bottom - 4);
            g.FillPolygon(b, new[] { p1, p2, p3 });
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (leftArrowRect.Contains(e.Location))
            {
                PrevMonth();
                return;
            }

            if (rightArrowRect.Contains(e.Location))
            {
                NextMonth();
                return;
            }

            int w = ClientSize.Width;
            int gridY = padding + headerHeight + weekdayHeight;
            int cellWidth = Math.Max(24, (w - padding * 2) / cols);
            int cellHeight = Math.Max(24, (ClientSize.Height - gridY - padding) / rows);

            if (e.Y < gridY) return;

            int col = (e.X - padding) / cellWidth;
            int row = (e.Y - gridY) / cellHeight;
            if (col < 0 || col >= cols || row < 0 || row >= rows) return;

            var dtfi = CultureInfo.CurrentCulture.DateTimeFormat;
            int firstDayOfWeek = (int)dtfi.FirstDayOfWeek;
            DateTime firstOfMonth = new DateTime(DisplayDate.Year, DisplayDate.Month, 1);
            int offset = ((int)firstOfMonth.DayOfWeek - firstDayOfWeek + 7) % 7;
            int idx = row * cols + col;
            int dayNumber = idx - offset + 1;
            int days = DateTime.DaysInMonth(DisplayDate.Year, DisplayDate.Month);
            if (dayNumber < 1 || dayNumber > days) return;

            var selected = new DateTime(DisplayDate.Year, DisplayDate.Month, dayNumber);
            SelectedDate = selected;
            Invalidate();

            DateSelected?.Invoke(this, new DateSelectedEventArgs(selected));
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }
    }
}