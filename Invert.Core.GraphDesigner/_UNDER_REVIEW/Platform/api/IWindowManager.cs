using System;
using System.Collections.Generic;
#if !UNITY_DLL
using Invert.GraphDesigner.WPF;
#endif
namespace Invert.Core
{
    public interface IWindowManager
    {
        //void InitTypeListWindow(SelectItemTypeViewModel viewModel);
        void InitTypeListWindow(GraphTypeInfo[] typesInfoList, Action<GraphTypeInfo> action);

        void InitItemWindow<TItem>(IEnumerable<TItem> items, Action<TItem> action, bool allowNone = false)
            where TItem : IItem;


        void ShowHelpWindow(string helpProviderName, Type graphItemType);
    }
}