using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace Invert.Core.GraphDesigner
{
    public class DesignerViewModel : ViewModel<IProjectRepository>
    {
        private ModelCollection<TabViewModel> _tabs;

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
                    foreach (var item in DiagramData.Validate())
                    {
                        yield return item;
                    }
                }

            }
        }
        public float SnapSize
        {
            get { return Settings.SnapSize * Scale; }
        }
        public ElementDiagramSettings Settings
        {
            get { return DiagramData.Settings; }
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
            DiagramData.FilterState.FilterStack.Clear();
            DiagramData.FilterState._persistedFilterStack.Clear();
            var path = node.FilterPath();
            foreach (var item in path.Skip(1))
            {


                DiagramData.PushFilter(item);
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

        public IGraphData DiagramData
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
            DiagramData.Prepare();

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

            CurrentNodes = DiagramData.CurrentFilter.FilterItems(CurrentRepository).ToArray();

            foreach (var item in CurrentNodes)
            {

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


            foreach (var item in connectors)
            {
                item.DiagramViewModel = this;
                GraphItems.Add(item);
            }
            //  var startTime = DateTime.Now;


            foreach (var connection in DiagramData.Connections)
            {
                var startConnector = connectors.FirstOrDefault(p => p.DataObject == connection.Output && p.Direction == ConnectorDirection.Output);
                var endConnector = connectors.FirstOrDefault(p => p.DataObject == connection.Input && p.Direction == ConnectorDirection.Input);
                if (startConnector == null || endConnector == null) continue;

                startConnector.HasConnections = true;
                endConnector.HasConnections = true;
                GraphItems.Add(new ConnectionViewModel(this)
                {
                    ConnectorA = endConnector,
                    ConnectorB = startConnector,
                    Color = Color.white,
                    Remove = (a) =>
                    {
                        DiagramData.RemoveConnection(connection.Output, connection.Input);
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

        public IDiagramNode[] CurrentNodes { get; set; }

        public ModelCollection<GraphItemViewModel> GraphItems
        {
            get { return _graphItems; }
            set { _graphItems = value; }
        }

        public int RefactorCount
        {
            get { return DiagramData.RefactorCount; }
        }

        public string Title
        {
            get { return CurrentRepository.CurrentFilter.Name; }
        }

        public bool HasErrors
        {
            get { return DiagramData.Errors; }
        }

        public Exception Errors
        {
            get { return DiagramData.Error; }
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
                if (SelectedNode.GraphItemObject == DiagramData.CurrentFilter)
                {
                    DiagramData.PopFilter(null);
                    DiagramData.UpdateLinks();
                }
                else
                {
                    DiagramData.PushFilter(SelectedNode.GraphItemObject as IDiagramFilter);
                    DiagramData.UpdateLinks();
                }
            }
        }

        public void Save()
        {
            CurrentRepository.Save();
        }

        public void MarkDirty()
        {
            CurrentRepository.MarkDirty(DiagramData);
        }

        public void RecordUndo(string title)
        {
            CurrentRepository.RecordUndo(DiagramData, title);
        }

        public void DeselectAll()
        {
            foreach (var item in AllViewModels.OfType<ItemViewModel>())
            {
                item.IsEditing = false;
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
                        item.IsEditing = false;
                        
                    }
                    
                });
            }

            DeselectAll();
            InvertGraphEditor.ExecuteCommand(_=>{});
        }

        public void Select(GraphItemViewModel viewModelObject)
        {
            //if (SelectedGraphItems.Count() <= 1)
            //    DeselectAll();

            viewModelObject.IsSelected = true;
        }

        public IEnumerable<IDiagramNode> GetImportableItems()
        {
            return CurrentRepository.GetImportableItems(DiagramData.CurrentFilter);
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
            AddNode(newNodeData, DiagramDrawer.LastMouseEvent.MouseDownPosition);
        }

        public void AddNode(IDiagramNode newNodeData, Vector2 position)
        {
            newNodeData.Graph = DiagramData;
            newNodeData.Name =
                CurrentRepository.GetUniqueName("New" + newNodeData.GetType().Name.Replace("Data", ""));
            CurrentRepository.SetItemLocation(newNodeData, position);
            CurrentRepository.AddNode(newNodeData);

            var filterNode = CurrentRepository.CurrentFilter as IDiagramNode;
            if (filterNode != null)
            {
                newNodeData.Name = filterNode.Name +
                                   CurrentRepository.GetUniqueName(newNodeData.GetType().Name.Replace("Data", ""));
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
    }
}