using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

namespace Invert.Core.GraphDesigner
{
    public class DesignerViewModel : ViewModel<IProjectRepository>
    {
        private ModelCollection<TabViewModel> _tabs;
        private IToolbarCommand[] _allCommands;

        protected override void DataObjectChanged()
        {
            base.DataObjectChanged();
            //Tabs = Data.OpenGraphs;
        }

        public TabViewModel CurrentTab { get; set; }

        public IEnumerable<OpenGraph> Tabs
        {
            get { return Data.OpenGraphs; }
        }

        public void OpenTab(IGraphData graphData, string[] path = null)
        {

            Data.CurrentGraph = graphData;


        }

        public IToolbarCommand[] AllCommands
        {
            get { return _allCommands ?? (_allCommands = InvertGraphEditor.Container.ResolveAll<IToolbarCommand>().ToArray()); }
            set { _allCommands = value; }
        }



    }

    public class TabViewModel : ViewModel
    {
        public string Name { get; set; }
        public string[] Path { get; set; }
        public Rect Bounds { get; set; }
        public IGraphData Graph { get; set; }
    }

    public class DiagramViewModel : Invert.Core.GraphDesigner.ViewModel
    {
        private ModelCollection<GraphItemViewModel> _graphItems = new ModelCollection<GraphItemViewModel>();
        private IEnumerable<IDiagramContextCommand> _diagramContextCommands;
        private ModelCollection<GraphItemViewModel> _inspectorItems = new ModelCollection<GraphItemViewModel>();

        public IEnumerable<ErrorInfo> Issues
        {
            get
            {
                var project = CurrentRepository as IProjectRepository;
                if (project != null)
                {
                    foreach (var graph in project.Graphs)
                    {
                        if (graph != null)
                        {
                            foreach (var item in graph.Validate())
                            {
                                yield return item;
                            }
                        }
                    }
                }
                else
                {
                    foreach (var item in GraphData.Validate())
                    {
                        yield return item;
                    }
                }

            }
        }
        public float SnapSize
        {
            get
            {


                return Settings.SnapSize * Scale;
            }
        }
        public ElementDiagramSettings Settings
        {
            get { return GraphData.Settings; }
        }

        public IEnumerable<IDiagramContextCommand> DiagramContextCommands
        {
            get { return _diagramContextCommands ?? (_diagramContextCommands = InvertGraphEditor.Container.ResolveAll<IDiagramContextCommand>()); }
            set { _diagramContextCommands = value; }
        }

        public IEnumerable<GraphItemViewModel> AllViewModels
        {
            get
            {
                foreach (var item in GraphItems)
                {

                    foreach (var child in item.ContentItems)
                    {
                        yield return child;
                    }
                    yield return item;
                }
            }
        }
        public IEnumerable<GraphItemViewModel> SelectedGraphItems
        {
            get { return AllViewModels.Where(p => p.IsSelected); }
        }

        public void NavigateTo(IDiagramNode node)
        {
            //for (var i = 0; i < DiagramData.FilterState.FilterStack.Count; i++)
            //{
            //    DiagramData.PopFilter(null);

            //}
            GraphData.FilterState.FilterStack.Clear();
            GraphData.FilterState._persistedFilterStack.Clear();
            var path = node.FilterPath();
            foreach (var item in path.Skip(1))
            {


                GraphData.PushFilter(item);
            }
            node.IsSelected = true;
        }

        public float Scale
        {
            get { return InvertGraphEditor.DesignerWindow.Scale; }
        }
        public Rect DiagramBounds
        {
            get
            {
                Rect size = new Rect();
                foreach (var diagramItem in GraphItems)
                {
                    var rect = diagramItem.Bounds.Scale(Scale);

                    if (rect.x < 0)
                        rect.x = 0;
                    if (rect.y < 0)
                        rect.y = 0;
                    //if (rect.x < size.x)
                    //{
                    //    size.x = rect.x;
                    //}
                    //if (rect.y < size.y)
                    //{
                    //    size.y = rect.y;
                    //}
                    if (rect.x + rect.width > size.x + size.width)
                    {
                        size.width = rect.x + rect.width;
                    }
                    if (rect.y + rect.height > size.y + size.height)
                    {
                        size.height = rect.y + rect.height;
                    }
                }
                size.height += 400f;
                size.width += 400f;
#if UNITY_DLL
                if (size.height < Screen.height)
                {
                    size.height = Screen.height;
                }
                if (size.width < Screen.width)
                {
                    size.width = Screen.width;
                }
#endif
                return size; ;
            }

        }

        public GraphItemViewModel SelectedGraphItem
        {
            get { return SelectedGraphItems.FirstOrDefault(); }
        }

        public DiagramNodeViewModel SelectedNode
        {
            get { return SelectedGraphItems.OfType<DiagramNodeViewModel>().FirstOrDefault(); }
        }

        public IEnumerable<GraphItemViewModel> SelectedNodeItems
        {
            get
            {
                return GraphItems.OfType<DiagramNodeViewModel>().SelectMany(p => p.ContentItems).Where(p => p.IsSelected);
            }
        }

        public GraphItemViewModel SelectedNodeItem
        {
            get { return SelectedNodeItems.FirstOrDefault(); }
        }

        public IGraphData GraphData
        {
            get { return DataObject as IGraphData; }
        }

        public IEnumerable<OutputGenerator> CodeGenerators
        {
            get
            {
                return InvertGraphEditor.GetAllCodeGenerators(null,
                    CurrentRepository);
            }
        }

        protected override void DataObjectChanged()
        {
            base.DataObjectChanged();
            GraphItems.Clear();

        }


        public INodeRepository CurrentRepository { get; set; }

        public DiagramViewModel(IGraphData diagram, INodeRepository currentRepository)
        {

            if (diagram == null) throw new Exception("Diagram not found");
            CurrentRepository = currentRepository;
            DataObject = diagram;
            GraphData.Prepare();

        }

        public void Invalidate()
        {
            foreach (var item in GraphItems)
            {
                item.IsDirty = true;
            }
        }

        public void Load()
        {
            GraphItems.Clear();
            var connectors = new List<ConnectorViewModel>();

            CurrentNodes = GraphData.CurrentFilter.FilterItems(CurrentRepository).ToArray();


            LoadInspector();

            foreach (var item in CurrentNodes)
            {
                if (!GraphData.DocumentationMode && item is ScreenshotNode) continue;
                // Get the ViewModel for the data
                var vm =
                    InvertApplication.Container.ResolveRelation<ViewModel>(item.GetType(), item, this) as
                        GraphItemViewModel;
                if (vm == null)
                {
                    if (InvertGraphEditor.Platform.MessageBox("Node Error", string.Format("Couldn't find view-model for {0} would you like to remove this item?", item.GetType()), "Yes", "No"))
                    {
                        CurrentRepository.RemoveNode(item);
                    }
                    continue;
                }
                vm.DiagramViewModel = this;
                GraphItems.Add(vm);
                // Clear the connections on the view-model
                vm.Connectors.Clear();
                vm.GetConnectors(vm.Connectors);
                connectors.AddRange(vm.Connectors);
            }
            RefreshConnectors(connectors);
        }

        public void LoadInspector()
        {
            InspectorItems.Clear();

            //var graphsHeader = new SectionHeaderViewModel()
            //{
            //    Name = "Graphs",
            //    DataObject = DiagramData,
            //};

            // InspectorItems.Add(graphsHeader);
            var vms = new List<ViewModel>();
            foreach (var item in SelectedGraphItems)
            {
                //var header = new SectionHeaderViewModel()
                //{
                //    Name = item.Name,
                //    DataObject = item
                //};

                //InspectorItems.Add(header);
                item.GetInspectorOptions(vms);
            }
            foreach (var item in vms.OfType<GraphItemViewModel>())
            {
                InspectorItems.Add(item);
            }

        }

        public void RefreshConnectors()
        {

            var items = GraphItems.OfType<ConnectorViewModel>().ToArray();
            var connections = GraphItems.OfType<ConnectionViewModel>().ToArray();

            foreach (var item in items)
            {
                GraphItems.Remove(item);
            }
            foreach (var item in connections)
            {
                GraphItems.Remove(item);
            }
            var connectors = new List<ConnectorViewModel>();
            foreach (var item in GraphItems)
            {
                item.GetConnectors(connectors);
            }
            RefreshConnectors(connectors);
        }
        private void RefreshConnectors(List<ConnectorViewModel> connectors)
        {

            var strategies = InvertGraphEditor.ConnectionStrategies;

            var outputs = new List<ConnectorViewModel>();
            var inputs = new List<ConnectorViewModel>();


            foreach (var item in connectors)
            {
                item.DiagramViewModel = this;
                GraphItems.Add(item);
                if (item.Direction == ConnectorDirection.Output)
                {
                    outputs.Add(item);
                }
                else
                {
                    inputs.Add(item);
                }
            }
          
            foreach (var output in outputs)
            {
                foreach (var input in inputs)
                {
                    foreach (var strategy in strategies)
                    {
                        if (strategy.IsConnected(output, input))
                        {
                            var strategy1 = strategy;
                            var output1 = output;
                            var input1 = input;
                            output.HasConnections = true;
                            input.HasConnections = true;
                            GraphItems.Add(new ConnectionViewModel(this)
                            {
                                ConnectorA = output,
                                ConnectorB = input,
                                Color = strategy.ConnectionColor,
                                Remove = (a) =>
                                {
                                    strategy1.Remove(output1, input1);
                                }
                            });
                        }
                    }
                }
            }

            foreach (var connection in CurrentRepository.Connections)
            {
                var startConnector = connectors.FirstOrDefault(p => p.DataObject == connection.Output && p.Direction == ConnectorDirection.Output);
                var endConnector = connectors.FirstOrDefault(p => p.DataObject == connection.Input && p.Direction == ConnectorDirection.Input);


                if (startConnector == null || endConnector == null) continue;

                var vm = endConnector.ConnectorFor.DataObject as IDiagramNodeItem;

                startConnector.HasConnections = true;
                endConnector.HasConnections = true;
                GraphItems.Add(new ConnectionViewModel(this)
                {
                    ConnectorA = endConnector,
                    ConnectorB = startConnector,
                    Color = vm != null ? GetColor(vm) : Color.white,
                    Remove = (a) =>
                    {
                        GraphData.RemoveConnection(connection.Output, connection.Input);
                    }
                });
            }
            //var endTime = DateTime.Now;
            //var diff = new TimeSpan(endTime.Ticks - startTime.Ticks);
            //Debug.Log(string.Format("{0} took {1} seconds {2} milliseconds", "New Strategy", diff.Seconds, diff.Milliseconds));

            //var connections = new List<ConnectionViewModel>();
            //var connectorInfo = new ConnectorInfo(connectors.ToArray(), this, CurrentRepository);
            //foreach (var strategy in InvertGraphEditor.ConnectionStrategies)
            //{
            //    var startTime = DateTime.Now;
            //    strategy.GetConnections(connections, connectorInfo);
            //    var endTime = DateTime.Now;
            //    var diff = new TimeSpan(endTime.Ticks - startTime.Ticks);
            //    Debug.Log(string.Format("{0} took {1} seconds {2} milliseconds", strategy.GetType().Name, diff.Seconds, diff.Milliseconds));
            //}

            //foreach (var item in connections)
            //{
            //    GraphItems.Add(item);
            //    item.ConnectorA.HasConnections = true;
            //    item.ConnectorB.HasConnections = true;
            //}
        }
        public Color GetColor(IGraphItem dataObject)
        {
            var item = dataObject as IDiagramNodeItem;
            if (item != null)
            {
                var node = item.Node as GenericNode;
                if (node != null)
                {
                    var color = node.Config.GetColor(node);
                    switch (color)
                    {
                        case NodeColor.Black:
                            return Color.black;
                        case NodeColor.Blue:
                            return new Color(0.25f, 0.25f, 0.65f);
                        case NodeColor.DarkDarkGray:
                            return new Color(0.25f, 0.25f, 0.25f);
                        case NodeColor.DarkGray:
                            return new Color(0.45f, 0.45f, 0.45f);
                        case NodeColor.Gray:
                            return new Color(0.65f, 0.65f, 0.65f);
                        case NodeColor.Green:
                            return new Color(0.00f, 1f, 0f);
                        case NodeColor.LightGray:
                            return new Color(0.75f, 0.75f, 0.75f);
                        case NodeColor.Orange:
                            return new Color(0.059f, 0.98f, 0.314f);
                        case NodeColor.Pink:
                            return new Color(0.059f, 0.965f, 0.608f);
                        case NodeColor.Purple:
                            return new Color(0.02f, 0.318f, 0.659f);
                        case NodeColor.Red:
                            return new Color(1f, 0f, 0f);
                        case NodeColor.Yellow:
                            return new Color(1f, 0.8f, 0f);
                        case NodeColor.YellowGreen:
                            return new Color(0.604f, 0.804f, 0.196f);

                    }

                }
            }
            return Color.white;
        }
        public IDiagramNode[] CurrentNodes { get; set; }

        public ModelCollection<GraphItemViewModel> InspectorItems
        {
            get { return _inspectorItems; }
            set { _inspectorItems = value; }
        }
        public ModelCollection<GraphItemViewModel> GraphItems
        {
            get { return _graphItems; }
            set { _graphItems = value; }
        }

        public int RefactorCount
        {
            get { return GraphData.RefactorCount; }
        }

        public string Title
        {
            get { return CurrentRepository.CurrentFilter.Name; }
        }

        public bool HasErrors
        {
            get { return GraphData.Errors; }
        }

        public Exception Errors
        {
            get { return GraphData.Error; }
        }

        public bool NeedsUpgrade
        {
            get
            {
                return false;
                //return string.IsNullOrEmpty(DiagramData.Version) || (Convert.ToDouble(DiagramData.Version) < uFrameVersionProcessor.CURRENT_VERSION_NUMBER && uFrameVersionProcessor.REQUIRE_UPGRADE);
            }
        }

        public void Navigate()
        {
            if (SelectedNode == null) return;
            if (SelectedNode.IsFilter)
            {
                if (SelectedNode.GraphItemObject == GraphData.CurrentFilter)
                {
                    GraphData.PopFilter(null);
                    GraphData.UpdateLinks();
                }
                else
                {
                    GraphData.PushFilter(SelectedNode.GraphItemObject as IDiagramFilter);
                    GraphData.UpdateLinks();
                }
            }
        }

        public void Save()
        {
            CurrentRepository.Save();
        }

        public void MarkDirty()
        {
            CurrentRepository.MarkDirty(GraphData);
        }

        public void RecordUndo(string title)
        {
            CurrentRepository.RecordUndo(GraphData, title);
        }

        public void DeselectAll()
        {
            foreach (var item in AllViewModels.OfType<ItemViewModel>())
            {
                item.EndEditing();
            }
            foreach (var item in SelectedGraphItems)
            {
                item.IsSelected = false;
            }

            foreach (var item in GraphItems.OfType<DiagramNodeViewModel>())
            {
                item.EndEditing();
            }
#if UNITY_DLL
            UnityEngine.GUI.FocusControl("");
#endif
        }

        //public void UpgradeProject()
        //{
        //    uFrameEditor.ExecuteCommand(new ConvertToJSON());
        //}

        public void NothingSelected()
        {
            var items = SelectedNodeItems.OfType<ItemViewModel>().Where(p => p.IsEditing).ToArray();
            if (items.Length > 0)
            {
                InvertGraphEditor.ExecuteCommand(_ =>
                {
                    foreach (var item in items)
                    {
                        item.EndEditing();

                    }

                });
            }

            DeselectAll();
            InvertGraphEditor.ExecuteCommand(_ => { });
        }

        public void Select(GraphItemViewModel viewModelObject)
        {
            if (viewModelObject.IsSelected)
            {
                return;
            }
            if (!LastMouseEvent.ModifierKeyStates.Alt)
                DeselectAll();

            viewModelObject.IsSelected = true;
            InvertApplication.SignalEvent<IGraphSelectionEvents>(
                _ => _.SelectionChanged(viewModelObject));
        }

        public IEnumerable<IDiagramNode> GetImportableItems()
        {
            return CurrentRepository.GetImportableItems(GraphData.CurrentFilter);
        }

        public void UpgradeProject()
        {
            InvertGraphEditor.ExecuteCommand((n) =>
            {
                Process15Uprade();
            });

        }

        public void Process15Uprade()
        {

        }

        public void AddNode(IDiagramNode newNodeData)
        {
            AddNode(newNodeData, LastMouseEvent.MouseDownPosition);
        }

        public MouseEvent LastMouseEvent { get; set; }

        public void AddNode(IDiagramNode newNodeData, Vector2 position)
        {
            newNodeData.Graph = GraphData;
            if (string.IsNullOrEmpty(newNodeData.Name))
                newNodeData.Name =
                    CurrentRepository.GetUniqueName("New" + newNodeData.GetType().Name.Replace("Data", ""));
            CurrentRepository.SetItemLocation(newNodeData, position);
            CurrentRepository.AddNode(newNodeData);

            var filterNode = CurrentRepository.CurrentFilter as IDiagramNode;
            if (filterNode != null)
            {
                //if (string.IsNullOrEmpty(newNodeData.Name))
                //newNodeData.Name = filterNode.Name +
                //                   CurrentRepository.GetUniqueName(newNodeData.GetType().Name.Replace("Data", ""));
                filterNode.NodeAddedInFilter(newNodeData);
            }

        }

        public IEnumerable<object> ContextObjects
        {
            get
            {
                yield return this;
                yield return DataObject;

                foreach (var nodeItem in GraphItems.Where(p => p.IsMouseOver || p.IsSelected).OfType<ConnectorViewModel>())
                {
                    yield return nodeItem;
                    yield return nodeItem.DataObject;

                }
                foreach (var nodeItem in GraphItems.SelectMany(p => p.ContentItems).Where(p => p.IsMouseOver || p.IsSelected))
                {
                    if (nodeItem is DiagramNodeViewModel) continue;
                    yield return nodeItem;
                    yield return nodeItem.DataObject;

                }
                foreach (var nodeItem in GraphItems.Where(p => p.IsMouseOver || p.IsSelected).OfType<DiagramNodeViewModel>())
                {
                    yield return nodeItem;
                    yield return nodeItem.DataObject;

                }
                //foreach (var nodeItem in SelectedNodeItems)
                //{
                //    yield return nodeItem;
                //    yield return nodeItem.DataObject;
                //}
                //foreach (var node in SelectedGraphItems.OfType<DiagramNodeViewModel>())
                //{
                //    yield return node;
                //    yield return node.DataObject;
                //}
                //foreach (var item in GraphItems)
                //{
                //    if (!item.IsMouseOver && !item.IsSelected) continue;
                //    yield return item;
                //    yield return item.DataObject;
                //}
            }
        }

        public void CommandExecuted(IEditorCommand command)
        {
            this.Load();
        }

        public void CommandExecuting(IEditorCommand command)
        {

        }

        public void NavigateTo(string identifier)
        {
            var graphItem = CurrentRepository.AllGraphItems.OfType<IDiagramNodeItem>().FirstOrDefault(p => p.Identifier == identifier);
            if (graphItem == null) return;
            var node = graphItem.Node;
            NavigateTo(node);
        }

        public void ShowQuickAdd()
        {
            var mousePosition = LastMouseEvent.MouseDownPosition;
            var items = InvertApplication.Plugins.OfType<IPrefabNodeProvider>().SelectMany(p => p.PrefabNodes(CurrentRepository)).ToArray();

            InvertGraphEditor.WindowManager.InitItemWindow(items, _ =>
            {
                _.Diagram = this;
                _.MousePosition = mousePosition;
                _.Action(_);
            });
        }

        public void ShowContainerDebug()
        {
            var mousePosition = LastMouseEvent.MouseDownPosition;
            var items = InvertApplication.Container.Instances.Select(p => new DefaultItem(string.Format("{0} : {1}", p.Name, p.Instance.GetType().Name), p.Base.Name));

            InvertGraphEditor.WindowManager.InitItemWindow(items, _ =>
            {

            });
        }
    }

    public interface IGraphSelectionEvents
    {
        void SelectionChanged(GraphItemViewModel selected);
    }
}