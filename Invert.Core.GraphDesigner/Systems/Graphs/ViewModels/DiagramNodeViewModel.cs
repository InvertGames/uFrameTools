using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Invert.Core.GraphDesigner;
using Invert.Data;
using UnityEngine;

namespace Invert.Core.GraphDesigner
{

    public abstract class DiagramNodeViewModel<TData> : DiagramNodeViewModel where TData : IDiagramNode
    {
        protected DiagramNodeViewModel()
        {
        }

        protected DiagramNodeViewModel(TData graphItemObject, DiagramViewModel diagramViewModel)
            : base(graphItemObject, diagramViewModel)
        {

        }

        protected override void DataObjectChanged()
        {
            base.DataObjectChanged();
        }

        protected override void CreateContent()
        {

            base.CreateContent();

            foreach (var item in GraphItem.DisplayedItems)
            {
                var vm = GetDataViewModel(item);

                if (vm == null)
                {
                    InvertApplication.LogError(string.Format("Couldn't find view-model for {0}", item.GetType()));
                    continue;
                }
                vm.DiagramViewModel = DiagramViewModel;
                ContentItems.Add(vm);
            }
            AddPropertyFields();
        }

        public void AddPropertyFields(string headerText = null)
        {
            var ps = GraphItem.GetPropertiesWithAttribute<NodeProperty>().ToArray();
            if (ps.Length < 1) return;

            if (!string.IsNullOrEmpty(headerText))
                ContentItems.Add(new SectionHeaderViewModel()
                {
                    Name = headerText,
                });

            foreach (var property in ps)
            {
                PropertyInfo property1 = property.Key;
                var vm = new PropertyFieldViewModel(this)
                {
                    Type = property.Key.PropertyType,
                    Name = property.Key.Name,
                    InspectorType = property.Value.InspectorType,
                    CustomDrawerType = property.Value.CustomDrawerType,
                    Getter = () => property1.GetValue(GraphItem, null),
                    Setter = (v) =>
                    {
                        property1.SetValue(GraphItem, v, null);

                    }
                };
                ContentItems.Add(vm);
            }
        }

        protected GraphItemViewModel GetDataViewModel(IGraphItem item)
        {
            var vm = InvertGraphEditor.Container.ResolveRelation<ItemViewModel>(item.GetType(), item, this) as GraphItemViewModel;
            vm.DiagramViewModel = DiagramViewModel;

            return vm;
        }

        public TData GraphItem
        {
            get { return (TData)GraphItemObject; }
        }
    }

    public abstract class DiagramNodeViewModel : GraphItemViewModel
    {
        private bool _isSelected = false;

        public IDiagramNode GraphItemObject
        {
            get { return DataObject as IDiagramNode; }
            set { DataObject = value; }
        }

        public virtual bool IsEditable
        {
            get { return true; }
        }
        public DiagramNodeViewModel(IDiagramNode graphItemObject, DiagramViewModel diagramViewModel)
            : this()
        {
            ColumnSpan = 2;
            DiagramViewModel = diagramViewModel;
            GraphItemObject = graphItemObject;

            OutputConnectorType = graphItemObject.GetType();
            InputConnectorType = graphItemObject.GetType();
            ToggleNode = new SimpleEditorCommand<DiagramNodeViewModel>(_ =>
            {
                this.IsCollapsed = !IsCollapsed;
            });

        }

        public bool IsExternal
        {
            get { return GraphItemObject.Graph.Identifier != DiagramViewModel.GraphData.Identifier; }
        }
        public string TagsString
        {
            get { return string.Join(" | ", Tags.ToArray()); }
        }
        public virtual Type ExportGraphType
        {
            get { return null; }
        }

        public override ConnectorViewModel InputConnector
        {
            get
            {
                if (!HasInputs)
                {
                    return null;
                }
                return base.InputConnector;
            }
        }

        public override ConnectorViewModel OutputConnector
        {
            get
            {
                if (!HasOutputs)
                    return null;
                return base.OutputConnector;
            }
        }

        public virtual bool HasInputs
        {
            get { return true; }
        }
        public virtual bool HasOutputs
        {
            get { return true; }
        }
        protected override ConnectorViewModel CreateInputConnector()
        {

            return base.CreateInputConnector();
        }

        protected DiagramNodeViewModel()
        {

        }

        public ModelCollection<GraphItemViewModel> PropertyViewModels { get; set; }

        public override Vector2 Position
        {
            get
            {
                if (!DiagramViewModel.FilterItems.ContainsKey(GraphItemObject.Identifier)) return new Vector2(45, 45);
                return DiagramViewModel.FilterItems[GraphItemObject.Identifier].Position;
            }
            set
            {
                DiagramViewModel.FilterItems[GraphItemObject.Identifier].Position = value;
            }
        }

        public virtual NodeColor Color
        {
            get
            {
                return NodeColor.LightGray;
            }
        }
        //public bool Dirty { get; set; }
        public override bool IsSelected
        {
            get
            {
                return GraphItemObject.IsSelected;
            }
            set
            {
                if (value == false)
                    IsEditing = false;
                GraphItemObject.IsSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

        public override void GetConnectors(List<ConnectorViewModel> list)
        {
            // base.GetConnectors(list);

            foreach (var item in ContentItems)
            {

                item.GetConnectors(list);
                if (IsCollapsed)
                {
                    if (item.InputConnector != null)
                    {
                        item.InputConnector.Disabled = true;
                        item.InputConnector.ConnectorFor = this;
                    }

                    if (item.OutputConnector != null)
                    {
                        item.OutputConnector.Disabled = true;
                        item.OutputConnector.ConnectorFor = this;

                    }



                }
            }

            if (InputConnector != null)
                list.Add(InputConnector);
            if (OutputConnector != null)
                list.Add(OutputConnector);



        }


        public virtual bool IsCollapsed
        {
            get
            {


                if (AllowCollapsing)
                {
                    if (!DiagramViewModel.FilterItems.ContainsKey(GraphItemObject.Identifier)) return false;
                    return DiagramViewModel.FilterItems[GraphItemObject.Identifier].Collapsed;
                }

                return true;

            }
            set
            {
                if (!DiagramViewModel.FilterItems.ContainsKey(GraphItemObject.Identifier)) return;
                DiagramViewModel.FilterItems[GraphItemObject.Identifier].Collapsed = value;
                OnPropertyChanged("IsCollapsed");
                IsDirty = true;
            }
        }

        public virtual bool ShowSubtitle { get { return false; } }

        public virtual float HeaderSize
        {
            get
            {
                return 27;
            }
        }

        public virtual bool AllowCollapsing
        {
            get { return ContentItems.Count > 0; }
        }

        protected override void DataObjectChanged()
        {
            base.DataObjectChanged();
            ContentItems.Clear();

            //IsLocal = DiagramViewModel == null || DiagramViewModel.CurrentRepository.NodeItems.Contains(GraphItemObject);
            CreateContent();
            if (GraphItemObject.IsEditing)
            {
                BeginEditing();
            }
            IsDirty = true;

        }

        protected virtual void CreateContent()
        {

        }
        public bool IsLocal { get; set; }
        public bool IsEditing
        {
            get { return GraphItemObject.IsEditing; }
            set
            {
                if (value == false)
                    EndEditing();
                GraphItemObject.IsEditing = value;

            }
        }

        public string FullLabel
        {
            get { return GraphItemObject.FullLabel; }
        }

        public IEnumerable<IDiagramNodeItem> Items
        {
            get { return GraphItemObject.DisplayedItems; }
        }

        public virtual string SubTitle
        {
            get
            {
                return GraphItemObject.SubTitle;
            }
        }

        public override string Name
        {
            get { return GraphItemObject.Name; }
            set
            {
                GraphItemObject.Name = value;
                OnPropertyChanged("Name");
            }
        }

        private INodeStyleSchema _normalStyleSchema;
        private INodeStyleSchema _minimalisticStyleSchema;
        private INodeStyleSchema _boldStyleSchema;

        public virtual string HeaderBaseImage
        {
            get
            {
                return "Header3";
            }
        }

        public virtual INodeStyleSchema NormalStyleSchema
        {
            get
            {
                return _normalStyleSchema ?? (_normalStyleSchema = CachedStyles.NodeStyleSchemaNormal);
            }
            set { _normalStyleSchema = value; }
        }
        public virtual INodeStyleSchema MinimalisticStyleSchema
        {
            get
            {
                return _minimalisticStyleSchema ?? (_minimalisticStyleSchema = CachedStyles.NodeStyleSchemaMinimalistic);
            }
            set { _minimalisticStyleSchema = value; }
        }

        public virtual INodeStyleSchema BoldStyleSchema
        {
            get
            {
                return _boldStyleSchema ?? (_boldStyleSchema = CachedStyles.NodeStyleSchemaBold);
            }
            set { _boldStyleSchema = value; }
        }

        public virtual INodeStyleSchema StyleSchema
        {
            get
            {
                if (IsCurrentFilter) return BoldStyleSchema;
                return NormalStyleSchema;
            }
        }

        public virtual string IconName
        {
            get { return "CommandIcon"; }
        }

        public virtual Color IconTint
        {
            get { return HeaderColor + new Color(0.2f, 0.2f, 0.2f, -0.1f); }
        }

        public virtual Color HeaderColor
        {
            get
            {
                switch (Color)
                {
                    case NodeColor.Gray:
                        return new Color32(104, 105, 109, 255);
                        break;
                    case NodeColor.DarkGray:
                        return new Color32(56, 56, 57, 255);
                        break;
                    case NodeColor.Blue:
                        return new Color32(115, 110, 180, 255);
                        break;
                    case NodeColor.LightGray:
                        return new Color32(87, 101, 108, 255);
                        break;
                    case NodeColor.Black:
                        return new Color32(50, 50, 50, 255);
                        break;
                    case NodeColor.DarkDarkGray:
                        return new Color32(51, 56, 58, 255);
                        break;
                    case NodeColor.Orange:
                        return new Color32(235, 126, 21, 255);
                        break;
                    case NodeColor.Red:
                        return new Color32(234, 20, 20, 255);
                        break;
                    case NodeColor.Yellow:
                        return new Color32(134, 134, 18, 255);
                        break;
                    case NodeColor.Green:
                        return new Color32(25, 99, 9, 255);
                        break;
                    case NodeColor.Purple:
                        return new Color32(58, 70, 94, 255);
                        break;
                    case NodeColor.Pink:
                        return new Color32(79, 44, 115, 255);
                        break;
                    case NodeColor.YellowGreen:
                        return new Color32(197, 191, 25, 255);
                        break;
                    default:
                        return default(Color);
                        break;
                }
            }
        }

        public string Label
        {
            get { return Name; }
        }

        //public bool IsSelected
        //{
        //    get { return _isSelected; }
        //    set
        //    {
        //        SetProperty(ref _isSelected, value, IsSelectedProperty);
        //    }
        //}
        public virtual Type CommandsType
        {
            get { return typeof(IDiagramNode); }
        }

        public override string ToString()
        {
            return GraphItemObject.Identifier;
        }

        public override void Select()
        {

            DiagramViewModel.Select(this);
            base.Select();


        }

        public override bool Enabled
        {
            get { return !this.IsExternal; }
        }


        public string editText = string.Empty;
        private string _headerBaseImage;

        public void Rename(string newText)
        {

            GraphItemObject.Rename(newText);

        }

        public void EndEditing()
        {
            if (!IsEditable) return;
            if (string.IsNullOrEmpty(GraphItemObject.Name))
            {
                GraphItemObject.Name = "RenameMe";
            }
            
            if (!IsEditing) return;
            var n = GraphItemObject.Name;
            GraphItemObject.Name = "_";

            InvertApplication.Execute(new LambdaCommand("Rename Item", () =>
            {
                GraphItemObject.Rename(n);
                GraphItemObject.EndEditing();
            }));
            InvertApplication.SignalEvent<INodeItemEvents>(_ => _.Renamed(GraphItemObject, editText, GraphItemObject.Name));
            Dirty = true;
        }

        public bool Dirty { get; set; }

        public bool IsFilter
        {
            get { return InvertGraphEditor.IsFilter(GraphItemObject.GetType()); }
        }

        public IEnumerable<OutputGenerator> CodeGenerators
        {
            get
            {
                return InvertGraphEditor.GetAllCodeGenerators(InvertGraphEditor.Container.Resolve<IGraphConfiguration>(),
                    new IDataRecord[] { GraphItemObject });
            }
        }

        public bool HasFilterItems
        {
            get
            {
                var filter = GraphItemObject as IGraphFilter;
                if (filter == null)
                {
                    return false;
                }
                return filter.FilterItems.Any();
            }
        }
        public IEnumerable<IDiagramNode> ContainedItems
        {
            get
            {
                var filter = GraphItemObject as IGraphFilter;
                if (filter == null)
                {
                    yield break;
                }
                foreach (var item in filter.FilterNodes)
                {
                    yield return item;
                }
            }
        }
        public virtual IEnumerable<string> Tags
        {
            get { yield break; }
        }

        public virtual ErrorInfo[] Issues
        {
            get { return GraphItemObject.Errors; }
        }

        public IEditorCommand ToggleNode { get; set; }
        public bool SaveImage { get; set; }

        public bool IsCurrentFilter
        {
            get { return GraphItemObject.Graph.CurrentFilter == GraphItemObject; }

        }

      
        public void BeginEditing()
        {
            if (!IsEditable) return;
            editText = Name;
            GraphItemObject.BeginEditing();

        }

        public void Remove()
        {
            GraphItemObject.RemoveFromDiagram();
        }

        public void Hide()
        {
            DiagramViewModel.GraphData.CurrentFilter.HideInFilter(GraphItemObject);
        }


        public virtual void CtrlClicked()
        {
            InvertApplication.Execute(() =>
            {
                var fileGenerator = this.CodeGenerators.OfType<CodeGenerator>().FirstOrDefault(p => !p.IsDesignerFile);
                if (fileGenerator != null)
                {
                    var filePath = fileGenerator.FullPathName;
                    //var filename = repository.GetControllerCustomFilename(this.Name);
                    InvertGraphEditor.Platform.OpenScriptFile("Assets" + fileGenerator.UnityPath);

                }
            });
        }

        public void CtrlShiftClicked()
        {
            InvertApplication.Execute(() =>
            {
                var fileGenerator = this.CodeGenerators.OfType<CodeGenerator>().LastOrDefault(p => !p.IsDesignerFile);
                if (fileGenerator != null)
                {

                    InvertGraphEditor.Platform.OpenScriptFile("Assets" + fileGenerator.UnityPath);

                }
            });
        }


    }

    public class InspectorViewModel : GraphItemViewModel
    {
        private GraphItemViewModel _targetViewModel;

        public bool Visible
        {
            get { return DataObject != null; }
        }

        public GraphItemViewModel TargetViewModel
        {
            get { return _targetViewModel; }
            set
            {
                _targetViewModel = value;
                if (_targetViewModel != null)
                {
                    DataObject = value.DataObject;
                }
                else
                {
                    DataObject = null;
                }
            }
        }

        public override Vector2 Position { get; set; }
        public override string Name { get; set; }

        protected override void DataObjectChanged()
        {
            base.DataObjectChanged();
            AddPropertyFields();
            IsDirty = true;

        }
        public void AddPropertyFields(string headerText = null)
        {
            if (DataObject == null) return;
            ContentItems.Clear();
            var ps = DataObject.GetPropertiesWithAttribute<InspectorProperty>().ToArray();
            if (ps.Length < 1) return;

            if (!string.IsNullOrEmpty(headerText))
                ContentItems.Add(new SectionHeaderViewModel()
                {
                    Name = headerText,
                });
            var data = DataObject;
            foreach (var property in ps)
            {
                PropertyInfo property1 = property.Key;
                var vm = new PropertyFieldViewModel()
                {
                    Type = property.Key.PropertyType,
                    Name = property.Key.Name,
                    InspectorType = property.Value.InspectorType,
                    CustomDrawerType = property.Value.CustomDrawerType,
                    Getter = () => property1.GetValue(data, null),
                    Setter = (v) =>
                    {

                        property1.SetValue(data, v, null);

                    }
                };
                ContentItems.Add(vm);
            }
            IsDirty = true;
        }
    }

    public class SectionHeaderViewModel : GraphItemViewModel
    {
        public override Vector2 Position { get; set; }
        public override string Name { get; set; }

        public IEditorCommand AddCommand { get; set; }

    }
}