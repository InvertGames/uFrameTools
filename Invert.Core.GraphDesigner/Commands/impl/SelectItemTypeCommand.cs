using System;
using System.Collections.Generic;
using System.Linq;

namespace Invert.Core.GraphDesigner
{
    public class SelectItemTypeCommand : EditorCommand<DiagramViewModel>
    {
        private List<GraphTypeInfo> _additionalTypes;
        public bool AllowNone { get; set; }
        public bool PrimitiveOnly { get; set; }
        public bool IncludePrimitives { get; set; }

        public SelectItemTypeCommand()
        {
        }

        public SelectItemTypeCommand(Func<IDiagramNodeItem, GraphTypeInfo[]> typesSelector)
        {
            TypesSelector = typesSelector;
            AllowNone = false;
        
        }

        public List<GraphTypeInfo> AdditionalTypes
        {
            get { return _additionalTypes ?? (_additionalTypes = new List<GraphTypeInfo>()); }
            set { _additionalTypes = value; }
        }
        public Func<IDiagramNodeItem, GraphTypeInfo[]> TypesSelector { get; set; }

        public override void Perform(DiagramViewModel node)
        {
            var typesList = GetRelatedTypes(node);

            var viewModelItem = node.SelectedNodeItem as TypedItemViewModel;
            ITypedItem viewModelItemData;
            if (viewModelItem == null)
            {

                viewModelItemData = node.SelectedNode.DataObject as ITypedItem;
                if (viewModelItemData == null)
                    return;
            }
            else
            {
                viewModelItemData = viewModelItem.Data;
            }


            InvertGraphEditor.WindowManager.InitTypeListWindow(typesList.ToArray(), (selected) =>
            {
                InvertGraphEditor.ExecuteCommand((diagram) =>
                {
                    viewModelItemData.RelatedType = selected.Name;
                });
            });
        }

        public virtual IEnumerable<GraphTypeInfo> GetRelatedTypes(DiagramViewModel diagramData)
        {
            if (AllowNone)
            {
                yield return new GraphTypeInfo() { Name = null, Group = "", Label = "[ None ]" };
            }
            if (IncludePrimitives)
            {
                var itemTypes = InvertGraphEditor.TypesContainer.ResolveAll<GraphTypeInfo>();
                foreach (var elementItemType in itemTypes)
                {
                    yield return elementItemType;
                }
            }
            foreach (var item in AdditionalTypes)
            {
                yield return item;
            }
            if (TypesSelector != null)
            {
                foreach (var item in TypesSelector(diagramData.SelectedNodeItem.DataObject as IDiagramNodeItem))
                {
                    yield return item;
                }
            }
            if (PrimitiveOnly) yield break;

            foreach (var item in diagramData.CurrentRepository.NodeItems.OfType<IDesignerType>())
            {
                yield return new GraphTypeInfo() { Name = item.Identifier, Group = "", Label = item.Name };
            }
        }

        public override string CanPerform(DiagramViewModel node)
        {
        
            if (node == null) return "No element data.";
            if (node.SelectedNode == null) return "No selection";
            //if (node.SelectedNodeItem as TypedItemViewModel == null) return "Must be an element item";
            return null;
        }
    }
}


