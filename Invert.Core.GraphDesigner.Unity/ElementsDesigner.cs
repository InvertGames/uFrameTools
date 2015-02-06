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
    public class ElementsDesigner : EditorWindow, IGraphWindow
    {
        public event EventHandler ProjectChanged;

        private static float HEIGHT = 768;

        private static List<Matrix4x4> stack = new List<Matrix4x4>();

        private static float WIDTH = 1024;

        private IProjectRepository _currentProject;

        [SerializeField]
        private DiagramDrawer _diagramDrawer;

        private bool _drawEveryFrame = true;

        private ModifierKeyState _modifierKeyStates;

        private MouseEvent _mouseEvent;

        private ProjectRepository[] _projects;

        private Vector2 _scrollPosition;

        private ICommandUI _toolbar;
        private string _newProjectName = "NewProject";
        private DesignerViewModel _designerViewModel;

        public static INodeRepository SelectedElementDiagram
        {
            get { return Selection.activeObject as IGraphData; }
        }

        public DesignerViewModel Designer
        {
            get
            {
                if (CurrentProject == null)
                    return null;
                if (_designerViewModel == null)
                {
                    _designerViewModel = new DesignerViewModel()
                    {
                        Data = CurrentProject
                    };
                }
                return _designerViewModel;
            }
        }

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

        
        public IProjectRepository CurrentProject
        {
            get
            {
                if (_currentProject == null)
                {
                    var projectService = InvertGraphEditor.Container.Resolve<ProjectService>();
                    if (!String.IsNullOrEmpty(LastLoadedProject))
                    {
                        _currentProject = projectService.Projects.FirstOrDefault(p => p.Name == LastLoadedProject);
                    }
                    if (_currentProject == null) 
                    {
                        _currentProject = projectService.Projects.FirstOrDefault();
                    }
                    _designerViewModel = null;
                }
                return _currentProject;
            }
            set
            {
                var changed = _currentProject != value;

                _currentProject = value;

                _designerViewModel = null;

                if (value != null)
                {
                    if (changed)
                    {
                        OnProjectChanged();
                    }
                    LastLoadedProject = value.Name;
                    _currentProject.CurrentGraph.SetProject(_currentProject);
                }
            }
        }

        public DiagramDrawer DiagramDrawer
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

        public float Scale
        {
            get { return ElementDesignerStyles.Scale; }
            set { ElementDesignerStyles.Scale = value; }
        }

        public void RefreshContent()
        {
            
            if (DiagramDrawer != null)
            {
                DiagramDrawer.Refresh(InvertGraphEditor.PlatformDrawer);
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

        private MouseEvent _event;
        public MouseEvent MouseEvent
        {
            get { return _event ?? (_event = new MouseEvent(ModifierKeyStates, DiagramDrawer)); }
            set { _event  = value; }
        }

        public Vector2 PanStartPosition { get; set; }

        public ICommandUI Toolbar
        {
            get
            {
                if (_toolbar != null) return _toolbar;

                return _toolbar = InvertGraphEditor.CreateCommandUI<ToolbarUI>(typeof(IToolbarCommand));
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

        [MenuItem("Window/uFrame Designer", false, 1)]
        public static void Init()
        {
            // Get existing open window or if none, make a new one:
            var window = (ElementsDesigner)GetWindow(typeof(ElementsDesigner));
            window.title = "uFrame";
            //uFrameEditor.ProjectChanged += window.UFrameEditorOnProjectChanged;
            //window.DesignerViewModel = uFrameEditor.Application.Designer;
            window.wantsMouseMove = true;
            //var repo = new ElementsDataRepository();
            //var diagram = new ElementsDiagram(repo);
            //diagram.Data.ViewModels.Add(repo.GetViewModel(typeof(FPSWeaponViewModel)));
            //diagram.Data.ViewModels.Add(repo.GetViewModel(typeof(FPSBulletViewModel)));
            InvertGraphEditor.DesignerWindow = window;
            // RemoveFromDiagram when switching to add all
            window.Show();
        }

        public void CommandExecuted(IEditorCommand command)
        {
            if (DiagramDrawer != null)
            {
                //DiagramViewModel.Invalidate();
                DiagramDrawer.Refresh(InvertGraphEditor.PlatformDrawer);
                //var contextObjects = ContextObjects.ToArray();
                //foreach (var contextObject in contextObjects.OfType<DiagramNodeViewModel>())
                //{
                //    contextObject.DataObject = contextObject.DataObject;
                //    contextObject.DataObject = contextObject.DataObject;
                //}
                //DiagramViewModel.RefreshConnectors();
                //////DiagramDrawer.Refresh(InvertGraphEditor.PlatformDrawer);
                //foreach (var item in DiagramDrawer.Children)
                //{
                //    //item.RefreshContent();
                //    item.Refresh(InvertGraphEditor.PlatformDrawer, item.Bounds.position, true);
                //    //if (contextObjects.Contains(item.ViewModelObject) || item is ConnectorDrawer)
                //    //{
                //    //    item.REf
                //    //}
                //}
            }
            if (CurrentProject.CurrentGraph != null)
            {
                CurrentProject.Save();
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
                        InvertGraphEditor.ExecuteCommand(command);
                    }
                }
            }
            else if (GUILayout.Button(new GUIContent(command.Title), EditorStyles.toolbarButton))
            {
                if (command is IParentCommand)
                {
                    var contextUI = InvertGraphEditor.CreateCommandUI<ContextMenuUI>(command.GetType());
                    contextUI.Flatten = true;
                    contextUI.Go();
                }
                else
                {
                    InvertGraphEditor.ExecuteCommand(command);
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
                if (IsFocused)
                {
                    if (CurrentProject != null)
                    {
                        Selection.activeObject = CurrentProject as UnityEngine.Object;
                        EditorUtility.SetDirty(Selection.activeObject);
                    }
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
            
                var mp = (e.mousePosition) * (1f / DiagramDrawer.Scale);

                MouseEvent.MousePosition = mp;
                MouseEvent.MousePositionDelta = MouseEvent.MousePosition - MouseEvent.LastMousePosition;
                MouseEvent.MousePositionDeltaSnapped = MouseEvent.MousePosition.Snap(DiagramViewModel.SnapSize * ElementDesignerStyles.Scale) - MouseEvent.LastMousePosition.Snap(DiagramViewModel.SnapSize * ElementDesignerStyles.Scale);
                handler.OnMouseMove(MouseEvent);
                MouseEvent.LastMousePosition = mp;
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
            //if (DiagramViewModel.SelectedGraphItems.Any(p =>
            //{
            //    var viewModel = p as DiagramNodeViewModel;
            //    if (viewModel != null)
            //    {
            //        return viewModel.IsEditing;
            //    }
            //    else
            //    {
            //        var model = p as ItemViewModel;
            //        if (model != null)
            //        {
            //            return model.IsEditing;
            //        }
            //    }
            //    return false;
            //}))
            //{
            //    return;
            //}
            if (LastEvent != null)
            {
                if (LastEvent.type == EventType.keyUp)
                {
                    if (LastEvent.keyCode == KeyCode.LeftShift || LastEvent.keyCode == KeyCode.RightShift)
                        ModifierKeyStates.Shift = false;
                    if (LastEvent.keyCode == KeyCode.LeftControl || LastEvent.keyCode == KeyCode.RightControl || LastEvent.keyCode == KeyCode.LeftCommand || LastEvent.keyCode == KeyCode.RightCommand)
                        ModifierKeyStates.Ctrl = false;
                    if (LastEvent.keyCode == KeyCode.LeftAlt || LastEvent.keyCode == KeyCode.RightAlt || LastEvent.keyCode == KeyCode.LeftApple || LastEvent.keyCode == KeyCode.RightApple)
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
               
                if (DiagramViewModel != null &&
                    (DiagramViewModel.SelectedNode == null || !DiagramViewModel.SelectedNode.IsEditing))
                {
                    if (DiagramViewModel.SelectedNodeItem == null)
                    {
                        Debug.Log("Sending key command");
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
                CurrentProject.CurrentGraph.SetProject(CurrentProject);
                if (Undo.undoRedoPerformed != null) Undo.undoRedoPerformed -= UndoRedoPerformed;
                Undo.undoRedoPerformed += UndoRedoPerformed;
                //SerializedGraph = new SerializedObject(diagram as UnityEngine.Object);
                //Diagram = uFrameEditor.Container.Resolve<ElementsDiagram>();
                DiagramDrawer = new DiagramDrawer(new DiagramViewModel(diagram, CurrentProject));
                MouseEvent = new MouseEvent(ModifierKeyStates, DiagramDrawer);
                DiagramDrawer.Dirty = true;
                //DiagramDrawer.Data.ApplyFilter();
                DiagramDrawer.Refresh(InvertGraphEditor.PlatformDrawer);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Debug.Log("Either a plugin isn't installed or the file could no longer be found. See Exception error");
                LastLoadedDiagram = null;
            }
        }

        public SerializedObject SerializedGraph { get; set; }
        public bool IsFocused { get; set; }

        public void OnFocus()
        {
            _drawEveryFrame = true;
            IsFocused = true;
        }

        public void SwitchDiagram(IGraphData data)
        {
            Designer.OpenTab(data);

            LoadDiagram(CurrentProject.CurrentGraph);
            
            //CurrentProject.CurrentGraph = data as IGraphData;
            
        }

        public void OnEnable()
        {
            InvertGraphEditor.DesignerWindow = this;
        }

        public void OnGUI()
        {
            InvertGraphEditor.DesignerWindow = this;
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
            
            var rect = new Rect(0f, (EditorStyles.toolbar.fixedHeight) - 1, Screen.width - 3, Screen.height - ((EditorStyles.toolbar.fixedHeight * 2)) - EditorStyles.toolbar.fixedHeight - 2);
            
            EditorGUI.DrawRect(rect, InvertGraphEditor.Settings.BackgroundColor);
            var tabBarRect = new Rect(rect);
            tabBarRect.height = 31;
            var color = new Color(InvertGraphEditor.Settings.BackgroundColor.r*0.8f,
                InvertGraphEditor.Settings.BackgroundColor.g*0.8f, InvertGraphEditor.Settings.BackgroundColor.b*0.8f);
            EditorGUI.DrawRect(tabBarRect, color);

            if (Designer != null)
            {
                
                GUILayout.BeginHorizontal();

                foreach (var tab in Designer.Tabs.ToArray())
                {
                    var isCurrent = CurrentProject != null && CurrentProject.CurrentGraph != null && tab.GraphIdentifier == CurrentProject.CurrentGraph.Identifier;
                    if (GUILayout.Button(tab.GraphName,
                        isCurrent
                            ? ElementDesignerStyles.TabStyle
                            : ElementDesignerStyles.TabInActiveStyle,GUILayout.MinWidth(150)))
                    {
                        if (Event.current.button == 1)
                        {
                           var isLastGraph = CurrentProject.OpenGraphs.Count() <= 1;

                           if (!isLastGraph)
                            {
                                CurrentProject.CloseGraph(tab);
                                var lastGraph = CurrentProject.OpenGraphs.LastOrDefault();
                                if (isCurrent && lastGraph != null)
                                {
                                    var graph = CurrentProject.Graphs.FirstOrDefault(p => p.Identifier == lastGraph.GraphIdentifier);
                                    SwitchDiagram(graph);
                                }
                            
                            }
                        }
                        else
                        {
                            SwitchDiagram(CurrentProject.Graphs.FirstOrDefault(p => p.Identifier == tab.GraphIdentifier));    
                        }
                        
                    }

                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }



          
            rect.y += 31;
            rect.height -= 31;
            DiagramRect = rect;
            

            
            //GUI.Box(DiagramRect, String.Empty, style);
            //if (CurrentProject == null)
            //{
            //    DiagramDrawer = null;
            //    var width = 400;
            //    var height = 300;
            //    var x = (Screen.width / 2f) - (width / 2f);
            //    var y = (Screen.height / 2f) - (width / 2f);
            //    var padding = 30;
            //    var rect = new Rect(x, y, width, height);
            //    ElementDesignerStyles.DrawExpandableBox(rect, ElementDesignerStyles.NodeBackground, string.Empty);
            //    GUI.DrawTexture(new Rect(x, y + 25, width, 100), ElementDesignerStyles.GetSkinTexture("uframeLogoLarge"), ScaleMode.ScaleToFit);
            //    rect.y += 110;
            //    rect.height -= 110;
            //    GUILayout.BeginArea(rect);
            //    GUILayout.BeginArea(new Rect(padding, padding, width - (padding * 2), height - (padding * 2)));

            //    EditorGUILayout.BeginVertical();
            //    EditorGUILayout.LabelField("Create New Project", ElementDesignerStyles.ViewModelHeaderStyle);

            //    GUILayout.Space(20f);
            //    _newProjectName = EditorGUILayout.TextField("New Project Name:", _newProjectName);
            //    if (GUILayout.Button("Create Project"))
            //    {
            //        InvertGraphEditor.AssetManager.CreateAsset()
            //        UFrameAssetManager.NewUFrameProject(_newProjectName);
            //    }
            //    EditorGUILayout.EndVertical();

            //    GUILayout.EndArea();
            //    GUILayout.EndArea();
            //}
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
                _scrollPosition = GUI.BeginScrollView(DiagramRect, _scrollPosition, DiagramViewModel.DiagramBounds);

                HandlePanning(DiagramRect);
                if (DiagramViewModel != null && InvertGraphEditor.Settings.UseGrid)
                {
                    var softColor = InvertGraphEditor.Settings.GridLinesColor;
                    var hardColor = InvertGraphEditor.Settings.GridLinesColorSecondary;
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
                DiagramDrawer.Draw(InvertGraphEditor.PlatformDrawer, ElementDesignerStyles.Scale);
                HandleInput();

                if (InvertGraphEditor.Settings.ShowGraphDebug)
                {
                    GUILayout.BeginArea(new Rect(10f, 70f, 500f, 10000f));
                    GUILayout.Label(string.Format("Mouse Position: x = {0}, y = {1}", MouseEvent.MousePosition.x, MouseEvent.MousePosition.y));
                    GUILayout.Label(string.Format("Mouse Position Delta: x = {0}, y = {1}", MouseEvent.MousePositionDelta.x, MouseEvent.MousePositionDelta.y));
                    GUILayout.Label(string.Format("Mouse Down: {0}", MouseEvent.IsMouseDown));
                    GUILayout.Label(string.Format("Last Mouse Down Position: {0}", MouseEvent.LastMousePosition));
                    if (DiagramDrawer != null)
                    {
                        GUILayout.Label(string.Format("Drawer Count: {0}", DiagramDrawer.DiagramViewModel.GraphItems.Count));
                        if (DiagramDrawer.DrawersAtMouse != null)
                            foreach (var drawer in DiagramDrawer.DrawersAtMouse)
                            {
                                GUILayout.Label(drawer.ToString());
                            }
                        if (DiagramDrawer.DiagramViewModel != null)
                            foreach (var drawer in DiagramDrawer.DiagramViewModel.SelectedGraphItems)
                            {
                                GUILayout.Label(drawer.ToString());
                            }
                    }
                    foreach (var item in InvertGraphEditor.Container.Instances.Where(p=>p.Base == typeof(IConnectionStrategy)))
                    {
                        GUILayout.Label(item.Name);
                    }
                    foreach (var item in RegisteredConnectionStrategy.ConnectionTypes)
                    {
                        EditorGUILayout.LabelField(item.TOutputType.Name, item.TInputType.Name);
                    }
                    GUILayout.EndArea();
                }
              

    
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
            InvertGraphEditor.DesignerWindow = this;
            //if (DiagramViewModel != null)
            //    DiagramViewModel.DeselectAll();

            ModifierKeyStates = new ModifierKeyState();
            MouseEvent = null;
            _drawEveryFrame = false;
            IsFocused = false;
        }

        private int fpsCount = 0;
        public void Update()
        {

            if (fpsCount > 15 || (MouseEvent != null && MouseEvent.IsMouseDown))
            {
                fpsCount = 0;
                Repaint();

            }
            fpsCount++;
            //if (MouseEvent != null && MouseEvent.IsMouseDown)
            //{
            //    Repaint();
            //}
            //if (_drawEveryFrame)
            //    Repaint();
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
            foreach (var item in CurrentProject.Graphs)
            {
                IGraphData item1 = item;
                menu.AddItem(new GUIContent(item.Name), DiagramDrawer != null && CurrentProject.CurrentGraph == item1, () =>
                {
                    
                    CurrentProject.CurrentGraph = item1;
                    LoadDiagram(CurrentProject.CurrentGraph);
                });
            }
            menu.AddSeparator("");
            foreach (var graphType in InvertGraphEditor.Container.Mappings.Where(p => p.From == typeof(IGraphData)))
            {
                TypeMapping type = graphType;
                menu.AddItem(new GUIContent("Create " + graphType.Name), false, () =>
                {
                    var diagram = CurrentProject.CreateNewDiagram(type.To);
                    DiagramDrawer = null;
                    SwitchDiagram(diagram);
                   
                });
            }
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Force Refresh"), false, () => { CurrentProject.Refresh(); });

            menu.ShowAsContext();
        }

        private void SelectProject()
        {
            var projectService = InvertGraphEditor.Container.Resolve<ProjectService>();
            var projects = projectService.Projects;

            var menu = new GenericMenu();
            foreach (var project in projects)
            {
                IProjectRepository project1 = project;
                menu.AddItem(new GUIContent(project.Name), project1 == CurrentProject, () =>
                {
                    CurrentProject = project1;
                    LoadDiagram(CurrentProject.CurrentGraph);
                });
            }

            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Force Refresh"), false, () =>
            {
              
                projectService.RefreshProjects();
            });
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