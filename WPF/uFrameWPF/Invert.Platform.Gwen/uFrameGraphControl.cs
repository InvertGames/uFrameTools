using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Windows.Forms;
using Gwen.Control;
using Gwen.Input;
using Invert.Core;
using Invert.Core.GraphDesigner;
using Invert.Platform.Gwen;
using OpenTK.Graphics;
using KeyPressEventArgs = System.Windows.Forms.KeyPressEventArgs;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;
namespace Invert.Platform.Gwen
{
    public partial class uFrameGraphControl : UserControl
    {
         private global::Gwen.Input.OpenTK input;
        private global::Gwen.Renderer.OpenTK renderer;
        private global::Gwen.Skin.Base skin;
        private global::Gwen.Control.Canvas canvas;
        private GraphApplication _theGraphApplication;

        public OpenTK.GLControl GLControl;
        public uFrameGraphControl()
        {
            this.GLControl = new OpenTK.GLControl(new GraphicsMode(ColorFormat.Empty, 24, 0, 8));
            this.GLControl.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(83)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.GLControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GLControl.Location = new System.Drawing.Point(0, 24);
            this.GLControl.Name = "GLControl";
            this.GLControl.Size = new System.Drawing.Size(766, 467);
            this.GLControl.TabIndex = 0;
            this.GLControl.VSync = false;
            this.GLControl.Load += new System.EventHandler(this.GLControl_Load);
            this.GLControl.Paint += new System.Windows.Forms.PaintEventHandler(this.GLControl_Paint);
            this.GLControl.KeyDown += new System.Windows.Forms.KeyEventHandler(this.GLControl_KeyDown);
            this.GLControl.KeyUp += new System.Windows.Forms.KeyEventHandler(this.GLControl_KeyUp);
            this.GLControl.MouseClick += new System.Windows.Forms.MouseEventHandler(this.GLControl_MouseClick);
            this.GLControl.Resize += new System.EventHandler(this.GLControl_Resize);
            this.Controls.Add(this.GLControl);
            InitializeComponent();
            
            

            GLControl.KeyDown += Keyboard_KeyDown;
            GLControl.KeyPress += Keyboard_KeyPress;
            GLControl.KeyUp += Keyboard_KeyUp;

            GLControl.MouseDown += Mouse_ButtonDown;
            GLControl.MouseUp += Mouse_ButtonUp;
            GLControl.MouseMove += Mouse_Move;
            GLControl.MouseWheel += Mouse_Wheel;
        }

        private void Keyboard_KeyPress(object sender, KeyPressEventArgs e)
        {
            canvas.Input_Character(e.KeyChar);
            GLControl.Invalidate();
        }

        private void Mouse_Wheel(object sender, MouseEventArgs e)
        {

        }

        private void Mouse_Move(object sender, MouseEventArgs e)
        {
            int dx = e.X - LastLocation.X;
            int dy = e.Y - LastLocation.Y;

            canvas.Input_MouseMoved(e.X, e.Y, dx, dy);
            LastLocation = e.Location;
            GLControl.Invalidate();
        }

        public Point LastLocation { get; set; }

        public GraphApplication TheGraphApplication
        {
            get { return _theGraphApplication; }
            set { _theGraphApplication = value; }
        }

        public bool IsDirty { get; set; }
        public string Filename { get; set; }

        private void Mouse_ButtonUp(object sender, MouseEventArgs e)
        {
            int ButtonID = -1; //Do not trigger event.

            if (e.Button == MouseButtons.Left)
                ButtonID = 0;
            else if (e.Button == MouseButtons.Right)
                ButtonID = 1;

            if (ButtonID != -1) //We only care about left and right click for now
                canvas.Input_MouseButton(ButtonID, false);
        }

        private void Mouse_ButtonDown(object sender, MouseEventArgs e)
        {
            int ButtonID = -1; //Do not trigger event.

            if (e.Button == MouseButtons.Left)
                ButtonID = 0;
            else if (e.Button == MouseButtons.Right)
                ButtonID = 1;

            if (ButtonID != -1) //We only care about left and right click for now
                canvas.Input_MouseButton(ButtonID, true);

            GLControl.Invalidate();
        }
        
        private void Keyboard_KeyUp(object sender, KeyEventArgs e)
        {
            char ch = TranslateChar(e.KeyCode);

            if (InputHandler.DoSpecialKeys(canvas, ch))
                return;
            /*
            if (ch != ' ')
            {
                m_Canvas.Input_Character(ch);
            }
            */
            global::Gwen.Key iKey = TranslateKeyCode(e.KeyCode);

            canvas.Input_Key(iKey, false);
            GLControl.Invalidate();
        }
              /// <summary>
        /// Translates alphanumeric OpenTK key code to character value.
        /// </summary>
        /// <param name="key">OpenTK key code.</param>
        /// <returns>Translated character.</returns>
        private static char TranslateChar(Keys key)
        {
                  switch (key)
                  {
                      case Keys.A:
                          return 'a';
                      case Keys.B:
                          return 'b';
                      case Keys.C:
                          return 'c';
                      case Keys.D:
                          return 'd';
                      case Keys.E:
                          return 'e';
                      case Keys.F:
                          return 'f';
                      case Keys.G:
                          return 'g';
                      case Keys.H:
                          return 'h';
                      case Keys.J:
                          return 'j';
                      case Keys.K:
                          return 'k';
                      case Keys.L:
                          return 'l';
                      case Keys.M:
                          return 'm';
                      case Keys.N:
                          return 'n';
                      case Keys.O:
                          return 'o';
                      case Keys.P:
                          return 'p';
                      case Keys.Q:
                          return 'q';
                      case Keys.R:
                          return 'r';
                      case Keys.S:
                          return 's';
                      case Keys.T:
                          return 't';
                      case Keys.U:
                          return 'u';
                      case Keys.V:
                          return 'v';
                      case Keys.W:
                          return 'w';
                      case Keys.X:
                          return 'x';
                      case Keys.Y:
                          return 'y';
                      case Keys.Z:
                          return 'z';

                  }

            return ' ';
        }
        private void Keyboard_KeyDown(object sender, KeyEventArgs e)
        {
            //KeyboardKeyEventArgs ev = args as KeyboardKeyEventArgs;
            char ch = TranslateChar(e.KeyCode);

            if (InputHandler.DoSpecialKeys(canvas, ch))
                return;
            /*
            if (ch != ' ')
            {
                m_Canvas.Input_Character(ch);
            }
            */
            global::Gwen.Key iKey = TranslateKeyCode(e.KeyCode);

            canvas.Input_Key(iKey, true); 
            GLControl.Invalidate();

        }
        /// <summary>
        /// Translates control key's OpenTK key code to GWEN's code.
        /// </summary>
        /// <param name="key">OpenTK key code.</param>
        /// <returns>GWEN key code.</returns>
        private global::Gwen.Key TranslateKeyCode(Keys key)
        {
            switch (key)
            {
                case Keys.Back: return global::Gwen.Key.Backspace;
                case Keys.Enter: return global::Gwen.Key.Return;
                case Keys.Escape: return global::Gwen.Key.Escape;
                case Keys.Tab: return global::Gwen.Key.Tab;
                case Keys.Space: return global::Gwen.Key.Space;
                case Keys.Up: return global::Gwen.Key.Up;
                case Keys.Down: return global::Gwen.Key.Down;
                case Keys.Left: return global::Gwen.Key.Left;
                case Keys.Right: return global::Gwen.Key.Right;
                case Keys.Home: return global::Gwen.Key.Home;
                case Keys.End: return global::Gwen.Key.End;
                case Keys.Delete: return global::Gwen.Key.Delete;
                //case Keys.LControl:
                //    this.m_AltGr = true;
                //    return Key.Control;
                case Keys.Alt: return global::Gwen.Key.Alt;
                case Keys.Shift: return global::Gwen.Key.Shift;
                case Keys.Control: return global::Gwen.Key.Control;
                //case global::OpenTK.Input.Key.RAlt:
                //    if (this.m_AltGr)
                //    {
                //        this.m_Canvas.Input_Key(Key.Control, false);
                //    }
                //    return Key.Alt;
                //case global::OpenTK.Input.Key.RShift: return Key.Shift;

            }
            return global::Gwen.Key.Invalid;
        }

        bool loaded = false;
        private void Form1_Load(object sender, EventArgs e)
        {
            GLControl.Invalidate();
            WindowsContextMenu.ContextMenu = TheContextMenu;
            WindowsContextMenu.Control = GLControl;
            var commands = InvertGraphEditor.Container.ResolveAll<IToolbarCommand>();
            foreach (var command in commands)
            {
                var command1 = command;
                Toolbar.Items.Add(new ToolStripButton(command.Name, null, (s, ev) =>
                {
                    InvertGraphEditor.ExecuteCommand(command1);
                }));
            }
        }   

        private void GLControl_Load(object sender, EventArgs e)
        {
            loaded = true;

            renderer = new global::Gwen.Renderer.OpenTK();
            skin = new global::Gwen.Skin.TexturedBase(renderer, "DefaultSkin.png");
            //skin = new Gwen.Skin.Simple(renderer);
            //skin.DefaultFont = new Font(renderer, "Courier", 10);
            canvas = new Canvas(skin);

            input = new global::Gwen.Input.OpenTK(this);
            input.Initialize(canvas);

            canvas.SetSize(GLControl.Width, GLControl.Height);
            canvas.ShouldDrawBackground = true;
            canvas.BackgroundColor = Color.FromArgb(255, 75, 75, 75);
            TheGraphApplication = new GraphApplication(canvas);
            if (Filename != null)
            {
                TheGraphApplication.Filename = Filename;
                TheGraphApplication.LoadFile();
            }
            GLControl.Invalidate();
            GLControl_Resize(sender,e);
        }

        private void GLControl_Resize(object sender, EventArgs e)
        {
            if (!loaded)
                return;
            GL.Viewport(0, 0, GLControl.Width, GLControl.Height);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, GLControl.Width, GLControl.Height, 0, -1, 1);

            canvas.SetSize(GLControl.Width, GLControl.Height);
        }

        private void GLControl_Paint(object sender, PaintEventArgs e)
        {
            if (!loaded) // Play nice
                return;

            // GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);
            canvas.RenderCanvas();


            GLControl.SwapBuffers();
        }

        private void GLControl_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {

        }

        private void GLControl_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void GLControl_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Destroy();
        }

        public  void Destroy()
        {
            canvas.Dispose();
            skin.Dispose();
            renderer.Dispose();
        }

        public void Save()
        {
            
            var projectService = InvertApplication.Container.Resolve<ProjectService>();
            projectService.CurrentProject.Save();
      
        }
    }
}
