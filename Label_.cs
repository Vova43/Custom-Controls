using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

public class Label_ : System.Windows.Forms.Control
{
    private StringFormat Sf = new StringFormat();
    public Label_()
    {
        InitializeComponent();
        Size = new Size(10, 20);
    }

    private void InitializeComponent()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor | ControlStyles.UserPaint, true);
        DoubleBuffered = true;

        Sf = new StringFormat();

        Sf.Alignment = StringAlignment.Center;
        Sf.LineAlignment = StringAlignment.Center;
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        Graphics g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;

        Rectangle Rectangle_ = new Rectangle(0, 0, Width - 1, Height - 1);
        g.DrawRectangle(new Pen(BackColor), Rectangle_);
        g.FillRectangle(new SolidBrush(BackColor), Rectangle_);

        g.DrawString(Text, Font, new SolidBrush(ForeColor), Rectangle_, Sf);
    }
}