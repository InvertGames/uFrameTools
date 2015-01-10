using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
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

        public bool ShowHelp
        {
            get { return true; }
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
                    Setter = (v) => property1.SetValue(GraphItem, v, null)
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
            DiagramViewModel = diagramViewModel;
            GraphItemObject = graphItemObject;
        
            OutputConnectorType = graphItemObject.GetType();
            InputConnectorType = graphItemObject.GetType();
            ToggleNode = new SimpleEditorCommand<DiagramNodeViewModel>(_ =>
            {
                this.IsCollapsed = !IsCollapsed;
            });

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
            get { return base.InputConnector; }
        }

        protected DiagramNodeViewModel()
        {

        }
        
        public ModelCollection<GraphItemViewModel> PropertyViewModels { get; set; }

        public override Vector2 Position
        {
            get
            {
                if (IsScreenshot)
                {
                    return new Vector2(45,45);
                }
                return DiagramViewModel.CurrentRepository.GetItemLocation(GraphItemObject);
                //return GraphItemObject.Location;
            }
            set
            {
                if (IsScreenshot)
                {
                    //GraphItemObject.DefaultLocation = value;
                }
                else
                {
                    DiagramViewModel.CurrentRepository.SetItemLocation(GraphItemObject, value);
                }
               
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
            //if (!IsCollapsed)
            //{
                foreach (var item in ContentItems)
                {
                    if (IsCollapsed)
                    {
                        item.GetConnectors(list);
                        if (item.InputConnector != null)
                            item.InputConnector.ConnectorFor = this;
                        if (item.OutputConnector != null)
                            item.OutputConnector.ConnectorFor = this;
                    }
                    else
                    {
                        item.GetConnectors(list);
                    
                    }
                    

                    
                    
                }
            //}
            if (InputConnector != null)
                list.Add(InputConnector);
            if (OutputConnector != null)
                list.Add(OutputConnector);

      

        }


        public bool IsCollapsed
        {
            get
            {
                if (AllowCollapsing)
                    return GraphItemObject.IsCollapsed;
                return true;

            }
            set
            {
                GraphItemObject.IsCollapsed = value;
                OnPropertyChanged("IsCollapsed");
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
            
            IsLocal = DiagramViewModel == null ||DiagramViewModel.CurrentRepository.NodeItems.Contains(GraphItemObject);
            CreateContent();
            if (GraphItemObject.IsEditing)
            {
                BeginEditing();
            }

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
                GraphItemObject.IsEditing = value;
                if (value == false)
                    EndEditing();
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

        public string SubTitle
        {
            get { return GraphItemObject.SubTitle; }
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



        public string InfoLabel
        {
            get { return GraphItemObject.InfoLabel; }
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
            if (DiagramViewModel.SelectedGraphItems.Count() < 2)
                DiagramViewModel.DeselectAll();
            base.Select();

        }

        public string editText = string.Empty;
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
            GraphItemObject.EndEditing();
            Dirty = true;
        }

        public bool Dirty { get; set; }

        public bool IsFilter
        {
            get { return InvertGraphEditor.IsFilter(GraphItemObject.GetType()) && IsLocal; }
        }

        public IEnumerable<OutputGenerator> CodeGenerators
        {
            get
            {
                return DiagramViewModel.CodeGenerators.Where(p => p.ObjectData == DataObject);
            }
        }

        public bool HasFilterItems
        {
            get
            {
                var filter = GraphItemObject as IDiagramFilter;
                if (filter == null)
                {
                    return false;
                }
                return filter.GetContainingNodes(DiagramViewModel.CurrentRepository).Any();
            }
        }

        public virtual IEnumerable<string> Tags
        {
            get { yield break; }
        }

        public virtual IEnumerable<ErrorInfo> Issues
        {
            get
            {
                yield break;
            }
        }

        public IEditorCommand ToggleNode { get; set; }


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
            

            DiagramViewModel.CurrentRepository.HideNode(GraphItemObject.Identifier);
            //DiagramViewModel.Data.CurrentFilter.Locations.Remove(GraphItemObject.Identifier);
        }


        public virtual void CtrlClicked()
        {
            InvertGraphEditor.ExecuteCommand((diagram) =>
            {
                var fileGenerator = this.CodeGenerators.OfType<CodeGenerator>().FirstOrDefault(p => !p.IsDesignerFile);
                if (fileGenerator != null)
                {
                    var filePath = fileGenerator.FullPathName;
                    //var filename = repository.GetControllerCustomFilename(this.Name);
                    InvertGraphEditor.Platform.OpenScriptFile(filePath);
                
                }
            });
        }

        public void CtrlShiftClicked()
        {
            InvertGraphEditor.ExecuteCommand((diagram) =>
            {
                var fileGenerator = this.CodeGenerators.OfType<CodeGenerator>().LastOrDefault(p => !p.IsDesignerFile);
                if (fileGenerator != null)
                {
                    var filePath = fileGenerator.FullPathName;
                    InvertGraphEditor.Platform.OpenScriptFile(filePath);
                 
                }
            });
        }



    }

    public class SectionHeaderViewModel : GraphItemViewModel
    {
        public override Vector2 Position { get; set; }
        public override string Name { get; set; }

        public IEditorCommand AddCommand { get; set; }

    }
}