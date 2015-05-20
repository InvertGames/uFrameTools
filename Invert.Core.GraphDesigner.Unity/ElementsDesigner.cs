using System.ComponentModel;
using Invert.Common;
using Invert.Core.GraphDesigner;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Invert.Core.GraphDesigner.Unity
{
    public class ElementsDesigner : EditorWindow, IDesignerWindowEvents
    { 
        private Vector2 _scrollPosition;

        public IEnumerable<object> ContextObjects
        {
            get
            {
                yield return this;
                if (DiagramViewModel != null)
                {
                    foreach (var co in DiagramViewModel.ContextObjects)
                    {
                        yield return co;
                    }
                }
            }
        }

        public bool IsMiddleMouseDown { get; set; }

        public Event LastEvent { get; set; }

        public Event LastMouseDownEvent { get; set; }

        public Vector2 PanStartPosition { get; set; }

        [MenuItem("Window/uFrame Designer", false, 1)]
        public static void Init()
        {
            // Get existing open window or if none, make a new one:
            var window = (ElementsDesigner)GetWindow(typeof(ElementsDesigner));
            window.title = "uFrame";
            window.wantsMouseMove = true;
            InvertGraphEditor.DesignerWindow = window.DesignerWindow;
            window.Show();
        }


        public DesignerWindow DesignerWindow
        {
            get
            {
                if (_designerWindow == null)
                {
                    _designerWindow = new DesignerWindow()
                    {
                        ParentHandler = this
                    };
                    
                }
                return _designerWindow;
            }
            set { _designerWindow = value; }
        }

        public Action DesignerWindowDisposer { get; set; }

        public DiagramViewModel DiagramViewModel
        {
            get { return DesignerWindow.DiagramViewModel; }
        }
        public DiagramDrawer DiagramDrawer
        {
            get { return DesignerWindow.DiagramDrawer; }
        }

        public void OnDestroy()
        {
            if (DesignerWindowDisposer != null)
            {
                DesignerWindowDisposer();
            }
            InvertApplication.Container = null;
        }
   
        private void HandleInput(MouseEvent mouse)
        {
            if (DiagramDrawer == null) return;
            if (DiagramViewModel == null) return;
            var e = Event.current;
            if (e == null)
            {
                return;
            }
            var handler = DesignerWindow.MouseEvent.CurrentHandler;

            if (e.type == EventType.MouseDown)
            {
                mouse.MouseDownPosition = mouse.MousePosition;
                mouse.IsMouseDown = true;
                mouse.MouseButton = e.button;
                handler.OnMouseDown(mouse);
                if (e.button == 1)
                {
                    e.Use();
                    handler.OnRightClick(mouse);
                    
                }

                if (e.clickCount > 1)
                {
                    handler.OnMouseDoubleClick(mouse);
                }
                if (IsFocused)
                {
                    if (DesignerWindow.CurrentProject != null)
                    {
                        Selection.activeObject = DesignerWindow.CurrentProject as UnityEngine.Object;
                        EditorUtility.SetDirty(Selection.activeObject);
                    }
                }
                LastMouseDownEvent = e;
            }
            if (e.rawType == EventType.MouseUp)
            {
                mouse.MouseUpPosition = mouse.MousePosition;
                mouse.IsMouseDown = false;
                handler.OnMouseUp(mouse);
            }
            else if (e.rawType == EventType.KeyDown)
            {
            }
            else
            {
            
                var mp = (e.mousePosition) * (1f / DiagramDrawer.Scale);

                mouse.MousePosition = mp;
                mouse.MousePositionDelta = mouse.MousePosition - mouse.LastMousePosition;
                mouse.MousePositionDeltaSnapped = mouse.MousePosition.Snap(DiagramViewModel.SnapSize * ElementDesignerStyles.Scale) - mouse.LastMousePosition.Snap(DiagramViewModel.SnapSize * ElementDesignerStyles.Scale);
                handler.OnMouseMove(mouse);
                mouse.LastMousePosition = mp;
                if (e.type == EventType.MouseMove)
                {
                    e.Use();
                    Repaint();
                }
            }
           
            LastEvent = Event.current;
            if (DiagramDrawer.IsEditingField)
            {
                if (LastEvent.keyCode == KeyCode.Return)
                {
                    var selectedGraphItem = DiagramViewModel.SelectedGraphItem;
                    DiagramViewModel.DeselectAll();
                    if (selectedGraphItem != null)
                    {
                        selectedGraphItem.Select();
                    }
                    return;    
                }
                return;
            }

            if (LastEvent != null)
            {
                if (LastEvent.type == EventType.keyUp)
                {
                    if (LastEvent.keyCode == KeyCode.LeftShift || LastEvent.keyCode == KeyCode.RightShift)
                        mouse.ModifierKeyStates.Shift = false;
                    if (LastEvent.keyCode == KeyCode.LeftControl || LastEvent.keyCode == KeyCode.RightControl || LastEvent.keyCode == KeyCode.LeftCommand || LastEvent.keyCode == KeyCode.RightCommand)
                        mouse.ModifierKeyStates.Ctrl = false;
                    if (LastEvent.keyCode == KeyCode.LeftAlt || LastEvent.keyCode == KeyCode.RightAlt )
                        mouse.ModifierKeyStates.Alt = false;
                }
            }

            if (LastEvent != null)
            {
                if (LastEvent.type == EventType.keyDown)
                {
                    if (LastEvent.keyCode == KeyCode.LeftShift || LastEvent.keyCode == KeyCode.RightShift )
                        mouse.ModifierKeyStates.Shift = true;
                    if (LastEvent.keyCode == KeyCode.LeftControl || LastEvent.keyCode == KeyCode.RightControl || LastEvent.keyCode == KeyCode.LeftCommand || LastEvent.keyCode == KeyCode.RightCommand)
                        mouse.ModifierKeyStates.Ctrl = true;
                    if (LastEvent.keyCode == KeyCode.LeftAlt || LastEvent.keyCode == KeyCode.RightAlt )
                        mouse.ModifierKeyStates.Alt = true;
                }
                // Debug.Log(string.Format("Shift: {0}, Alt: {1}, Ctrl: {2}",ModifierKeyStates.Shift,ModifierKeyStates.Alt,ModifierKeyStates.Ctrl));
            }

            var evt = LastEvent;
            if (evt != null && evt.isKey && evt.type == EventType.KeyUp && DiagramDrawer != null)
            {
               
                if (DiagramViewModel != null &&
                    (DiagramViewModel.SelectedNode == null || !DiagramViewModel.SelectedNode.IsEditing))
                {
                    if (DiagramViewModel.SelectedNodeItem == null)
                    {

                        if (DiagramDrawer.HandleKeyEvent(evt, mouse.ModifierKeyStates))
                        {
                            evt.Use();
                            mouse.ModifierKeyStates = null;
                        }
                    }
                    
                }
            }
        }

        public void InfoBox(string message, MessageType type = MessageType.Info)
        {
            EditorGUI.HelpBox(new Rect(15, 30, 300, 30), message, type);
        }

        public SerializedObject SerializedGraph { get; set; }
        public bool IsFocused { get; set; }

        public void OnFocus()
        {
            IsFocused = true;
        }

        public void OnEnable()
        {
            InvertGraphEditor.DesignerWindow = this.DesignerWindow;
        }

        public void OnGUI()
        {
            InvertGraphEditor.DesignerWindow = this.DesignerWindow;
            if (InvertGraphEditor.Container != null)
            {
                var width = Screen.width;
                var height = Screen.height;
                var toolbarTopRect = new Rect(0, 0, width, 18);
                var tabsRect = new Rect(0, toolbarTopRect.height, width, 31);

                var diagramRect = new Rect(0f, tabsRect.y + tabsRect.height, width - 3, height - ((toolbarTopRect.height * 2)) - tabsRect.height - 20);
                var toolbarBottomRect = new Rect(0f, diagramRect.y + diagramRect.height, width - 3, toolbarTopRect.height);

                EditorGUI.DrawRect(diagramRect, InvertGraphEditor.Settings.BackgroundColor);
                DesignerWindow.Draw(InvertGraphEditor.PlatformDrawer, Screen.width, Screen.height, _scrollPosition, 1f);
            }
  
            if (EditorApplication.isCompiling)
            {
                InfoBox("Compiling.. Please Wait!");
            }
            if (DiagramViewModel != null)
            {
                var refactors = DiagramViewModel.RefactorCount;
                if (refactors > 0)
                {
                    InfoBox(String.Format("You have {0} refactors. Save before recompiling occurs.", refactors), MessageType.Warning);
                }
            }
       
     
        }

        public void OnLostFocus()
        {
            InvertGraphEditor.DesignerWindow = this.DesignerWindow;
            IsFocused = false;
            DesignerWindow.ModifierKeyStates = null;
            DesignerWindow.MouseEvent = null;
            DesignerWindow.Toolbar = null;
        }

        private int fpsCount = 0;
        private DesignerWindow _designerWindow;

        public void Update()
        {

            if (fpsCount > 15 || (DesignerWindow.MouseEvent != null && DesignerWindow.MouseEvent.IsMouseDown))
            {
                fpsCount = 0;
                Repaint();

            }
            fpsCount++;
        }


        private void HandlePanning(Rect diagramRect)
        {
            if (Event.current.button == 2 && Event.current.type == EventType.MouseDown)
            {
                IsMiddleMouseDown = true;
                PanStartPosition = Event.current.mousePosition;
            }
            if (Event.current.button == 2 && Event.current.rawType == EventType.MouseUp && IsMiddleMouseDown)
            {
                IsMiddleMouseDown = false;
            }
            if (IsMiddleMouseDown)
            {
                var delta = PanStartPosition - Event.current.mousePosition;
                _scrollPosition += delta;
                if (_scrollPosition.x < 0)
                    _scrollPosition.x = 0;
                if (_scrollPosition.y < 0)
                    _scrollPosition.y = 0;
                if (_scrollPosition.x > diagramRect.width - diagramRect.x)
                {
                    _scrollPosition.x = diagramRect.width - diagramRect.x;
                }
                if (_scrollPosition.y > diagramRect.height - diagramRect.y)
                {
                    _scrollPosition.y = diagramRect.height - diagramRect.y;
                }
            }
        }

        public void ProcessInput()
        {
            HandleInput(DesignerWindow.MouseEvent);
            HandlePanning(DesignerWindow.DiagramRect);
        }

        public void BeforeDrawGraph(Rect diagramRect)
        {
            
            _scrollPosition = GUI.BeginScrollView(diagramRect, _scrollPosition, DiagramViewModel.DiagramBounds);

        }

        public void AfterDrawGraph(Rect diagramRect)
        {
            GUI.EndScrollView();
        }

        public void DrawComplete()
        {
            
        }

     
    }
}