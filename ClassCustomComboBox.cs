

using System;
using System.Drawing;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace SDRSharp.RTLTCP
{
    public class ColorComboBoxFakeDisabled : ComboBox
    {
        public bool FakeDisabled { get; private set; }

        public Color DisabledBackColor { get; set; }
        public Color DisabledForeColor { get; set; }

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

        protected override void OnPaint(PaintEventArgs e)
        {
            // Fond
            Color back = FakeDisabled ? DisabledBackColor : BackColor;
            using (var b = new SolidBrush(back))
                e.Graphics.FillRectangle(b, ClientRectangle);

            Color fore = FakeDisabled ? DisabledForeColor : ForeColor;
            Rectangle rect = new Rectangle(3, 0, Width - 20, Height);

            TextRenderer.DrawText(
                e.Graphics,
                Text,
                Font,
                rect,
                fore,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter
            );

            // Bordure
            using (var p = new Pen(Color.FromArgb(80, 80, 80)))
                e.Graphics.DrawRectangle(p, 0, 0, Width - 1, Height - 1);
        }

        protected override void OnDropDown(EventArgs e)
        {
            if (FakeDisabled)
                return;
            base.OnDropDown(e);
        }

        protected override void OnSelectionChangeCommitted(EventArgs e)
        {
            if (FakeDisabled)
                return;
            base.OnSelectionChangeCommitted(e);
        }

        protected override void OnEnter(EventArgs e)
        {
            if (FakeDisabled)
                return;
            base.OnEnter(e);
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
