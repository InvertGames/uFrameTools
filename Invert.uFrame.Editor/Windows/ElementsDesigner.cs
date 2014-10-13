using Invert.Common;
using Invert.uFrame.Editor.ElementDesigner;
using System;
using System.Collections.Generic;
using System.Linq;
using Invert.uFrame.Editor.ElementDesigner.Commands;
using UnityEditor;
using UnityEngine;

namespace Invert.uFrame.Editor
{
    public class ElementsDesigner : EditorWindow, ICommandHandler
    {
        public event EventHandler ProjectChanged;

        private static float HEIGHT = 768;

        private static List<Matrix4x4> stack = new List<Matrix4x4>();

        private static float WIDTH = 1024;

        private IProjectRepository _currentProject;

        [SerializeField]
        private ElementsDiagram _diagramDrawer;

        private bool _drawEveryFrame = true;

        private ModifierKeyState _modifierKeyStates;

        private MouseEvent _mouseEvent;

        private ProjectRepository[] _projects;

        private Vector2 _scrollPosition;

        private ICommandUI _toolbar;
        private string _newProjectName = "NewProject";

        public static INodeRepository SelectedElementDiagram
        {
            get { return Selection.activeObject as IGraphData; }
        }

        public IEnumerable<object> ContextObjects
        {
            get
            {
                yield return this;
                if (DiagramDrawer != null)
                {
                    foreach (var co in DiagramDrawer.ContextObjects)
                    {
                        yield return co;
                    }
                }
            }
        }

        public IProjectRepository CurrentProject
        {
            get
            {
                if (uFrameEditor.CurrentProject == null)
                {
                    if (!String.IsNullOrEmpty(LastLoadedProject))
                    {
                        uFrameEditor.CurrentProject = uFrameEditor.Projects.FirstOrDefault(p => p.name == LastLoadedProject);
                    }
                    if (uFrameEditor.CurrentProject == null)
                    {
                        uFrameEditor.CurrentProject = uFrameEditor.Projects.FirstOrDefault();
                    }
                }
                return uFrameEditor.CurrentProject;
            }
            set
            {
                var changed = uFrameEditor.CurrentProject != value;

                uFrameEditor.CurrentProject = value;
                if (value != null)
                {
                    if (changed)
                    {
                        OnProjectChanged();
                    }
                    LastLoadedProject = value.Name;
                }
            }
        }

        public ElementsDiagram DiagramDrawer
        {
            get { return _diagramDrawer; }
            set
            {
                _diagramDrawer = value;
                _toolbar = null;
            }
        }

        public Rect DiagramRect { get; set; }

        public DiagramViewModel DiagramViewModel
        {
            get
            {
                if (DiagramDrawer == null) return null;
                return DiagramDrawer.DiagramViewModel;
            }
        }

        public float InspectorWidth
        {
            get
            {
                return 0;
                //if (Diagram == null || Diagram.Selected == null)
                //    return 0;
                //return 240;
            }
        }

        public bool IsMiddleMouseDown { get; set; }

        public Event LastEvent { get; set; }

        public string LastLoadedDiagram
        {
            get
            {
                return EditorPrefs.GetString("LastLoadedDiagram", null);
            }
            set
            {
                EditorPrefs.SetString("LastLoadedDiagram", value);
            }
        }

        public string LastLoadedProject
        {
            get { return EditorPrefs.GetString("UF_LastLoadedProject", String.Empty); }
            set { EditorPrefs.SetString("UF_LastLoadedProject", value); }
        }

        public Event LastMouseDownEvent { get; set; }

        public Vector2 LastMousePosition { get; set; }

        public ModifierKeyState ModifierKeyStates
        {
            get { return _modifierKeyStates ?? (_modifierKeyStates = new ModifierKeyState()); }
            set { _modifierKeyStates = value; }
        }

        public MouseEvent MouseEvent
        {
            get { return _mouseEvent ?? (_mouseEvent = new MouseEvent(ModifierKeyStates, DiagramDrawer)); }
            set { _mouseEvent = value; }
        }

        public Vector2 PanStartPosition { get; set; }

        public ICommandUI Toolbar
        {
            get
            {
                if (_toolbar != null) return _toolbar;

                return _toolbar = uFrameEditor.CreateCommandUI<ToolbarUI>(this, typeof(IToolbarCommand));
            }
            set { _toolbar = value; }
        }

        static public void BeginGUI()
        {
            stack.Add(GUI.matrix);
            Matrix4x4 m = new Matrix4x4();
            var w = (float)Screen.width;
            var h = (float)Screen.height;
            var aspect = w / h;
            var scale = 1f;
            var offset = Vector3.zero;
            if (aspect < (WIDTH / HEIGHT))
            { //screen is taller
                scale = (Screen.width / WIDTH);
                offset.y += (Screen.height - (HEIGHT * scale)) * 0.5f;
            }
            else
            { // screen is wider
                scale = (Screen.height / HEIGHT);
                offset.x += (Screen.width - (WIDTH * scale)) * 0.5f;
            }
            m.SetTRS(offset, Quaternion.identity, Vector3.one * scale);
            GUI.matrix *= m;
        }

        static public void EndGUI()
        {
            GUI.matrix = stack[stack.Count - 1];
            stack.RemoveAt(stack.Count - 1);
        }

        [MenuItem("Window/Element Designer", false, 1)]
        public static void Init()
        {
            // Get existing open window or if none, make a new one:
            var window = (ElementsDesigner)GetWindow(typeof(ElementsDesigner));
            window.title = "Elements";
            //uFrameEditor.ProjectChanged += window.UFrameEditorOnProjectChanged;
            //window.DesignerViewModel = uFrameEditor.Application.Designer;

            //var repo = new ElementsDataRepository();
            //var diagram = new ElementsDiagram(repo);
            //diagram.Data.ViewModels.Add(repo.GetViewModel(typeof(FPSWeaponViewModel)));
            //diagram.Data.ViewModels.Add(repo.GetViewModel(typeof(FPSBulletViewModel)));
            uFrameEditor.DesignerWindow = window;
            // RemoveFromDiagram when switching to add all
            window.Show();
        }

        public void CommandExecuted(IEditorCommand command)
        {
            if (DiagramDrawer != null)
            {
                DiagramDrawer.Refresh();
            }
        }

        public void CommandExecuting(IEditorCommand command)
        {
        }

        public void DoCommand(IEditorCommand command)
        {
            var obj = ContextObjects.FirstOrDefault(p => command.For.IsAssignableFrom(p.GetType()));
            GUI.enabled = command.CanPerform(obj) == null;
            if (command is IDynamicOptionsCommand)
            {
                var cmd = command as IDynamicOptionsCommand;

                foreach (var ufContextMenuItem in cmd.GetOptions(obj))
                {
                    if (GUILayout.Button(new GUIContent(ufContextMenuItem.Name), EditorStyles.toolbarButton))
                    {
                        cmd.SelectedOption = ufContextMenuItem;
                        this.ExecuteCommand(command);
                    }
                }
            }
            else if (GUILayout.Button(new GUIContent(command.Title), EditorStyles.toolbarButton))
            {
                if (command is IParentCommand)
                {
                    var contextUI = uFrameEditor.CreateCommandUI<ContextMenuUI>(this, command.GetType());
                    contextUI.Flatten = true;
                    contextUI.Go();
                }
                else
                {
                    this.ExecuteCommand(command);
                }
            }
            GUI.enabled = true;
        }

        private void HandleInput()
        {
            if (DiagramDrawer == null) return;

            var e = Event.current;
            if (e == null)
            {
                return;
            }
            var handler = MouseEvent.CurrentHandler;

            if (e.type == EventType.MouseDown)
            {
                MouseEvent.MouseDownPosition = MouseEvent.MousePosition;
                MouseEvent.IsMouseDown = true;
                MouseEvent.MouseButton = e.button;
                handler.OnMouseDown(MouseEvent);
                if (e.button == 1)
                {
                    handler.OnRightClick(MouseEvent);
                }

                if (e.clickCount > 1)
                {
                    handler.OnMouseDoubleClick(MouseEvent);
                }
                LastMouseDownEvent = e;
            }
            if (e.rawType == EventType.MouseUp)
            {
                MouseEvent.MouseUpPosition = MouseEvent.MousePosition;
                MouseEvent.IsMouseDown = false;
                handler.OnMouseUp(MouseEvent);
            }
            else if (e.rawType == EventType.KeyDown)
            {
            }
            else
            {
                var mp = (e.mousePosition) * (1f / ElementsDiagram.Scale);

                MouseEvent.MousePosition = mp;
                MouseEvent.MousePositionDelta = MouseEvent.MousePosition - MouseEvent.LastMousePosition;

                handler.OnMouseMove(MouseEvent);
                MouseEvent.LastMousePosition = mp;
            }

            LastEvent = Event.current;
            if (LastEvent != null)
            {
                if (LastEvent.type == EventType.keyUp)
                {
                    if (LastEvent.keyCode == KeyCode.LeftShift || LastEvent.keyCode == KeyCode.RightShift)
                        ModifierKeyStates.Shift = false;
                    if (LastEvent.keyCode == KeyCode.LeftControl || LastEvent.keyCode == KeyCode.RightControl)
                        ModifierKeyStates.Ctrl = false;
                    if (LastEvent.keyCode == KeyCode.LeftAlt || LastEvent.keyCode == KeyCode.RightAlt)
                        ModifierKeyStates.Alt = false;
                }
            }

            if (LastEvent != null)
            {
                if (LastEvent.type == EventType.keyDown)
                {
                    if (LastEvent.keyCode == KeyCode.LeftShift || LastEvent.keyCode == KeyCode.RightShift)
                        ModifierKeyStates.Shift = true;
                    if (LastEvent.keyCode == KeyCode.LeftControl || LastEvent.keyCode == KeyCode.RightControl)
                        ModifierKeyStates.Ctrl = true;
                    if (LastEvent.keyCode == KeyCode.LeftAlt || LastEvent.keyCode == KeyCode.RightAlt)
                        ModifierKeyStates.Alt = true;
                }
                // Debug.Log(string.Format("Shift: {0}, Alt: {1}, Ctrl: {2}",ModifierKeyStates.Shift,ModifierKeyStates.Alt,ModifierKeyStates.Ctrl));
            }

            var evt = LastEvent;
            if (evt != null && evt.isKey && evt.type == EventType.KeyUp && DiagramDrawer != null)
            {
                Debug.Log("Sending key command");
                if (DiagramViewModel != null &&
                    (DiagramViewModel.SelectedNode == null || !DiagramViewModel.SelectedNode.IsEditing))
                {
                    if (DiagramViewModel.SelectedNodeItem == null)
                    {
                        if (DiagramDrawer.HandleKeyEvent(evt, ModifierKeyStates))
                        {
                            evt.Use();
                        }
                    }
                    
                }
            }
        }

        public void InfoBox(string message, MessageType type = MessageType.Info)
        {
            EditorGUI.HelpBox(new Rect(15, 30, 300, 30), message, type);
        }

        private void LoadDiagram(IGraphData diagram)
        {
            if (diagram == null) return;
            try
            {
                if (Undo.undoRedoPerformed != null) Undo.undoRedoPerformed -= UndoRedoPerformed;
                Undo.undoRedoPerformed += UndoRedoPerformed;
                SerializedGraph = new SerializedObject(diagram as UnityEngine.Object);
                //Diagram = uFrameEditor.Container.Resolve<ElementsDiagram>();
                DiagramDrawer = new ElementsDiagram(new DiagramViewModel(diagram, CurrentProject));
                MouseEvent = new MouseEvent(ModifierKeyStates, DiagramDrawer);
                DiagramDrawer.Dirty = true;
                //DiagramDrawer.Data.ApplyFilter();
                DiagramDrawer.Refresh();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Debug.Log("Either a plugin isn't installed or the file could no longer be found. See Exception error");
                LastLoadedDiagram = null;
            }
        }

        public SerializedObject SerializedGraph { get; set; }

        public void OnFocus()
        {
            _drawEveryFrame = true;
        }

        public void SwitchDiagram(IGraphData data)
        {
            CurrentProject.CurrentGraph = data as ElementsGraph;
            LoadDiagram(CurrentProject.CurrentGraph);
        }

        public void OnEnable()
        {
            uFrameEditor.DesignerWindow = this;
        }

        public void OnGUI()
        {
            uFrameEditor.DesignerWindow = this;
            var style = ElementDesignerStyles.Background;
            style.border = new RectOffset(
                Mathf.RoundToInt(41),
                Mathf.RoundToInt(41),
                Mathf.RoundToInt(32),
                Mathf.RoundToInt(32));
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            //DoToolbar(diagramRect);
            DoToolbar();
            GUILayout.EndHorizontal();

            DiagramRect = new Rect(0f, (EditorStyles.toolbar.fixedHeight) - 1, Screen.width - 3, Screen.height - (EditorStyles.toolbar.fixedHeight * 2) - EditorStyles.toolbar.fixedHeight - 2);

            EditorGUI.DrawRect(DiagramRect, uFrameEditor.Settings.BackgroundColor);
            GUI.Box(DiagramRect, String.Empty, style);
            if (CurrentProject == null)
            {
                DiagramDrawer = null;
                var width = 400;
                var height = 300;
                var x = (Screen.width / 2f) - (width / 2f);
                var y = (Screen.height / 2f) - (width / 2f);
                var padding = 30;
                var rect = new Rect(x, y, width, height);
                ElementDesignerStyles.DrawExpandableBox(rect, ElementDesignerStyles.NodeBackground, string.Empty);
                GUI.DrawTexture(new Rect(x, y + 25, width, 100), ElementDesignerStyles.GetSkinTexture("uframeLogoLarge"), ScaleMode.ScaleToFit);
                rect.y += 110;
                rect.height -= 110;
                GUILayout.BeginArea(rect);
                GUILayout.BeginArea(new Rect(padding, padding, width - (padding * 2), height - (padding * 2)));

                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("Create New Project", ElementDesignerStyles.ViewModelHeaderStyle);

                GUILayout.Space(20f);
                _newProjectName = EditorGUILayout.TextField("New Project Name:", _newProjectName);
                if (GUILayout.Button("Create Project"))
                {
                    UFrameAssetManager.NewUFrameProject(_newProjectName);
                }
                EditorGUILayout.EndVertical();

                GUILayout.EndArea();
                GUILayout.EndArea();
            }
            if (DiagramDrawer == null)
            {
                if (CurrentProject != null)
                {
                    if (CurrentProject.CurrentGraph != null)
                    {
                        LoadDiagram(CurrentProject.CurrentGraph);
                    }
                }
                //else
                //{
                //    return;
                //}
            }
            if (DiagramDrawer == null)
            {
                return;
            }
            else
            {
                DiagramDrawer.Rect = DiagramRect;
                if (Event.current.control && Event.current.type == EventType.mouseDown)
                {
                }
                _scrollPosition = GUI.BeginScrollView(DiagramRect, _scrollPosition, DiagramDrawer.DiagramSize);

                HandlePanning(DiagramRect);
                if (DiagramViewModel != null)
                {
                    var softColor = uFrameEditor.Settings.GridLinesColor;
                    var hardColor = uFrameEditor.Settings.GridLinesColorSecondary;
                    var x = 0f;
                    var every10 = 0;

                    while (x < DiagramRect.width + _scrollPosition.x)
                    {
                        Handles.color = softColor;
                        if (every10 == 10)
                        {
                            Handles.color = hardColor;
                            every10 = 0;
                        }
                        Handles.DrawLine(new Vector2(x, 0f), new Vector2(x, Screen.height + _scrollPosition.y));
                        x += DiagramViewModel.Settings.SnapSize * ElementDesignerStyles.Scale;
                        every10++;
                    }
                    var y = 3f;
                    every10 = 0;
                    while (y < DiagramRect.height + _scrollPosition.y)
                    {
                        Handles.color = softColor;
                        if (every10 == 10)
                        {
                            Handles.color = hardColor;
                            every10 = 0;
                        }
                        Handles.DrawLine(new Vector2(0, y), new Vector2(Screen.width + _scrollPosition.x, y));
                        y += DiagramViewModel.Settings.SnapSize * ElementDesignerStyles.Scale;
                        every10++;
                    }
                }
                //BeginGUI();
                DiagramDrawer.Draw(ElementDesignerStyles.Scale);
                HandleInput();

                //#if DEBUG
                //                GUILayout.BeginArea(new Rect(10f, 70f, 500f, 500f));
                //                GUILayout.Label(string.Format("Mouse Position: x = {0}, y = {1}", MouseEvent.MousePosition.x, MouseEvent.MousePosition.y));
                //                GUILayout.Label(string.Format("Mouse Position Delta: x = {0}, y = {1}", MouseEvent.MousePositionDelta.x, MouseEvent.MousePositionDelta.y));
                //                GUILayout.Label(string.Format("Mouse Down: {0}", MouseEvent.IsMouseDown));
                //                GUILayout.Label(string.Format("Last Mouse Down Position: {0}", MouseEvent.LastMousePosition));
                //                if (DiagramDrawer != null)
                //                {
                //                    GUILayout.Label(string.Format("Drawer Count: {0}", DiagramDrawer.DiagramViewModel.GraphItems.Count));
                //                    if (DiagramDrawer.DrawersAtMouse != null)
                //                        foreach (var drawer in DiagramDrawer.DrawersAtMouse)
                //                        {
                //                            GUILayout.Label(drawer.ToString());
                //                        }
                //                    if (DiagramDrawer.DiagramViewModel != null)
                //                        foreach (var drawer in DiagramDrawer.DiagramViewModel.SelectedGraphItems)
                //                        {
                //                            GUILayout.Label(drawer.ToString());
                //                        }
                //                }

                //                GUILayout.EndArea();
                //#endif
                //EndGUI();
                GUI.EndScrollView();
                GUILayout.Space(DiagramRect.height);
                GUILayout.BeginHorizontal(EditorStyles.toolbar);
                //DoToolbar(diagramRect);
                Toolbar.GoBottom();
                GUILayout.EndHorizontal();
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
            HandleKeys();

            if (Event.current.type == EventType.ValidateCommand && Event.current.commandName == "UndoRedoPerformed")
            {
            }

            if (DiagramDrawer != null && DiagramDrawer.Dirty || EditorApplication.isCompiling)
            {
            }
        }

        private void HandleKeys()
        {

        }

        public void OnLostFocus()
        {
            uFrameEditor.DesignerWindow = this;
            if (DiagramViewModel != null)
                DiagramViewModel.DeselectAll();

            ModifierKeyStates = new ModifierKeyState();
            MouseEvent = null;
            _drawEveryFrame = false;
        }

        public void Update()
        {
            if (_drawEveryFrame)
                Repaint();
        }

        private void DoToolbar()
        {
            try
            {
                if (
                  GUILayout.Button(
                      new GUIContent(CurrentProject == null ? "--Select Project--" : CurrentProject.Name),
                      EditorStyles.toolbarPopup))
                {
                    SelectProject();
                }

                if (CurrentProject != null)
                {
                    if (
                    GUILayout.Button(
                        new GUIContent(DiagramViewModel == null ? "--Select Diagram--" : DiagramViewModel.Title),
                        EditorStyles.toolbarPopup))
                    {
                        SelectDiagram();
                    }
                    Toolbar.Go();
                }
            }
            catch (Exception ex)
            {
                DiagramDrawer = null;
                return;
            }
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

        private void OnProjectChanged()
        {
            DiagramDrawer = null;
        }

        private void SelectDiagram()
        {
            var menu = new GenericMenu();
            foreach (var item in CurrentProject.Diagrams)
            {
                GraphData item1 = item;
                menu.AddItem(new GUIContent(item.Name), DiagramDrawer != null && CurrentProject.CurrentGraph == item1, () =>
                {
                    CurrentProject.CurrentGraph = item1;
                    LoadDiagram(CurrentProject.CurrentGraph);
                });
            }
            menu.AddItem(new GUIContent("Force Refresh"), false, () => { CurrentProject.Refresh(); });
            foreach (var graphType in uFrameEditor.Container.Mappings.Where(p => p.From == typeof(GraphData)))
            {
                TypeMapping type = graphType;
                menu.AddItem(new GUIContent("Create " + graphType.Name), false, () =>
                {
                    var diagram = CurrentProject.CreateNewDiagram(type.To);
                    DiagramDrawer = null;
                    SwitchDiagram(diagram);
                   
                });
            }

            menu.ShowAsContext();
        }

        private void SelectProject()
        {
            var projects = uFrameEditor.Projects;

            var menu = new GenericMenu();
            foreach (var project in projects)
            {
                IProjectRepository project1 = project;
                menu.AddItem(new GUIContent(project.name), project1 == CurrentProject, () =>
                {
                    CurrentProject = project1;
                    LoadDiagram(CurrentProject.CurrentGraph);
                });
            }

            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Force Refresh"), false, () => { uFrameEditor.Projects = null; });
            menu.ShowAsContext();
        }

        private void UFrameEditorOnProjectChanged(object sender, EventArgs eventArgs)
        {
        }

        private void UndoRedoPerformed()
        {
            CurrentProject.Refresh();
            SwitchDiagram(CurrentProject.CurrentGraph);
        }
    }
}