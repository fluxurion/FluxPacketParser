using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace WowPacketParserGUI;

/// <summary>
/// Owner-drawn button with rounded corners, hover/pressed/disabled states,
/// and an optional accent (primary action) style.
/// </summary>
internal sealed class ModernButton : Button
{
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int CornerRadius { get; set; } = 7;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool Accent { get; set; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color NormalBackColor { get; set; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color HoverBackColor { get; set; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color PressedBackColor { get; set; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color DisabledBackColor { get; set; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color NormalForeColor { get; set; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color DisabledForeColor { get; set; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color BorderColor { get; set; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color AccentBackColor { get; set; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color AccentHoverBackColor { get; set; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color AccentPressedBackColor { get; set; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color AccentForeColor { get; set; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color AccentBorderColor { get; set; }

    private bool _hovered;
    private bool _pressed;

    public ModernButton()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        DoubleBuffered = true;
        Cursor = Cursors.Hand;
    }

    internal static GraphicsPath RoundedRect(Rectangle bounds, int radius)
    {
        int d = Math.Min(radius * 2, Math.Min(bounds.Width, bounds.Height));
        var path = new GraphicsPath();
        if (d <= 0)
        {
            path.AddRectangle(bounds);
            return path;
        }
        path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
        path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
        path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (Width > 0 && Height > 0)
            Region = new Region(RoundedRect(new Rectangle(0, 0, Width, Height), CornerRadius));
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        _hovered = true;
        Invalidate();
        base.OnMouseEnter(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        _hovered = false;
        _pressed = false;
        Invalidate();
        base.OnMouseLeave(e);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
            _pressed = true;
        Invalidate();
        base.OnMouseDown(e);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        _pressed = false;
        Invalidate();
        base.OnMouseUp(e);
    }

    protected override void OnEnabledChanged(EventArgs e)
    {
        Invalidate();
        base.OnEnabledChanged(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        using (var parentBrush = new SolidBrush(Parent?.BackColor ?? SystemColors.Control))
            g.FillRectangle(parentBrush, ClientRectangle);

        var bounds = new Rectangle(0, 0, Width - 1, Height - 1);

        var back = !Enabled ? DisabledBackColor
            : Accent ? (_pressed ? AccentPressedBackColor : _hovered ? AccentHoverBackColor : AccentBackColor)
            : (_pressed ? PressedBackColor : _hovered ? HoverBackColor : NormalBackColor);
        var fore = !Enabled ? DisabledForeColor : Accent ? AccentForeColor : NormalForeColor;
        var border = Accent && Enabled ? AccentBorderColor : BorderColor;

        using var path = RoundedRect(bounds, CornerRadius);
        using var backBrush = new SolidBrush(back);
        g.FillPath(backBrush, path);

        using var borderPen = new Pen(border);
        g.DrawPath(borderPen, path);

        TextRenderer.DrawText(g, Text, Font, ClientRectangle, fore,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
            TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);
    }
}
