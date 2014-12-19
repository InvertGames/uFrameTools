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

        public void InitItemWindow<TItem>(IEnumerable<TItem> items, Action<TItem> action) where TItem : IItem
        {
            
        }
    }
}