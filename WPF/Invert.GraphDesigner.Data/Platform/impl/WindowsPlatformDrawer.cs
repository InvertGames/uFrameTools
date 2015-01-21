using Invert.Core.GraphDesigner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using UnityEngine;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Color = UnityEngine.Color;
using FontFamily = System.Windows.Media.FontFamily;
using Pen = System.Windows.Media.Pen;
using Point = System.Windows.Point;
using Rect = UnityEngine.Rect;
using Size = System.Windows.Size;

namespace DiagramDesigner.Platform
{
    public class WindowsPlatformDrawer : IPlatformDrawer
    {
        public DesignerStyles Styles = new DesignerStyles();

        private Dictionary<string, Control> _controls;

        private List<string> _ControlsLeftOver = new List<string>();

        private Brush _defaultBrush;

        private Brush _nodeBrush;

        private Typeface _typeFace;

        public Canvas Canvas { get; set; }

        public DrawingContext Context { get; set; }

        public Dictionary<string, Control> Controls
        {
            get { return _controls ?? (_controls = new Dictionary<string, Control>()); }
            set { _controls = value; }
        }

        public Brush DefaultBrush
        {
            get { return _defaultBrush ?? (_defaultBrush = Brushes.White); }
        }

        public MouseEvent Event { get; set; }

        public Brush NodeBrush
        {
            get { return _nodeBrush ?? (_nodeBrush = Brushes.LightBlue); }
        }

        public Typeface Type
        {
            get { return _typeFace ?? (_typeFace = new Typeface(new FontFamily("Verdana"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal)); }
        }

        public static void DrawRoundedHighlight(DrawingContext dc, Brush brush, Pen pen, System.Windows.Rect rect, CornerRadius cornerRadius)
        {
            var geometry = new StreamGeometry();
            using (var context = geometry.Open())
            {
                bool isStroked = pen != null;
                const bool isSmoothJoin = true;

                context.BeginFigure(rect.TopLeft + new Vector(0, cornerRadius.TopLeft), brush != null, false);
                context.ArcTo(new Point(rect.TopLeft.X + cornerRadius.TopLeft, rect.TopLeft.Y),
                    new Size(cornerRadius.TopLeft, cornerRadius.TopLeft),
                    90, false, SweepDirection.Clockwise, isStroked, isSmoothJoin);
                context.LineTo(rect.TopRight - new Vector(cornerRadius.TopRight, 0), isStroked, isSmoothJoin);
                context.ArcTo(new Point(rect.TopRight.X, rect.TopRight.Y + cornerRadius.TopRight),
                    new Size(cornerRadius.TopRight, cornerRadius.TopRight),
                    90, false, SweepDirection.Clockwise, isStroked, isSmoothJoin);
                context.Close();
                //context.LineTo(rect.BottomRight - new Vector(0, cornerRadius.BottomRight), isStroked, isSmoothJoin);
                //context.ArcTo(new Point(rect.BottomRight.X - cornerRadius.BottomRight, rect.BottomRight.Y),
                //    new Size(cornerRadius.BottomRight, cornerRadius.BottomRight),
                //    90, false, SweepDirection.Clockwise, isStroked, isSmoothJoin);
                //context.LineTo(rect.BottomLeft + new Vector(cornerRadius.BottomLeft, 0), isStroked, isSmoothJoin);
                //context.ArcTo(new Point(rect.BottomLeft.X, rect.BottomLeft.Y - cornerRadius.BottomLeft),
                //    new Size(cornerRadius.BottomLeft, cornerRadius.BottomLeft),
                //    90, false, SweepDirection.Clockwise, isStroked, isSmoothJoin);

                //context.Close();
            }
            dc.DrawGeometry(brush, pen, geometry);
        }

        public void BeginRender(object sender, MouseEvent mouseEvent)
        {
          
            Event = mouseEvent;
            Canvas = sender as Canvas;
            _ControlsLeftOver.AddRange(Controls.Select(p => p.Key));
        }

        public Vector2 CalculateSize(string text, object tag1)
        {
            FormattedText t = ((GraphStyle)tag1).GetFormattedText(text);

            return new Vector2(Convert.ToSingle(t.Width), Convert.ToSingle(t.Height));
        }

        public void DoButton(Rect scale, string label, object style, Action action)
        {
            ((GraphStyle)style).DrawStyle(Context, new System.Windows.Rect(scale.x, scale.y, scale.width, scale.height));
            if (Event != null && Event.IsMouseDown && scale.Contains(Event.MouseDownPosition) && scale.Contains(Event.MousePosition))
            {
                action();
                
            }
        }

        public void DrawBezier(Vector3 startPosition, Vector3 endPosition, Vector3 startTangent, Vector3 endTangent, Color color,
            float width)
        {
            var geometry = new PathGeometry();
            var figure = new PathFigure();
            figure.StartPoint = new Point(startPosition.x, startPosition.y);
            figure.Segments.Add(new BezierSegment(new Point(startTangent.x, startTangent.y), new Point(endTangent.x, endTangent.y), new Point(endPosition.x, endPosition.y), true));

            geometry.Figures.Add(figure);
            Context.DrawGeometry(null, new Pen(new SolidColorBrush(System.Windows.Media.Color.FromScRgb(color.a, color.r,color.g,color.b)), 2f), geometry);
            //Context.DrawLine(new Pen(Brushes.White,2), new Point(startPosition.x, startPosition.y), new Point(endPosition.x, endPosition.y));
        }

        public void DrawColumns(Rect rect, int[] columnWidths, params Action<Rect>[] columns)
        {
            var x = 0;
            for (int index = 0; index < columns.Length; index++)
            {
                var item = columns[index];
                if (index == columns.Length - 1)
                {
                    // Use the remaining width of this item
                    var width = rect.width - x;
                    var newRect = new Rect(rect.x + x, rect.y, width, rect.height);
                    item(newRect);
                }
                else
                {
                    var newRect = new Rect(rect.x + x, rect.y, columnWidths[index], rect.height);
                    item(newRect);

                }

                x += columnWidths[index];
            }
        }

        public void DrawColumns(Rect rect, params Action<Rect>[] columns)
        {
            var columnsLength = columns.Length;
            var columnFactor = rect.width / columnsLength;

            for (int index = 0; index < columns.Length; index++)
            {
                var item = columns[index];
                var newRect = new Rect(rect.x + (index * columnFactor), rect.y, columnFactor, rect.height);
                item(newRect);
            }
        }

        public void DrawImage(Rect bounds, string texture, bool b)
        {
            Context.DrawImage(Styles.GetImageInternal(texture), new System.Windows.Rect(bounds.x, bounds.y, bounds.width, bounds.height));
        }

        public void DrawLabel(Rect rect, string label, object style, DrawingAlignment alignment = DrawingAlignment.MiddleLeft)
        {
            ((GraphStyle)style).DrawStyleWithText(Context, new System.Windows.Rect(rect.x, rect.y, rect.width, rect.height), label, alignment);
        }

        public void DrawPolyLine(Vector2[] lines, Color color)
        {
        }

        public void DrawPropertyField(PropertyFieldDrawer propertyFieldDrawer, float scale)
        {
        }

        public void DrawStretchBox(Rect scale, object style, float offset)
        {
            ((GraphStyle)style)
                .DrawStyle(Context, new System.Windows.Rect(scale.x, scale.y, scale.width, scale.height));
        }

        public void DrawStretchBox(Rect scale, object style, Rect offset)
        {
            ((GraphStyle)style).DrawStyle(Context, new System.Windows.Rect(scale.x, scale.y, scale.width, scale.height),
                new CornerRadius(10f, 10f, 0f, 0f), new Rect(9, -1, -19, 0), false);
        }

        public void DrawTextbox(string id, Rect bounds, string value, object itemTextEditingStyle, Action<string, bool> valueChangedAction)
        {
            var textbox = EnsureControl<TextBox>(id, bounds, _ =>
            {
                var style = ((GraphStyle)itemTextEditingStyle);
                if (style != null)
                {
                    _.Background = style.Background;
                    _.BorderBrush = null;
                    
                    //if (style.Border != null)
                    //    _.BorderBrush = style.Border.Brush;
                    if (style.Foreground != null)
                        _.Foreground = style.Foreground;
                }
               _.TextAlignment = TextAlignment.Center;
               // _.Height = style.FontSize + 4;
                _.Text = value;
                _.SelectAll();
                _.Focus();
                _.TextChanged += (sender, args) =>
                {
                    valueChangedAction(_.Text, false);
                };
                _.KeyDown += (sender, args) =>
                {
                    if (args.Key == Key.Enter)
                    {
                        valueChangedAction(_.Text, true);
                        Canvas.InvalidateVisual();
                    }
                };
            });
            textbox.Text = value;
            //controlCount++;
        }

        public void DrawWarning(Rect rect, string key)
        {
            DrawLabel(rect,key,CachedStyles.NodeHeader13,DrawingAlignment.MiddleCenter);
        }

        public void EndRender()
        {
            for (int index = 0; index < _ControlsLeftOver.Count; index++)
            {
                var item = _ControlsLeftOver[index];
                this.Canvas.Children.Remove(Controls[item]);
                Controls.Remove(item);
            }
            _ControlsLeftOver.Clear();
        }

        public TControl EnsureControl<TControl>(string id, Rect rect, Action<TControl> init = null) where TControl : Control, new()
        {
            TControl control;
            if (Controls.ContainsKey(id))
            {
                control = (TControl)Controls[id];
            }
            else
            {
                control = new TControl();
                control.Name = "Control" + _controls.Count;
                this.Canvas.Children.Add(control);
                Controls.Add(id, control);
                if (init != null)
                {
                    init(control);
                }
            }
            _ControlsLeftOver.Remove(id);
            control.Width = rect.width;
            control.Height = rect.height;
            Canvas.SetLeft(control, rect.x);
            Canvas.SetTop(control, rect.y);
            return control;
        }

        public System.Windows.Media.Color GetColor(string hex)
        {
            return (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(hex);
        }
    }
}