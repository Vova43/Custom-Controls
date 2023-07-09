using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApplication23_Paint
{
    public class ZoomPictureBox : UserControl
    {
        private Bitmap image;
        private PointF visibleCenter;
        public float zoom = 1f;
        private MouseState mouseState;
        private Point startDragged;
        private PointF startDraggedVisibleCenter;
        private int sourceImageWidth;
        private int sourceImageHeight;
        public int boardSize = 60;

        //public event EventHandler VisibleCenterChanged;

        [DefaultValue(0.1f)]
        public float ZoomDelta { get; set; }
        [DefaultValue(true)]
        public bool AllowUserDrag { get; set; }
        [DefaultValue(true)]
        public bool AllowUserZoom { get; set; }

        public InterpolationMode InterpolationMode { get; set; }
        public InterpolationMode InterpolationModeZoomOut { get; set; }
        public PixelOffsetMode PixelOffsetMode { get; set; }

        public ZoomPictureBox()
        {

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

            ZoomDelta = 0.1f;
            AllowUserDrag = true;
            AllowUserZoom = true;
            InterpolationMode = InterpolationMode.Bicubic;
            InterpolationModeZoomOut = InterpolationMode.Bilinear;
            PixelOffsetMode = PixelOffsetMode.HighQuality;
        }

        [DefaultValue(null)]
        public Bitmap Image
        {
            get { return image; }
            set
            {
                image = value;

                if (value == null)
                {
                    sourceImageWidth = 0;
                    sourceImageHeight = 0;
                    visibleCenter = new PointF(0, 0);
                }
                else
                {
                    sourceImageWidth = value.Width;
                    sourceImageHeight = value.Height;
                    //visibleCenter = new PointF(sourceImageWidth / 2f, sourceImageHeight / 2f);
                }

                Invalidate();
            }
        }

        public void UpdateImage(Bitmap image)
        {
            this.image = image;

            if (image != null)
            {
                sourceImageWidth = image.Width;
                sourceImageHeight = image.Height;
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
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (AllowUserDrag)
                if (e.Button == MouseButtons.Left)
                {
                    Cursor = Cursors.SizeAll;
                    mouseState = MouseState.Drag;
                }

            startDragged = e.Location;
            startDraggedVisibleCenter = visibleCenter;
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            Cursor = Cursors.Default;
            mouseState = MouseState.None;
            Invalidate();
        }

        public int dx1 = 0;
        public int dy1 = 0;
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (mouseState == MouseState.Drag)
            {
                dx1 = e.Location.X - startDragged.X;
                dy1 = e.Location.Y - startDragged.Y;

                var dx = e.Location.X - startDragged.X;
                var dy = e.Location.Y - startDragged.Y;
                visibleCenter = new PointF(startDraggedVisibleCenter.X - dx / zoom, startDraggedVisibleCenter.Y - dy / zoom);
                Invalidate();
            }
        }

        public void DecreaseZoom()
        {
            zoom = (float)Math.Exp(Math.Log(zoom) - ZoomDelta);
        }

        public void IncreazeZoom()
        {
            zoom = (float)Math.Exp(Math.Log(zoom) + ZoomDelta);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (image == null)
                return;
            if (zoom > 100F)
                zoom = 100F;
            if (zoom < 0)
                zoom = 0F;

            e.Graphics.ResetTransform();
            e.Graphics.InterpolationMode = zoom < 1f ? InterpolationModeZoomOut : InterpolationMode;
            e.Graphics.PixelOffsetMode = PixelOffsetMode;

            //if (mouseState == MouseState.Drag)
            {
                e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            }

            var p = ImagePointToClient(Point.Empty);

            e.Graphics.DrawImage(image, p.X, p.Y, image.Width * zoom, image.Height * zoom);

            Rectangle Rectangle_ = new Rectangle(p.X - 1, p.Y - 1, (int)(image.Width * zoom) + 1, (int)(image.Height * zoom) + 1);
            e.Graphics.DrawRectangle(new Pen(ForeColor), Rectangle_);

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
            var dx = (point.X - ClientSize.Width / 2f) / zoom + visibleCenter.X;
            var dy = (point.Y - ClientSize.Height / 2f) / zoom + visibleCenter.Y;
            return new PointF(dx, dy);
        }

        public PointF ImagePointToClient(PointF point)
        {
            var dx = (point.X - visibleCenter.X) * zoom + ClientSize.Width / 2f;
            var dy = (point.Y - visibleCenter.Y) * zoom + ClientSize.Height / 2f;
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
            None, Drag
        }
    }
}
