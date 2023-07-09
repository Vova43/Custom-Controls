using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;

public class PaintPictureBox : UserControl
{
    private Bitmap ImageBitMap;
    private PointF visibleCenter;
    public float ZoomImage = 1f;
    private MouseState MouseStateControl;
    private Point StartDragged;
    private PointF StartDraggedVisibleCenter;
    private int SourceImageWidth;
    private int SourceImageHeight;
    //public int boardSize = 60;

    //public event EventHandler VisibleCenterChanged;

    public Color BorderColor { get; set; }

    [DefaultValue(0.1f)]
    public float ZoomDelta { get; set; }
    [DefaultValue(true)]
    public bool AllowUserDrag { get; set; }
    [DefaultValue(true)]
    public bool AllowUserZoom { get; set; }

    public InterpolationMode InterpolationMode { get; set; }
    public InterpolationMode InterpolationModeZoomOut { get; set; }
    public PixelOffsetMode PixelOffsetMode { get; set; }

    public PaintPictureBox()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

        ZoomDelta = 0.1f;
        AllowUserDrag = true;
        AllowUserZoom = true;
        InterpolationMode = InterpolationMode.Bicubic;
        InterpolationModeZoomOut = InterpolationMode.Bilinear;
        PixelOffsetMode = PixelOffsetMode.HighQuality;

        BorderColor = Color.White;
    }

    [DefaultValue(null)]
    public Bitmap Image
    {
        get { return ImageBitMap; }
        set
        {
            ImageBitMap = value;

            if (value == null)
            {
                SourceImageWidth = 0;
                SourceImageHeight = 0;
                visibleCenter = new PointF(0, 0);
            }
            else
            {
                SourceImageWidth = value.Width;
                SourceImageHeight = value.Height;
                visibleCenter = new PointF(SourceImageWidth / 2F, SourceImageHeight / 2F);
                //ZoomImage = ((SourceImageWidth) / Width) + ((SourceImageHeight) / Height);
            }

            Invalidate();
        }
    }

    public void UpdateImage(Bitmap ImageBitMap)
    {
        this.ImageBitMap = ImageBitMap;

        if (ImageBitMap != null)
        {
            SourceImageWidth = ImageBitMap.Width;
            SourceImageHeight = ImageBitMap.Height;
        }

        Invalidate();
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);
        if (!AllowUserZoom)
            return;

        if (e.Delta > 0)
            IncreazeZoom();

        if (e.Delta < 0)
            DecreaseZoom();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);

        if (AllowUserDrag)
            if (e.Button == MouseButtons.Right)
            {
                Cursor = Cursors.SizeAll;
                MouseStateControl = MouseState.Drag;
            }
        if (e.Button == MouseButtons.Left)
        {
            MouseStateControl = MouseState.Paint;
            int dx = (int)((e.X - ClientSize.Width / 2F) / ZoomImage + visibleCenter.X);
            int dy = (int)((e.Y - ClientSize.Height / 2F) / ZoomImage + visibleCenter.Y);
            if (dx < 0)
                return;
            if (dx > ImageBitMap.Width - 1)
                return;
            if (dy < 0)
                return;
            if (dy > ImageBitMap.Height - 1)
                return;
            ImageBitMap.SetPixel(dx, dy, ForeColor);
            Invalidate();
        }

        StartDragged = e.Location;
        StartDraggedVisibleCenter = visibleCenter;
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        Cursor = Cursors.Default;
        MouseStateControl = MouseState.None;
        Invalidate();
    }

    public int dx1 = 0;
    public int dy1 = 0;
    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (MouseStateControl == MouseState.Drag)
        {
            dx1 = e.Location.X - StartDragged.X;
            dy1 = e.Location.Y - StartDragged.Y;

            var dx = e.Location.X - StartDragged.X;
            var dy = e.Location.Y - StartDragged.Y;
            visibleCenter = new PointF(StartDraggedVisibleCenter.X - dx / ZoomImage, StartDraggedVisibleCenter.Y - dy / ZoomImage);
            Invalidate();
        }

        if (MouseStateControl == MouseState.Paint)
        {
            int dx = (int)((e.X - ClientSize.Width / 2F) / ZoomImage + visibleCenter.X);
            int dy = (int)((e.Y - ClientSize.Height / 2F) / ZoomImage + visibleCenter.Y);
            if (dx < 0)
                return;
            if (dx > ImageBitMap.Width - 1)
                return;
            if (dy < 0)
                return;
            if (dy > ImageBitMap.Height - 1)
                return;
            ImageBitMap.SetPixel(dx, dy, ForeColor);
            Invalidate();
        }
    }

    public void DecreaseZoom()
    {
        ZoomImage = (float)Math.Exp(Math.Log(ZoomImage) - ZoomDelta);
        Invalidate();
    }

    public void IncreazeZoom()
    {
        ZoomImage = (float)Math.Exp(Math.Log(ZoomImage) + ZoomDelta);
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        if (ImageBitMap == null)
            return;
        if (ZoomImage > 100F)
            ZoomImage = 100F;
        if (ZoomImage < 0)
            ZoomImage = 0F;

        e.Graphics.ResetTransform();
        e.Graphics.InterpolationMode = ZoomImage < 1f ? InterpolationModeZoomOut : InterpolationMode;
        e.Graphics.PixelOffsetMode = PixelOffsetMode;

        //if (MouseStateControl == MouseState.Drag)
        {
            e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
        }

        var dx = (0 - visibleCenter.X) * ZoomImage + ClientSize.Width / 2F;
        var dy = (0 - visibleCenter.Y) * ZoomImage + ClientSize.Height / 2F;

        e.Graphics.DrawImage(ImageBitMap, dx, dy, ImageBitMap.Width * ZoomImage, ImageBitMap.Height * ZoomImage);

        Pen pen = new Pen(BorderColor);
        e.Graphics.DrawRectangle(pen, dx, dy, (ImageBitMap.Width * ZoomImage), (ImageBitMap.Height * ZoomImage));
        pen.Dispose();

        base.OnPaint(e);
    }

    public Point ClientToImagePoint(Point point)
    {
        return Point.Round(ClientToImagePoint((PointF)point));
    }

    public Point ImagePointToClient(Point point)
    {
        return Point.Round(ImagePointToClient((PointF)point));
    }

    public PointF ClientToImagePoint(PointF point)
    {
        var dx = (point.X - ClientSize.Width / 2F) / ZoomImage + visibleCenter.X;
        var dy = (point.Y - ClientSize.Height / 2F) / ZoomImage + visibleCenter.Y;
        return new PointF(dx, dy);
    }

    public PointF ImagePointToClient(PointF point)
    {
        var dx = (point.X - visibleCenter.X) * ZoomImage + ClientSize.Width / 2F;
        var dy = (point.Y - visibleCenter.Y) * ZoomImage + ClientSize.Height / 2F;
        return new PointF(dx, dy);
    }

    public Image GetScreenshot()
    {
        Image img = new Bitmap(ClientSize.Width, ClientSize.Height);
        using (var gr = Graphics.FromImage(img))
            OnPaint(new PaintEventArgs(gr, ClientRectangle));
        return img;
    }

    private enum MouseState
    {
        None, Drag, Paint
    }
}
