using System;
using System.Collections.Generic;
using Invert.Core;

namespace DiagramDesigner.Platform
{
    public class WindowsWindowManager : IWindowManager
    {
        public void InitTypeListWindow(GraphTypeInfo[] typesInfoList, Action<GraphTypeInfo> action)
        {
            
        }


        public void InitItemWindow<TItem>(IEnumerable<TItem> items, Action<TItem> action, bool allowNone = false) where TItem : IItem
        {
            
        }

        public void ShowHelpWindow(string helpProviderName, Type graphItemType)
        {
            
        }

        public void TypeInputWindow(Action<GraphTypeInfo> action)
        {
            
        }
    }
}