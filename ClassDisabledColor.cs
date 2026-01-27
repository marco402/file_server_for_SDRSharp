
using System.Drawing;
using System.Windows.Forms;
namespace SDRSharp.RTLTCP
{
    public interface IDisabledColorControl
    {
        Color DisabledForeColor { get; set; }
    }

    public abstract class ColorableControlBase : Control, IDisabledColorControl
    {
        public Color DisabledForeColor { get; set; } = Color.DarkGray;

        protected Color GetCurrentForeColor()
            => Enabled ? ForeColor : DisabledForeColor;
    }
}




