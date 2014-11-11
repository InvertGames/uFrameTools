using System;
using System.Collections.Generic;
using System.Linq;
using Invert.Core.GraphDesigner;
using Invert.uFrame.Editor;
using Invert.uFrame.Editor.ElementDesigner;
using Invert.uFrame.Editor.ElementDesigner.Commands;
using Invert.uFrame.Editor.ViewModels;
using UnityEditor;
using UnityEngine;

public class SelectItemTypeCommand : EditorCommand<DiagramViewModel>
{
    public bool AllowNone { get; set; }
    public bool PrimitiveOnly { get; set; }
    public bool IncludeUnityEngine { get; set; }
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

        if (!PrimitiveOnly)
        {
            foreach (var viewModel in diagramData.CurrentRepository.NodeItems.OfType<IDesignerType>())
            {
                yield return new GraphTypeInfo()
                {
                    Name = viewModel.Identifier,
                    Label = viewModel.Name,
                    Group = diagramData.Title
                };
            }
        }
        var itemTypes = InvertGraphEditor.TypesContainer.ResolveAll<GraphTypeInfo>();
        foreach (var elementItemType in itemTypes)
        {
            yield return elementItemType;
        }

        if (PrimitiveOnly) yield break;
        //if (IncludeUnityEngine)
        //{
        //    yield return new ElementItemType() { Type = typeof(UnityEngine.MonoBehaviour), Group = "UnityEngine", Label = "MonoBehaviour" };
        //    yield return new ElementItemType() { Type = typeof(UnityEngine.Component), Group = "UnityEngine", Label = "Component" };


        //}

        foreach (var item in diagramData.CurrentRepository.NodeItems.OfType<IDesignerType>())
        {
            yield return new GraphTypeInfo() { Name = item.Identifier, Group = "", Label = item.Name };
        }
        
        //foreach (var projectAssembly in AppDomain.CurrentDomain.GetAssemblies())
        //{
        //    foreach (var type in projectAssembly.GetTypes())
        //    {
        //        if (type.ContainsGenericParameters) continue;
        //        if (type.Name.Contains("$")) continue;
        //        if (IncludeUnityEngine && typeof(UnityEngine.Object).IsAssignableFrom(type))
        //        {
        //            yield return new ElementItemType() { Type = type, Group = "Components", Label = type.Name }; ;
        //            continue;
        //        }
        //        if (!typeof(Component).IsAssignableFrom(type) && type.IsClass && !type.Name.Contains("<") && !typeof(ViewModel).IsAssignableFrom(type) && !typeof(Controller).IsAssignableFrom(type) && !typeof(ViewBase).IsAssignableFrom(type))
        //        {
        //            if (!type.ContainsGenericParameters)
        //                yield return new ElementItemType() { Type = type, Group = "Project", Label = type.Name };
        //        }
        //    }
        //}
        
        
    }

    public override string CanPerform(DiagramViewModel node)
    {
        
        if (node == null) return "No element data.";
        if (node.SelectedNode == null) return "No selection";
        //if (node.SelectedNodeItem as TypedItemViewModel == null) return "Must be an element item";
        return null;
    }
}


