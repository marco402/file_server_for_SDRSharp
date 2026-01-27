

using System;
using System.Drawing;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace SDRSharp.RTLTCP
{
    using System.Drawing.Drawing2D;

    public class ColorComboBoxFakeDisabled : ComboBox
    {
        public bool FakeDisabled { get; private set; }

        public Color DisabledBackColor { get; set; }
        public Color DisabledForeColor { get; set; }

        public int BorderRadius { get; set; } = 6;
        public Color BorderColor { get; set; } = Color.FromArgb(80, 80, 80);

        public ColorComboBoxFakeDisabled()
        {
            DropDownStyle = ComboBoxStyle.DropDownList;
            DrawMode = DrawMode.OwnerDrawFixed;

            DisabledBackColor = Color.FromArgb(60, 60, 60);
            DisabledForeColor = Color.FromArgb(150, 150, 150);
        }

        public void SetFakeEnabled(bool enabled)
        {
            FakeDisabled = !enabled;
            Invalidate();
        }

        private GraphicsPath GetRoundRect(Rectangle r, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Color back = FakeDisabled ? DisabledBackColor : BackColor;
            Color fore = FakeDisabled ? DisabledForeColor : ForeColor;

            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);

            // Fond arrondi
            using (var path = GetRoundRect(rect, BorderRadius))
            using (var b = new SolidBrush(back))
                g.FillPath(b, path);

            // Texte
            Rectangle textRect = new Rectangle(6, 0, Width - 24, Height);
            TextRenderer.DrawText(
                g,
                Text,
                Font,
                textRect,
                fore,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter
            );

            // Bordure arrondie
            using (var path = GetRoundRect(rect, BorderRadius))
            using (var p = new Pen(BorderColor))
                g.DrawPath(p, path);
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index < 0)
                return;

            Color back = FakeDisabled ? DisabledBackColor : BackColor;
            Color fore = FakeDisabled ? DisabledForeColor : ForeColor;

            using (var b = new SolidBrush(back))
                e.Graphics.FillRectangle(b, e.Bounds);

            TextRenderer.DrawText(
                e.Graphics,
                Items[e.Index].ToString(),
                Font,
                e.Bounds,
                fore,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter
            );
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_LBUTTONDOWN = 0x0201;
            const int WM_LBUTTONDBLCLK = 0x0203;
            const int WM_MOUSEWHEEL = 0x020A;

            if (FakeDisabled)
            {
                if (m.Msg == WM_LBUTTONDOWN ||
                    m.Msg == WM_LBUTTONDBLCLK ||
                    m.Msg == WM_MOUSEWHEEL)
                    return;
            }

            base.WndProc(ref m);
        }
    }
}
