using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace WowPacketParserGUI;

/// <summary>
/// Owner-drawn progress bar with rounded track and accent fill.
/// </summary>
internal sealed class ModernProgressBar : Control
{
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int Minimum { get; set; } = 0;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int Maximum { get; set; } = 100;

    private int _value;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int Value
    {
        get => _value;
        set { _value = Math.Clamp(value, Minimum, Maximum); Invalidate(); }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color FillColor { get; set; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color TrackColor { get; set; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int CornerRadius { get; set; } = 6;

    public ModernProgressBar()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        DoubleBuffered = true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        using (var parentBrush = new SolidBrush(Parent?.BackColor ?? SystemColors.Control))
            g.FillRectangle(parentBrush, ClientRectangle);

        var bounds = new Rectangle(0, 0, Width - 1, Height - 1);
        using (var trackPath = ModernButton.RoundedRect(bounds, CornerRadius))
        using (var trackBrush = new SolidBrush(TrackColor))
            g.FillPath(trackBrush, trackPath);

        var range = Maximum - Minimum;
        if (range > 0 && _value > Minimum)
        {
            var fillWidth = (int)Math.Round((double)(_value - Minimum) / range * Width);
            fillWidth = Math.Min(fillWidth, Width);
            if (fillWidth > 0)
            {
                var fillBounds = new Rectangle(0, 0, Math.Max(fillWidth - 1, 1), Height - 1);
                using var fillPath = ModernButton.RoundedRect(fillBounds, CornerRadius);
                using var fillBrush = new SolidBrush(FillColor);
                g.FillPath(fillBrush, fillPath);
            }
        }
    }
}
