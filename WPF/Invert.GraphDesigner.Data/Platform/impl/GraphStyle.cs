using System.Globalization;
using System.Windows;
using System.Windows.Media;
using Invert.Core.GraphDesigner;
using Rect = UnityEngine.Rect;

namespace DiagramDesigner.Platform
{
    public class GraphStyle
    {
        private static Typeface _defaultTypeface;
        private Typeface _typeface;
        private Brush _foreground = new SolidColorBrush(Colors.Black);
        private double _fontSize = 12;
        public float shadowSpread = 0f;
        public Typeface DefaultTypeface
        {
            get { return _defaultTypeface ?? (_defaultTypeface = new Typeface(new FontFamily("Arial"), FontStyles.Normal, FontWeight, FontStretches.Normal)); }
        }

        public Brush Background { get; set; }

        public Brush Foreground
        {
            get { return _foreground; }
            set { _foreground = value; }
        }

        public Pen Border { get; set; }
        public CornerRadius CornerRadius { get; set; }
        public DrawingAlignment Alignment { get; set; }
        public Rect Offset { get; set; }
        public Typeface Typeface
        {
            get { return _typeface ?? (_typeface = DefaultTypeface); }
            set { _typeface = value; }
        }

        public FlowDirection FlowDirection
        {
            get
            {
                if (Alignment == DrawingAlignment.BottomRight|| Alignment == DrawingAlignment.MiddleRight ||
                    Alignment == DrawingAlignment.TopRight)
                {
                    return FlowDirection.RightToLeft;
                }
                return FlowDirection.LeftToRight;
            }
        }

        public double FontSize
        {
            get { return _fontSize; }
            set { _fontSize = value; }
        }

        public FontWeight FontWeight { get; set; }

        public FormattedText GetFormattedText(string text)
        {
            var ft = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection, Typeface, FontSize, Foreground);
            return ft;
        }
        public static void DrawShadow(DrawingContext drawingContext, double shadowSize, System.Windows.Rect shadowRect)
        {
            var rect = new System.Windows.Rect(shadowRect.X - 3, shadowRect.Y - 3, shadowRect.Width, shadowRect.Height);
            if (shadowSize <= 0 || rect.Width < shadowSize * 2 || rect.Height < shadowSize * 2)
                return;

            // Get the colors for the shadow
            var shadowColor = System.Windows.Media.Color.FromArgb(128, 0, 0, 0);
            var transparentColor = System.Windows.Media.Color.FromArgb(16, 0, 0, 0);
            // Create a GradientStopCollection from these
            GradientStopCollection gradient = new GradientStopCollection(2);
            gradient.Add(new GradientStop(shadowColor, 0.5));
            gradient.Add(new GradientStop(transparentColor, 1.0));
            // Create the background brush
            var backgroundBrush = new SolidColorBrush(shadowColor);
            // Create the LinearGradientBrushes
            var rightBrush = new LinearGradientBrush(gradient, new Point(0.0, 0.0), new Point(1.0, 0.0));
            var bottomBrush = new LinearGradientBrush(gradient, new Point(0.0, 0.0), new Point(0.0, 1.0));
            // Create the RadialGradientBrushes
            var  bottomRightBrush = new RadialGradientBrush(gradient);
            bottomRightBrush.GradientOrigin = new Point(0.0, 0.0);
            bottomRightBrush.Center = new Point(0.0, 0.0);
            bottomRightBrush.RadiusX = 1.0;
            bottomRightBrush.RadiusY = 1.0;


            var topRightBrush = new RadialGradientBrush(gradient);
            topRightBrush.GradientOrigin = new Point(0.0, 1.0);
            topRightBrush.Center = new Point(0.0, 1.0);
            topRightBrush.RadiusX = 1.0;
            topRightBrush.RadiusY = 1.0;


            var bottomLeftBrush = new RadialGradientBrush(gradient);
            bottomLeftBrush.GradientOrigin = new Point(1.0, 0.0);
            bottomLeftBrush.Center = new Point(1.0, 0.0);
            bottomLeftBrush.RadiusX = 1.0;
            bottomLeftBrush.RadiusY = 1.0;
            //// Draw the background (this may show through rounded corners of the child object)
            //var backgroundRect = new System.Windows.Rect(shadowSize, shadowSize, rect.Width - shadowSize, rect.Height - shadowSize);
            //drawingContext.DrawRectangle(backgroundBrush, null, backgroundRect);
            // Now draw the shadow gradients
            var topRightRect = new System.Windows.Rect(rect.X + rect.Width,rect.Y + shadowSize, shadowSize, shadowSize);
            drawingContext.DrawRectangle(topRightBrush, null, topRightRect);
            var rightRect = new System.Windows.Rect(rect.X + rect.Width, rect.Y + shadowSize * 2, shadowSize, rect.Height - shadowSize * 2);
            drawingContext.DrawRectangle(rightBrush, null, rightRect);
            var bottomRightRect = new System.Windows.Rect(rect.X + rect.Width, rect.Y + rect.Height, shadowSize, shadowSize);
            drawingContext.DrawRectangle(bottomRightBrush, null, bottomRightRect);
            var bottomRect = new System.Windows.Rect(rect.X + shadowSize * 2, rect.Y + rect.Height, rect.Width - shadowSize * 2, shadowSize);
            drawingContext.DrawRectangle(bottomBrush, null, bottomRect);
            var bottomLeftRect = new System.Windows.Rect(rect.X + shadowSize, rect.Y + rect.Height, shadowSize, shadowSize);
            drawingContext.DrawRectangle(bottomLeftBrush, null, bottomLeftRect);
        }

        public void DrawStyle(DrawingContext context, System.Windows.Rect rect, bool strokeBottom = true)
        {
            DrawStyle(context, rect, CornerRadius, strokeBottom);
        }

        public void DrawStyle(DrawingContext context, System.Windows.Rect rect, CornerRadius radius, bool strokeBottom = true)
        {
   
            DrawStyle(context, rect, radius, Offset, strokeBottom);
        }

        public void DrawStyle(DrawingContext context, System.Windows.Rect rect, CornerRadius radius , Rect offset, bool strokeBottom = true)
        {
            var isRoundedCorner = radius.BottomLeft > 0 || radius.BottomRight > 0 ||
                                  radius.TopLeft > 0 || radius.TopRight > 0;
            
           // var finalRect = new System.Windows.Rect(rect.X + offset.x, rect.Y + offset.y, rect.Width + offset.width,rect.Height + offset.height);
            var finalRect = new System.Windows.Rect(rect.X , rect.Y , rect.Width ,rect.Height);
            if (shadowSpread > 0f)
            {
                DrawShadow(context, shadowSpread, finalRect);
            }
            if (isRoundedCorner && (Background != null || Border != null))
            {
                DrawRoundedRectangle(context, Background, Border, finalRect, radius, strokeBottom);
                //context.DrawRectangle(Background, Border, finalRect);
            }
            else if (Background != null || Border != null)
            {
                context.DrawRectangle(Background, Border, finalRect);
            }
        }

        public void DrawStyleWithText(DrawingContext context, System.Windows.Rect rect,string text)
        {
            DrawStyleWithText(context, rect, text, Alignment);
        }

        public void DrawStyleWithText(DrawingContext context, System.Windows.Rect rect,string text ,DrawingAlignment alignment )
        {
            
            DrawStyle(context, rect);
            var finalRect = rect;// new System.Windows.Rect(rect.X + Offset.x, rect.Y + Offset.y, rect.Width + Offset.width,rect.Height + Offset.height);
            var ft = GetFormattedText(text);

            var point = new Point(finalRect.X, finalRect.Y);

            //// Adjust Y Position 
            if (alignment == DrawingAlignment.MiddleCenter || alignment == DrawingAlignment.MiddleLeft ||
                alignment == DrawingAlignment.MiddleRight)
            {
                point.Y += ((finalRect.Height / 2f) - (ft.Height / 2f));
            }
            if (alignment == DrawingAlignment.BottomCenter || alignment == DrawingAlignment.BottomLeft ||
                alignment == DrawingAlignment.BottomRight)
            {
                point.Y += (finalRect.Height) - (ft.Height);
            }
            if (alignment == DrawingAlignment.MiddleCenter || alignment == DrawingAlignment.BottomCenter || alignment == DrawingAlignment.TopCenter)
            {
                point.X += (finalRect.Width / 2f) - (ft.Width / 2f);
            }
            context.DrawText(ft, point);
        }

        public void DrawAsImage(DrawingContext context)
        {
            
        }

        public  void DrawRoundedRectangle(DrawingContext dc, Brush brush, Pen pen, System.Windows.Rect rect, CornerRadius cornerRadius, bool strokeBottom = true)
        {
            var geometry = new StreamGeometry();
            using (var context = geometry.Open())
            {
                bool isStroked = pen != null;
                const bool isSmoothJoin = true;

                context.BeginFigure(rect.TopLeft + new Vector(0, cornerRadius.TopLeft), brush != null, true);
                context.ArcTo(new Point(rect.TopLeft.X + cornerRadius.TopLeft, rect.TopLeft.Y),
                    new Size(cornerRadius.TopLeft, cornerRadius.TopLeft),
                    90, false, SweepDirection.Clockwise, isStroked, isSmoothJoin);
                context.LineTo(rect.TopRight - new Vector(cornerRadius.TopRight, 0), isStroked, isSmoothJoin);
                context.ArcTo(new Point(rect.TopRight.X, rect.TopRight.Y + cornerRadius.TopRight),
                    new Size(cornerRadius.TopRight, cornerRadius.TopRight),
                    90, false, SweepDirection.Clockwise, isStroked, isSmoothJoin);
                context.LineTo(rect.BottomRight - new Vector(0, cornerRadius.BottomRight), isStroked, isSmoothJoin);
                context.ArcTo(new Point(rect.BottomRight.X - cornerRadius.BottomRight, rect.BottomRight.Y),
                    new Size(cornerRadius.BottomRight, cornerRadius.BottomRight),
                    90, false, SweepDirection.Clockwise, isStroked, isSmoothJoin);
                context.LineTo(rect.BottomLeft + new Vector(cornerRadius.BottomLeft, 0), isStroked && strokeBottom, isSmoothJoin);
                context.ArcTo(new Point(rect.BottomLeft.X, rect.BottomLeft.Y - cornerRadius.BottomLeft),
                    new Size(cornerRadius.BottomLeft, cornerRadius.BottomLeft),
                    90, false, SweepDirection.Clockwise, isStroked, isSmoothJoin);

                context.Close();
            }
            dc.DrawGeometry(brush, pen, geometry);

        }
    }
}