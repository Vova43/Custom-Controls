using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

public class TrackBar_ : System.Windows.Forms.Control
    {
        public Color Col { get; set; }

        // Scroll zone:
        private int X_SCROLL = 0;
        private int X_MAX_SCROLL = 1;
        private int x_scroll = 0;

        private bool SelectTab_ = false;
        private bool MouseDown_ = false;
        private bool Start_Value_Set_ = true;


        public float Value { get; set; }

        public float Maximum { get; set; }

        public float Minimum { get; set; }

        public int ScrollLeft { get; set; }
        public int ScrollHeight { get; set; }
        public int ScrollIndexMouseWheel { get; set; }
        public int ScrollIndexKeyDown { get; set; }
        public int SliderHeight { get; set; }
        public int SliderWidth { get; set; }
        public Color RectangleScrollColor { get; set; }
        public Color RectanglePanelColor { get; set; }
        public Color RectanglePanelColorTab { get; set; }
        public Color RectangleSliderColor { get; set; }

        public TrackBar_()
        {
            InitializeComponent();
            Size = new Size(80, 20);

            Minimum = 0;
            Maximum = 10;

            ScrollLeft = 4;
            ScrollHeight = 9;
            ScrollIndexMouseWheel = 2;
            ScrollIndexKeyDown = 1;
            SliderHeight = 15;
            SliderWidth = 10;

            RectanglePanelColorTab = Color.Red;
            RectanglePanelColor = Color.Gray;
            RectangleScrollColor = Color.Black;
            RectangleSliderColor = Color.Blue;
        }

        private void InitializeComponent()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor | ControlStyles.UserPaint, true);
            DoubleBuffered = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;

            Rectangle RectanglePanel = new Rectangle(0, 0, Width, Height);
            g.FillRectangle(new SolidBrush(RectanglePanelColor), RectanglePanel);
            if (SelectTab_)
                g.DrawRectangle(new Pen(RectanglePanelColorTab), RectanglePanel);

            Rectangle RectangleScroll = new Rectangle(ScrollLeft, (RectanglePanel.Height - ScrollHeight) / 2, RectanglePanel.Width - (ScrollLeft + ScrollLeft) - 1, ScrollHeight);
            g.FillRectangle(new SolidBrush(RectangleScrollColor), RectangleScroll);

            X_MAX_SCROLL = RectangleScroll.Width - 1;

            if (x_scroll < ScrollLeft - SliderWidth / 2)
                x_scroll = ScrollLeft - SliderWidth / 2;
            if (x_scroll > (RectangleScroll.Width - 1) + ScrollLeft - SliderWidth / 2)
                x_scroll = (RectangleScroll.Width - 1) + ScrollLeft - SliderWidth / 2;

            if (Start_Value_Set_)
            {
                if (Minimum < Maximum)
                {
                    if (Maximum < Value)
                        throw new ArgumentOutOfRangeException("Value: Maximum < Value");
                    else if (Minimum > Value)
                        throw new ArgumentOutOfRangeException("Value: Minimum > Value");
                    SetValueScroll();
                }
                else
                {
                    if (Maximum > Value)
                        throw new ArgumentOutOfRangeException("Value: Maximum > Value");
                    else if (Minimum < Value)
                        throw new ArgumentOutOfRangeException("Value: Minimum < Value");
                    SetValueScroll();
                }

                Start_Value_Set_ = false;
            }

            X_SCROLL = x_scroll - ScrollLeft + SliderWidth / 2;

            Rectangle RectangleSlider = new Rectangle(0, (Size.Height - 1 - SliderHeight) / 2, SliderWidth, SliderHeight);
            RectangleSlider.Location = new Point(x_scroll, RectangleSlider.Y);
            g.FillRectangle(new SolidBrush(RectangleSliderColor), RectangleSlider);
        }

        private void GetValueScroll()
        {
            var diff_X = (Minimum * -1) + Maximum;
            var interval_X = diff_X / X_MAX_SCROLL;
            Value = (Minimum + interval_X * X_SCROLL);
        }

        private void SetValueScroll()
        {
            var diff_X = (Minimum * -1) + Maximum;
            var interval_X = diff_X / X_MAX_SCROLL;
            var X_SCROLL = ((Minimum - Value) / interval_X);
            x_scroll = (int)(X_SCROLL * -1) + ScrollLeft - SliderWidth / 2;
        }

        private void plus(int num)
        {
            if (Maximum < Minimum)
                Value = Math.Min(num + Value, Minimum);
            else
                Value = Math.Min(num + Value, Maximum);
        }
        private void minus(int num)
        {
            if (Maximum < Minimum)
                Value = Math.Max(Value - num, Maximum);
            else
                Value = Math.Max(Value - num, Minimum);
        }


        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
        }

        // Tab Select
        protected override void Select(bool directed, bool forward)
        {
            base.Select(directed, forward);
            SelectTab_ = true;
            Invalidate();
        }
        protected override void OnLeave(EventArgs e)
        {
            base.OnLeave(e);
            SelectTab_ = false;
            Invalidate();
        }

        // Mouse interaction
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            MouseDown_ = false;
            Invalidate();
        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            MouseDown_ = true;
            if (MouseDown_)
            {
                x_scroll = e.X - SliderWidth / 2;
                GetValueScroll();
                Invalidate();
            }
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (MouseDown_)
            {
                x_scroll = e.X - SliderWidth / 2;
                GetValueScroll();
                Invalidate();
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (0 < e.Delta)
            {
                plus(ScrollIndexMouseWheel);
                SetValueScroll();
                Invalidate();
            }
            else
            {
                minus(ScrollIndexMouseWheel);
                SetValueScroll();
                Invalidate();
            }
        }

        // Key Down
        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            switch (e.KeyCode)
            {
                case Keys.Right:
                    plus(ScrollIndexKeyDown);
                    e.IsInputKey = true;
                    SetValueScroll();
                    Invalidate();
                    break;
                case Keys.Left:
                    minus(ScrollIndexKeyDown);
                    SetValueScroll();
                    Invalidate();
                    e.IsInputKey = true;
                    break;
                case Keys.Down:
                    plus(ScrollIndexKeyDown);
                    SetValueScroll();
                    Invalidate();
                    e.IsInputKey = true;
                    break;
                case Keys.Up:
                    minus(ScrollIndexKeyDown);
                    SetValueScroll();
                    Invalidate();
                    e.IsInputKey = true;
                    break;
            }
        }
    }
