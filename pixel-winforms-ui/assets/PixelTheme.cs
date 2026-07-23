using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PixelWinFormsUi {
  public static class PixelTheme {
    const int WM_SETREDRAW = 0x000B;
    [DllImport("user32.dll")]
    static extern IntPtr SendMessage(IntPtr handle, int message, IntPtr wParam, IntPtr lParam);

    public const string FontName = "SimSun";
    public const string MonoFontName = "NSimSun";
    public static readonly Font TextFont = new Font(FontName, 9f, FontStyle.Regular);
    public static readonly Font StrongFont = new Font(FontName, 9f, FontStyle.Bold);
    public static readonly Font TitleFont = new Font(FontName, 10f, FontStyle.Bold);
    public static readonly Font SmallFont = new Font(FontName, 8f, FontStyle.Regular);
    public static readonly Font MonoFont = new Font(MonoFontName, 9f, FontStyle.Regular);
    public static readonly Color Paper = Color.FromArgb(255, 255, 255);
    public static readonly Color Panel = Color.FromArgb(246, 247, 249);
    public static readonly Color Ink = Color.FromArgb(18, 22, 28);
    public static readonly Color Muted = Color.FromArgb(82, 91, 103);
    public static readonly Color Grid = Color.FromArgb(182, 188, 197);
    public static readonly Color Blue = Color.FromArgb(32, 107, 214);
    public static readonly Color Red = Color.FromArgb(226, 45, 59);
    public static readonly Color Yellow = Color.FromArgb(240, 177, 0);
    public static readonly Color Green = Color.FromArgb(16, 157, 88);
    public static readonly Color PaleBlue = Color.FromArgb(226, 239, 255);
    public static readonly Color PaleGreen = Color.FromArgb(220, 249, 232);
    public static readonly Color PaleRed = Color.FromArgb(255, 226, 230);

    public static Label Label(string text, Point location, Size size, bool heading) {
      return new Label { Text = text, AutoSize = false, Location = location, Size = size, BackColor = Color.Transparent, ForeColor = heading ? Ink : Muted, Font = heading ? StrongFont : SmallFont, TextAlign = ContentAlignment.MiddleCenter };
    }

    public static void PaintWindow(Graphics graphics, int width, int height, int dividerX) {
      graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
      graphics.Clear(Paper);
      using (var ink = new SolidBrush(Ink)) {
        graphics.FillRectangle(ink, 0, 0, width, 6);
        graphics.FillRectangle(ink, 0, height - 6, width, 6);
        graphics.FillRectangle(ink, 0, 0, 6, height);
        graphics.FillRectangle(ink, width - 6, 0, 6, height);
        graphics.FillRectangle(ink, 8, 43, width - 16, 4);
        if (dividerX > 0) graphics.FillRectangle(ink, dividerX, 51, 4, height - 65);
      }
      using (var header = new SolidBrush(Panel)) graphics.FillRectangle(header, 6, 6, width - 12, 37);
      using (var grid = new Pen(Grid, 2)) graphics.DrawRectangle(grid, 8, 49, width - 17, height - 58);
      using (var red = new SolidBrush(Red)) graphics.FillRectangle(red, 16, 17, 9, 9);
      using (var yellow = new SolidBrush(Yellow)) graphics.FillRectangle(yellow, 29, 17, 9, 9);
      using (var green = new SolidBrush(Green)) graphics.FillRectangle(green, 42, 17, 9, 9);
    }

    public static void StyleMenu(ContextMenuStrip menu) {
      menu.Renderer = new PixelMenuRenderer();
      menu.ShowImageMargin = false;
      menu.BackColor = Paper;
      menu.ForeColor = Ink;
      menu.DropShadowEnabled = false;
      menu.Font = StrongFont;
      menu.Padding = new Padding(3);
      foreach (ToolStripItem item in menu.Items) {
        item.AutoSize = false;
        item.Size = new Size(136, 29);
        item.BackColor = Paper;
        item.ForeColor = Ink;
        item.Padding = new Padding(8, 3, 8, 3);
        item.Margin = Padding.Empty;
      }
    }

    public static void RenderAtomically(Control target, Action render) {
      if (target == null || render == null) return;
      bool handleCreated = target.IsHandleCreated;
      if (handleCreated) SendMessage(target.Handle, WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);
      target.SuspendLayout();
      try { render(); }
      finally {
        target.ResumeLayout(false);
        if (handleCreated) {
          SendMessage(target.Handle, WM_SETREDRAW, new IntPtr(1), IntPtr.Zero);
          target.Invalidate(true);
          target.Update();
        }
      }
    }
  }

  public sealed class PixelButton : Control {
    bool hover, pressed, active, danger;
    public bool Active { get { return active; } set { active = value; Invalidate(); } }
    public bool Danger { get { return danger; } set { danger = value; Invalidate(); } }

    public PixelButton() {
      SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.Selectable, true);
      Cursor = Cursors.Hand; TabStop = true; Font = PixelTheme.StrongFont; ForeColor = PixelTheme.Ink; BackColor = PixelTheme.Paper;
    }

    protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { hover = false; pressed = false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnMouseDown(MouseEventArgs e) { if (e.Button == MouseButtons.Left) { pressed = true; Focus(); Invalidate(); } base.OnMouseDown(e); }
    protected override void OnMouseUp(MouseEventArgs e) { pressed = false; Invalidate(); base.OnMouseUp(e); }
    protected override void OnKeyDown(KeyEventArgs e) { if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter) { OnClick(EventArgs.Empty); e.Handled = true; } base.OnKeyDown(e); }

    protected override void OnPaint(PaintEventArgs e) {
      Graphics graphics = e.Graphics;
      graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
      Color fill = pressed ? PixelTheme.Grid : active ? PixelTheme.PaleGreen : danger && hover ? PixelTheme.PaleRed : hover ? PixelTheme.PaleBlue : PixelTheme.Paper;
      graphics.Clear(Parent == null ? PixelTheme.Paper : Parent.BackColor);
      using (var shadow = new SolidBrush(PixelTheme.Grid)) graphics.FillRectangle(shadow, 4, 4, Width - 4, Height - 4);
      using (var border = new SolidBrush(PixelTheme.Ink)) graphics.FillRectangle(border, 0, 0, Width - 4, Height - 4);
      using (var body = new SolidBrush(fill)) graphics.FillRectangle(body, 3, 3, Width - 10, Height - 10);
      if (active) using (var mark = new SolidBrush(PixelTheme.Green)) graphics.FillRectangle(mark, 6, 6, 5, Height - 16);
      if (danger && hover) using (var mark = new SolidBrush(PixelTheme.Red)) graphics.FillRectangle(mark, 6, 6, 5, Height - 16);
      TextRenderer.DrawText(graphics, Text, Font, new Rectangle(5, 2, Width - 14, Height - 10), ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
      if (Focused) using (var focus = new Pen(PixelTheme.Blue, 2)) graphics.DrawRectangle(focus, 5, 5, Width - 15, Height - 15);
    }
  }

  public sealed class PixelToggle : Control {
    bool isChecked, hover;
    public bool Checked { get { return isChecked; } set { if (isChecked == value) return; isChecked = value; Invalidate(); if (CheckedChanged != null) CheckedChanged(this, EventArgs.Empty); } }
    public event EventHandler CheckedChanged;

    public PixelToggle() {
      SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.Selectable, true);
      Cursor = Cursors.Hand; TabStop = true; Height = 24; Font = PixelTheme.TextFont; ForeColor = PixelTheme.Ink; BackColor = PixelTheme.Paper;
    }

    protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { hover = false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnClick(EventArgs e) { Checked = !Checked; base.OnClick(e); }
    protected override void OnKeyDown(KeyEventArgs e) { if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter) { Checked = !Checked; e.Handled = true; } base.OnKeyDown(e); }

    protected override void OnPaint(PaintEventArgs e) {
      Graphics graphics = e.Graphics;
      graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
      graphics.Clear(Parent == null ? PixelTheme.Paper : Parent.BackColor);
      using (var shadow = new SolidBrush(PixelTheme.Grid)) graphics.FillRectangle(shadow, 3, 6, 18, 18);
      using (var outer = new SolidBrush(hover ? PixelTheme.Blue : PixelTheme.Ink)) graphics.FillRectangle(outer, 0, 3, 19, 19);
      using (var well = new SolidBrush(PixelTheme.Paper)) graphics.FillRectangle(well, 3, 6, 13, 13);
      if (Checked) using (var on = new SolidBrush(PixelTheme.Green)) {
        graphics.FillRectangle(on, 6, 9, 7, 7);
        graphics.FillRectangle(on, 8, 7, 3, 11);
      }
      TextRenderer.DrawText(graphics, Text, Font, new Rectangle(28, 0, Width - 28, Height), ForeColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
      if (Focused) using (var focus = new Pen(PixelTheme.Blue, 2)) graphics.DrawRectangle(focus, 25, 2, Width - 27, Height - 5);
    }
  }

  public sealed class PixelProgressBar : Control {
    int value;
    public int Value { get { return value; } set { int next = Math.Max(0, Math.Min(100, value)); if (next == this.value) return; this.value = next; Invalidate(); } }

    public PixelProgressBar() {
      SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
      BackColor = PixelTheme.Paper; AccessibleName = "进度";
    }

    protected override void OnPaint(PaintEventArgs e) {
      Graphics graphics = e.Graphics;
      graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
      graphics.Clear(PixelTheme.Paper);
      using (var ink = new SolidBrush(PixelTheme.Ink)) {
        graphics.FillRectangle(ink, 0, 0, Width, 4);
        graphics.FillRectangle(ink, 0, Height - 4, Width, 4);
        graphics.FillRectangle(ink, 0, 0, 4, Height);
        graphics.FillRectangle(ink, Width - 4, 0, 4, Height);
      }
      int segments = 10, gap = 3, innerX = 7, innerY = 7, innerWidth = Width - 14, innerHeight = Height - 14;
      int segmentWidth = Math.Max(1, (innerWidth - gap * (segments - 1)) / segments);
      int filled = value == 0 ? 0 : Math.Min(segments, (value + 9) / 10);
      for (int index = 0; index < segments; index++) {
        Rectangle block = new Rectangle(innerX + index * (segmentWidth + gap), innerY, segmentWidth, innerHeight);
        using (var brush = new SolidBrush(index < filled ? PixelTheme.Green : PixelTheme.Panel)) graphics.FillRectangle(brush, block);
      }
    }
  }

  public sealed class PixelMenuRenderer : ToolStripProfessionalRenderer {
    protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e) { e.Graphics.Clear(PixelTheme.Paper); }
    protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e) { using (var pen = new Pen(PixelTheme.Ink, 3)) e.Graphics.DrawRectangle(pen, 1, 1, e.ToolStrip.Width - 3, e.ToolStrip.Height - 3); }
    protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e) {
      Rectangle area = new Rectangle(2, 1, e.Item.Width - 4, e.Item.Height - 2);
      using (var fill = new SolidBrush(e.Item.Selected ? PixelTheme.PaleGreen : PixelTheme.Paper)) e.Graphics.FillRectangle(fill, area);
      if (e.Item.Selected) using (var pen = new Pen(PixelTheme.Ink, 2)) e.Graphics.DrawRectangle(pen, area.X, area.Y, area.Width - 1, area.Height - 1);
    }
    protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e) { e.TextColor = PixelTheme.Ink; base.OnRenderItemText(e); }
    protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e) { using (var pen = new Pen(PixelTheme.Ink, 2)) e.Graphics.DrawLine(pen, 7, e.Item.Height / 2, e.Item.Width - 8, e.Item.Height / 2); }
  }
}
