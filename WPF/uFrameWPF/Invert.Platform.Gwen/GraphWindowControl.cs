using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Gwen;
using Gwen.Control;
using Gwen.Skin.Texturing;
using Invert.Core;
using Invert.Core.GraphDesigner;
using Invert.Json;
using UnityEngine;
using Color = UnityEngine.Color;

namespace Invert.Platform.Gwen
{
    public class GraphWindowControl : Base, IPlatformDrawer, IJsonTypeResolver, IDesignerWindowEvents, IImmediateControlsDrawer<Base>
    {
        //private MouseEvent _event;
        //

        public MouseEvent MouseEvent
        {
            get { return DesignerWindow.MouseEvent; }
            set { DesignerWindow.MouseEvent = value; }
        }

        private DesignerWindow _designerWindow;
        public DesignerWindow DesignerWindow
        {
            get
            {
                if (_designerWindow == null)
                {
                    _designerWindow = new DesignerWindow();
                    _designerWindow.ParentHandler = this;
                    //InvertApplication.ListenFor<IDesignerWindowEvents>(this);
                    //InvertApplication.Plugins.OfType<DesignerWindow>().FirstOrDefault();
                    ////if (_designerWindow != null)
                    //_designerWindow.Watch(this);
                }
                return _designerWindow;
            }
            set { _designerWindow = value; }
        }


        public GraphWindowControl(Base parent = null)
            : base(parent)
        {
            this.Dock = Pos.Fill;
            JsonExtensions.TypeResolver = this;
            InvertGraphEditor.PlatformDrawer = this;
            var styleProvider = InvertGraphEditor.StyleProvider as GwenStyleProvider;
            styleProvider.skin = Skin;
            styleProvider._Texture = new Texture(Skin.Renderer);
            styleProvider._Texture.Load("DiagramSkin.png");

        }

        protected override void OnMouseMoved(int x, int y, int dx, int dy)
        {
            base.OnMouseMoved(x, y, dx, dy);

            MouseEvent.MousePosition = new Vector2(x, y);
            MouseEvent.MousePositionDelta = new Vector2(dx, dy);
            MouseEvent.MousePositionDeltaSnapped = MouseEvent.MousePosition.Snap(DiagramViewModel.SnapSize * InvertGraphEditor.DesignerWindow.Scale) - MouseEvent.LastMousePosition.Snap(DiagramViewModel.SnapSize * InvertGraphEditor.DesignerWindow.Scale);
            if (DiagramDrawer != null)
            {
                foreach (var child in DiagramDrawer.Children.OfType<ConnectorDrawer>())
                {

                    child.Refresh(InvertGraphEditor.PlatformDrawer, Vector2.zero);

                }

                MouseEvent.CurrentHandler.OnMouseMove(MouseEvent);
            }
            
            MouseEvent.LastMousePosition = MouseEvent.MousePosition;
        }

        protected override void OnMouseClickedRight(int x, int y, bool down)
        {
            base.OnMouseClickedRight(x, y, down);
            if (!down)
            {
                MouseEvent.IsMouseDown = false;
                MouseEvent.MouseButton = 1;
                MouseEvent.MouseUpPosition = new Vector2(x, y);
                if (DiagramDrawer != null)
                {
                    MouseEvent.CurrentHandler.OnMouseUp(MouseEvent);
                }
            }
            else
            {
                MouseEvent.IsMouseDown = true;
                MouseEvent.MouseButton = 1;
                MouseEvent.MouseUpPosition = new Vector2(x, y);
                //if (DiagramDrawer != null)
                //{
                //    MouseEvent.CurrentHandler.OnMouseDown(MouseEvent);
                //}


                MouseEvent.MouseUpPosition = new Vector2(x, y);
                MouseEvent.MouseDownPosition = new Vector2(x, y);
                MouseEvent.MousePosition = new Vector2(x, y);
                MouseEvent.CurrentHandler.OnRightClick(MouseEvent);

            }


        }

        protected override void OnMouseDoubleClickedLeft(int x, int y)
        {
            base.OnMouseDoubleClickedLeft(x, y);
            MouseEvent.IsMouseDown = true;
            MouseEvent.MouseButton = 0;
            MouseEvent.MouseDownPosition = new Vector2(x, y);
            MouseEvent.CurrentHandler.OnMouseDoubleClick(MouseEvent);
        }

        protected override void OnMouseClickedLeft(int x, int y, bool down)
        {
            base.OnMouseClickedLeft(x, y, down);
            if (!down)
            {
                MouseEvent.IsMouseDown = false;
                MouseEvent.MouseButton = 0;
                MouseEvent.MouseUpPosition = new Vector2(x, y);
                if (DiagramDrawer != null)
                {
                    MouseEvent.CurrentHandler.OnMouseUp(MouseEvent);
                }
            }
            else
            {
                MouseEvent.IsMouseDown = true;
                MouseEvent.MouseButton = 0;
                MouseEvent.MouseDownPosition = new Vector2(x, y);
                if (DiagramDrawer != null)
                {
                    MouseEvent.CurrentHandler.OnMouseDown(MouseEvent);
                }

            }
            if (DiagramDrawer != null)
                if (DiagramDrawer.DrawersAtMouse.Length < 1)
                {
                    WindowsContextMenu.ContextMenu.Close();
                }


        }

        public Vector2 ScrollPosition
        {
            get { return new Vector2(-ScrollContainer.HorizontalScroll, -ScrollContainer.VerticalScroll); }
        }
        protected override void OnMouseDoubleClickedRight(int x, int y)
        {
            base.OnMouseDoubleClickedRight(x, y);

        }

        protected override void Render(global::Gwen.Skin.Base skin)
        {
            base.Render(skin);
            skin.Renderer.DrawColor = System.Drawing.Color.FromArgb(255, 45, 45, 45);
            skin.Renderer.DrawFilledRect(new Rectangle(0, 0, this.Width, this.Height));
            InvertGraphEditor.PlatformDrawer.BeginRender(null, DesignerWindow.MouseEvent);
            DesignerWindow.DrawToolbar = false;
            DesignerWindow.Draw(this, ScrollContainer.Width, ScrollContainer.Height, ScrollPosition, 1f);
            //DrawLabel(new Rect(400f,400f,100f,100f),skin.Renderer.ClipRegion.Width + ": " + skin.Renderer.ClipRegion.X,CachedStyles.ClearItemStyle );
            InvertGraphEditor.PlatformDrawer.EndRender();
            //
            ////

            //var y = 0;
            //var alternate = 0;
            //for (var i = 0; y < this.Height; i++, y += 10)
            //{
            //    if (alternate == 5)
            //    {
            //        skin.Renderer.DrawColor = System.Drawing.Color.FromArgb(255, 40, 40, 40);
            //        skin.Renderer.DrawFilledRect(new Rectangle(0,y, this.Width,1));

            //        alternate = 0;
            //    }
            //    else
            //    {
            //        skin.Renderer.DrawColor = System.Drawing.Color.FromArgb(255, 35, 35, 35);
            //        //skin.Renderer.DrawLine(0, y, this.Width, y);
            //        skin.Renderer.DrawFilledRect(new Rectangle(0, y, this.Width, 1));
            //        alternate++;
            //    }

            //}
            //var x = 0;
            //alternate = 0;
            //for (var i = 0; x < this.Width; i++, x += 10)
            //{
            //    if (alternate == 5)
            //    {
            //        skin.Renderer.DrawColor = System.Drawing.Color.FromArgb(255, 40, 40, 40);

            //        skin.Renderer.DrawFilledRect(new Rectangle(x, 0, 1, this.Height));

            //        alternate = 0;
            //    }
            //    else
            //    {
            //        skin.Renderer.DrawColor = System.Drawing.Color.FromArgb(255, 35, 35, 35);
            //        skin.Renderer.DrawFilledRect(new Rectangle(x, 0, 1, this.Height));

            //        alternate++;
            //    }


            //}
            //DiagramDrawer.Draw(this, 1f);
        }

        public void BeginRender(object sender, MouseEvent mouseEvent)
        {
            this.BeginImmediate();
        }

        public Vector2 CalculateSize(string text, object tag1)
        {
            var result = Skin.Renderer.MeasureText(Skin.DefaultFont, text);
            return new Vector2(result.X, result.Y);
        }

        public void DoButton(Rect scale, string label, object style, Action action, Action rightClick = null)
        {
            DrawStretchBox(scale, style, 0f);

            var button = this.EnsureControl<global::Gwen.Control.Base, global::Gwen.Control.Button>(label, scale, () =>
            {
                var t = new global::Gwen.Control.Button(this);
                t.SetText(label, false);
                t.Clicked += delegate(Base sender, ClickedEventArgs arguments)
                {
                    action();
                };
                t.ShouldDrawBackground = false;
                return t;
            });
            button.Text = label;
        }



        public void DrawBezier(Vector3 startPosition, Vector3 endPosition, Vector3 startTangent, Vector3 endTangent, Color color,
            float width)
        {
            var renderer = Skin.Renderer as global::Gwen.Renderer.OpenTK;
            if (renderer != null)
            {
                Skin.Renderer.DrawColor = System.Drawing.Color.FromArgb(Mathf.RoundToInt(color.a * 255),
                    Mathf.RoundToInt(color.r * 255), Mathf.RoundToInt(color.g * 255), Mathf.RoundToInt(color.b * 255));
                renderer.BezierCurveTo(ToPoint(startPosition), ToPoint(startTangent), ToPoint(endTangent),
                    ToPoint(endPosition));

            }
            else
            {
                Skin.Renderer.DrawLine(Mathf.RoundToInt(startPosition.x), Mathf.RoundToInt(startPosition.y), Mathf.RoundToInt(endPosition.x), Mathf.RoundToInt(endPosition.y));
            }

        }

        private static Point ToPoint(Vector3 startPosition)
        {
            return new Point(Mathf.RoundToInt(startPosition.x), Mathf.RoundToInt(startPosition.y));
        }

        public void DrawColumns(Rect rect, float[] columnWidths, params Action<Rect>[] columns)
        {

        }

        public void DrawColumns(Rect rect, params Action<Rect>[] columns)
        {

        }

        public void DrawImage(Rect bounds, string texture, bool b)
        {
            var styles = InvertGraphEditor.StyleProvider as GwenStyleProvider;
            var image = styles.GetBorderedImage(texture);
            if (image.MTexture != null)
            {
                image.Draw(Skin.Renderer, ToRectangle(bounds));
            }
        }

        public void DrawLabel(Rect rect, string label, object style, DrawingAlignment alignment = DrawingAlignment.MiddleLeft)
        {
            //Skin.DrawTextBox(this);
            // Skin.Renderer.DrawColor = System.Drawing.Color.White;
            //Skin.Renderer.DrawLinedRect(ToRectangle(rect));
            var s = style as GwenStyle;
            if (style != null)
            {
                s.Bordered.Draw(Skin.Renderer, ToRectangle(rect));
            }
            var finalRect = rect;// new System.Windows.Rect(rect.X + Offset.x, rect.Y + Offset.y, rect.Width + Offset.width,rect.Height + Offset.height);
            var size = CalculateSize(label, style);
            //Skin.Renderer.DrawLinedRect(new Rectangle(Mathf.RoundToInt(rect.x),Mathf.RoundToInt(rect.y),Mathf.RoundToInt(size.x), Mathf.RoundToInt(size.y)));
            var point = new Vector2(finalRect.x, finalRect.y);

            if (alignment == DrawingAlignment.MiddleCenter || alignment == DrawingAlignment.MiddleLeft || alignment == DrawingAlignment.MiddleRight)
            {
                point.y += ((rect.height / 2f) - (size.y / 2f));
            }
            if (alignment == DrawingAlignment.BottomCenter || alignment == DrawingAlignment.BottomLeft ||
                alignment == DrawingAlignment.BottomRight)
            {
                point.y += (rect.height) - (size.y);
            }
            if (alignment == DrawingAlignment.MiddleCenter || alignment == DrawingAlignment.BottomCenter || alignment == DrawingAlignment.TopCenter)
            {
                point.x += (finalRect.width / 2f) - (size.x / 2f);
            }
            if (s != null)
            {
                Skin.Renderer.DrawColor = s.FontColor;

            }

            Skin.DefaultFont.Size = 10;

            Skin.Renderer.RenderText(s == null ? Skin.DefaultFont : s.Font, new Point(Mathf.RoundToInt(point.x), Mathf.RoundToInt(point.y)), label);
        }

        public void DrawPolyLine(Vector2[] lines, Color color)
        {
            for (int index = 1; index < lines.Length; index++)
            {
                var line = lines[index];
                Skin.Renderer.DrawColor = System.Drawing.Color.FromArgb(
                    Mathf.RoundToInt(color.a * 255),
                    Mathf.RoundToInt(color.r * 255),
                    Mathf.RoundToInt(color.g * 255),
                    Mathf.RoundToInt(color.b * 255)
                    );
                Skin.Renderer.DrawLine(
                    Mathf.RoundToInt(lines[index - 1].x),
                    Mathf.RoundToInt(lines[index - 1].y),
                    Mathf.RoundToInt(line.x),
                    Mathf.RoundToInt(line.y), 2
                    );

            }

        }

        public void DrawPropertyField(PropertyFieldViewModel propertyFieldDrawer, float scale)
        {
            throw new NotImplementedException();
        }

        public void DrawPropertyField(PropertyFieldDrawer propertyFieldDrawer, float scale)
        {

        }

        public void DrawStretchBox(Rect scale, object nodeBackground, float offset)
        {
            var style = nodeBackground as GwenStyle;
            if (style != null)
            {
                if (offset > 0)
                {
                    var o = Mathf.RoundToInt(offset);
                    style.Bordered.SetMargin(o, o, o, o);
                }
                style.Bordered.Draw(Skin.Renderer, ToRectangle(scale));
            }
        }

        private static Rectangle ToRectangle(Rect scale)
        {
            return new Rectangle(
                Mathf.RoundToInt(scale.x),
                Mathf.RoundToInt(scale.y),
                Mathf.RoundToInt(scale.width),
                Mathf.RoundToInt(scale.height)
                );
        }

        public void DrawStretchBox(Rect scale, object nodeBackground, Rect offset)
        {
            var style = nodeBackground as GwenStyle;
            if (style != null)
            {


                style.Bordered.SetMargin(Mathf.RoundToInt(offset.x), Mathf.RoundToInt(offset.y), Mathf.RoundToInt(offset.width), Mathf.RoundToInt(offset.height));

                style.Bordered.Draw(Skin.Renderer, ToRectangle(scale));
            }

        }

        public void DrawTextbox(string id, Rect bounds, string value, object itemTextEditingStyle, Action<string, bool> valueChangedAction)
        {
            var c = this.EnsureControl<global::Gwen.Control.Base, global::Gwen.Control.TextBox>(id, bounds, () =>
            {
                var control = new global::Gwen.Control.TextBox(this)
                {
                    Text = value,
                    ShouldDrawBackground = false
                };
                control.TextColorOverride = System.Drawing.Color.White;
                control.Alignment = Pos.Center;
                control.TextChanged += delegate(Base sender, EventArgs arguments)
                {
                    valueChangedAction(control.Text, false);
                };
                control.SubmitPressed+= delegate(Base sender, EventArgs arguments)
                {
                    valueChangedAction(control.Text, true);
                };
                control.SelectAllOnFocus = true;
                control.Focus();
                this.Invalidate();
                return control;
            });
            c.Text = value;
        }

        public void DrawWarning(Rect rect, string key)
        {

        }

        public void EndRender()
        {
            this.EndImmediate();
      
        }

        public void DrawRect(Rect boundsRect, Color color)
        {
            throw new NotImplementedException();
        }

        public void DrawNodeHeader(Rect boxRect, object backgroundStyle, bool isCollapsed, float scale)
        {
            var style = backgroundStyle as GwenStyle;
            if (style != null)
            {
                if (isCollapsed)
                {

                    var adjustedBounds = new Rect(boxRect.x - 10, boxRect.y + 1, boxRect.width + 21, boxRect.height + 9);
                    style.Bordered.Draw(Skin.Renderer, ToRectangle(adjustedBounds));
                }
                else
                {
                    var adjustedBounds = new Rect(boxRect.x - 10, boxRect.y + 1, boxRect.width + 21, 27 * scale);
                    var old = style.Bordered;
                    var oldMargin = old.m_Margin;
                    var b = new Bordered(old.MTexture, old._x, old._y, old._w, 25, new Margin(oldMargin.Left, oldMargin.Right, oldMargin.Right, 0));
                    b.Draw(Skin.Renderer, ToRectangle(adjustedBounds));
                }




            }
        }

        public void DoToolbar(Rect toolbarTopRect, DesignerWindow designerWindow, ToolbarPosition position)
        {
            var commands = designerWindow.Designer.AllCommands.Where(p => p.Position == position).ToArray();
            var isRight = position == ToolbarPosition.BottomRight || position == ToolbarPosition.BottomRight;
            if (isRight)
            {
                var x = toolbarTopRect.x + toolbarTopRect.width;

                foreach (var command in commands)
                {
                    var size = CalculateSize(command.Name, CachedStyles.ToolbarButton).x + 15;
                    x -= size;
                    var command1 = command;
                    DoButton(new Rect(x, toolbarTopRect.y, size, 24), command.Name, CachedStyles.ToolbarButton, () =>
                    {
                        InvertGraphEditor.ExecuteCommand(command1);
                    });
                }
            }
            //Skin.Renderer.DrawColor = System.Drawing.Color.Blue;
            //Skin.Renderer.DrawFilledRect(ToRectangle(toolbarTopRect));
        }

        public void DoTabs(Rect tabsRect, DesignerWindow designerWindow)
        {
            Skin.Renderer.DrawColor = System.Drawing.Color.LightBlue;
            Skin.Renderer.DrawFilledRect(ToRectangle(tabsRect));

        }

        public IEnumerable<object> ContextObjects
        {
            get { return DiagramViewModel.ContextObjects; }
        }

        public void CommandExecuted(IEditorCommand command)
        {
            Invalidate();
        }

        public void CommandExecuting(IEditorCommand command)
        {

        }

        public DiagramViewModel DiagramViewModel
        {
            get { return DesignerWindow.DiagramViewModel; }

        }

        public float Scale
        {
            get { return 1f; }
            set
            {

            }
        }

        public DiagramDrawer DiagramDrawer
        {
            get { return DesignerWindow.DiagramDrawer; }
        }


        public Type FindType(string clrTypeString)
        {
            var name = clrTypeString.Split(',').FirstOrDefault();
            if (name != null)
            {
                return InvertApplication.FindType(name);
            }
            return null;
        }

        public void ProcessInput()
        {

        }

        public void BeforeDrawGraph(Rect diagramRect)
        {

        }

        public void AfterDrawGraph(Rect diagramRect)
        {

        }

        public void DrawComplete()
        {

        }

        public List<string> ControlsLeftOver { get; set; }
        public Dictionary<string, Base> Controls { get; set; }
        public ScrollControl ScrollContainer { get; set; }

        public void AddControlToCanvas(global::Gwen.Control.Base control)
        {
            //this.AddChild(control);
            // No need to add here because it automatically does so in the contructor when you pass it "this"
        }

        public void RemoveControlFromCanvas(global::Gwen.Control.Base control)
        {
            this.RemoveChild(control, true);

        }

        public void SetControlPosition(global::Gwen.Control.Base control, float x, float y, float width, float height)
        {
            control.SetBounds(x, y, width, height);
        }
    }
}