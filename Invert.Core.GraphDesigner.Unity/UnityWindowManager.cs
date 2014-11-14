using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Invert.Core;
using Invert.Core.GraphDesigner;
using Invert.uFrame.Editor;
using UnityEditor;

public class UnityWindowManager : IWindowManager
{
    public void InitTypeListWindow(GraphTypeInfo[] typesInfoList, Action<GraphTypeInfo> action)
    {
        ElementItemTypesWindow.InitTypeListWindow("Choose Type", typesInfoList, (selected) =>
        {
            EditorWindow.GetWindow<ElementItemTypesWindow>().Close();
            InvertGraphEditor.ExecuteCommand(_ =>
            {
                action(selected);
            });
          
          
        });
    }
    public void InitItemWindow<TItem>(IEnumerable<TItem> items, Action<TItem> action)
        where TItem : IItem
    {
        ItemSelectionWindow.Init("Select Item",items.Cast<IItem>(), (item) =>
        {
            InvertGraphEditor.ExecuteCommand(_ =>
            {
                action((TItem)item);
            });
            
            
        });
    }
}