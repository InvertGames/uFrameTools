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

        public ElementDiagramSettings Settings
        {
            get { return DiagramData.Settings; }
        }

        public IEnumerable<GraphItemViewModel> SelectedGraphItems
        {
            get
            {
                foreach (var item in GraphItems)
                {
                    if (item.IsSelected)
                    {
                        foreach (var child in item.ContentItems)
                        {
                            if (child.IsSelected) yield return child;
                        }

                        yield return item;
                    }

                }
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

        public IEnumerable<CodeGenerator> CodeGenerators
        {
            get
            {
                return InvertGraphEditor.GetAllCodeGenerators(null,
                    InvertGraphEditor.CurrentProject);
            }
        }

        protected override void DataObjectChanged()
        {
            base.DataObjectChanged();
            GraphItems.Clear();

            //foreach (var item in Data.GetDiagramItems())
            //{
            //    uFrameEditor.Container.ResolveRelation(item.GetType(), typeof(ViewModel));
            //}
            //_graphItems.CollectionChangedWith += GraphItemsChanged;

        }

        //private void GraphItemsChanged(ModelCollectionChangeEventWith<GraphItemViewModel> changeArgs)
        //{
        //    foreach (var graphItem in changeArgs.NewItemsOfT)
        //    {
        //        GraphItemAdded(graphItem);
        //    }
        //    foreach (var graphItem in changeArgs.OldItemsOfT)
        //    {
        //        GraphItemRemoved(graphItem);
        //    }
        //}

        //private void GraphItemAdded(GraphItemViewModel graphItem)
        //{

        //}



        //private void GraphItemRemoved(GraphItemViewModel graphItem)
        //{

        //}

        public IProjectRepository CurrentRepository { get; set; }

        public DiagramViewModel(IGraphData diagram, IProjectRepository currentRepository)
        {

           // var assetPath = InvertGraphEditor.Platform.GetAssetPath(diagram as GraphData);
            if (diagram == null) throw new Exception("Diagram not found");
            CurrentRepository = currentRepository;
            DataObject = diagram;




            DiagramData.Prepare();

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
                    if (InvertGraphEditor.Platform.MessageBox("Node Error", string.Format("Couldn't find view-model for {0} would you like to remove this item?", item.GetType()),"Yes","No"))
                    {
                        CurrentRepository.RemoveNode(item);
                    }
                    continue;
                }
                GraphItems.Add(vm);
                // Clear the connections on the view-model
                vm.Connectors.Clear();
                vm.GetConnectors(vm.Connectors);
                connectors.AddRange(vm.Connectors);

            }
            foreach (var item in connectors)
            {
                GraphItems.Add(item);
            }

            var connections = new List<ConnectionViewModel>();
            var connectorInfo = new ConnectorInfo(connectors.ToArray(), this, CurrentRepository);
            foreach (var strategy in InvertGraphEditor.ConnectionStrategies)
            {
                strategy.GetConnections(connections, connectorInfo);
            }

            foreach (var item in connections)
            {
                GraphItems.Add(item);
                item.ConnectorA.HasConnections = true;
                item.ConnectorB.HasConnections = true;
            }
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
            get
            {
                if (CurrentRepository.CurrentFilter == CurrentRepository.CurrentGraph.RootFilter)
                    return DiagramData.Name;
                return CurrentRepository.CurrentFilter.Name;
            }
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
            CurrentRepository.SaveDiagram(DiagramData);
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
            foreach (var item in SelectedGraphItems)
            {
                item.IsSelected = false;
            }
            foreach (var item in GraphItems.OfType<DiagramNodeViewModel>())
            {
                item.EndEditing();
            }
        }

        //public void UpgradeProject()
        //{
        //    uFrameEditor.ExecuteCommand(new ConvertToJSON());
        //}

        public void NothingSelected()
        {
            DeselectAll();
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

            newNodeData.Name =
                InvertGraphEditor.CurrentProject.GetUniqueName("New" + newNodeData.GetType().Name.Replace("Data", ""));
            CurrentRepository.SetItemLocation(newNodeData, InvertGraphEditor.CurrentMouseEvent.MouseUpPosition);
            CurrentRepository.AddNode(newNodeData);

            var filterNode = CurrentRepository.CurrentFilter as IDiagramNode;
            if (filterNode != null)
            {
                newNodeData.Name = filterNode.Name +
                                   CurrentRepository.GetUniqueName(newNodeData.GetType().Name.Replace("Data", ""));
                filterNode.NodeAddedInFilter(newNodeData);
            }

        }
    }
}