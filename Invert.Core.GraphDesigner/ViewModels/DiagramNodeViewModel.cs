using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Invert.Common;
using Invert.uFrame.Editor.ViewModels;
using UnityEditor;
using UnityEngine;

namespace Invert.Core.GraphDesigner
{

    public abstract class DiagramNodeViewModel<TData> : DiagramNodeViewModel where TData : IDiagramNode
    {
        protected DiagramNodeViewModel()
        {
        }

        public DiagramNodeViewModel(TData graphItemObject, DiagramViewModel diagramViewModel)
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
                    Debug.LogError(string.Format("Couldn't find view-model for {0}", item.GetType()));
                    continue;
                }
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
        public DiagramViewModel DiagramViewModel { get; set; }
        public DiagramNodeViewModel(IDiagramNode graphItemObject, DiagramViewModel diagramViewModel)
            : this()
        {
            GraphItemObject = graphItemObject;
            DiagramViewModel = diagramViewModel;
            OutputConnectorType = graphItemObject.GetType();
            InputConnectorType = graphItemObject.GetType();

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
                return DiagramViewModel.CurrentRepository.GetItemLocation(GraphItemObject);
                //return GraphItemObject.Location;
            }
            set
            {
                DiagramViewModel.CurrentRepository.SetItemLocation(GraphItemObject, value);
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
            set { GraphItemObject.IsCollapsed = value; }
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
            IsLocal = InvertGraphEditor.CurrentProject.CurrentGraph.NodeItems.Contains(GraphItemObject);
            CreateContent();

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
            if (string.IsNullOrEmpty(GraphItemObject.Name))
            {
                GraphItemObject.Name = "";
            }
            GraphItemObject.EndEditing();
            Dirty = true;
        }

        public bool Dirty { get; set; }

        public bool IsFilter
        {
            get { return InvertGraphEditor.IsFilter(GraphItemObject.GetType()) && IsLocal; }
        }

        public IEnumerable<CodeGenerator> CodeGenerators
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

        public virtual IEnumerable<KeyValuePair<string, ValidatorType>> Issues
        {
            get
            {
                yield break;
            }
        }


        public void BeginEditing()
        {
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
                var fileGenerator = this.CodeGenerators.FirstOrDefault(p => !p.IsDesignerFile);
                if (fileGenerator != null)
                {
                    var filePath = fileGenerator.FullPathName;
                    //var filename = repository.GetControllerCustomFilename(this.Name);
                    var scriptAsset = AssetDatabase.LoadAssetAtPath(filePath, typeof(TextAsset));
                    AssetDatabase.OpenAsset(scriptAsset);
                }
            });
        }

        public void CtrlShiftClicked()
        {
            InvertGraphEditor.ExecuteCommand((diagram) =>
            {
                var fileGenerator = this.CodeGenerators.LastOrDefault(p => !p.IsDesignerFile);
                if (fileGenerator != null)
                {
                    var filePath = fileGenerator.FullPathName;
                    //var filename = repository.GetControllerCustomFilename(this.Name);
                    var scriptAsset = AssetDatabase.LoadAssetAtPath(filePath, typeof(TextAsset));
                    AssetDatabase.OpenAsset(scriptAsset);
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