using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using Invert.Core;
using Invert.GraphDesigner.WPF;
using Invert.uFrame.VS.Windows;

namespace Invert.uFrame.VS
{
    public class VSWindows : IWindowManager
    {
        public void InitTypeListWindow(SelectItemTypeViewModel viewModel)
        {
      
        }

        public void InitTypeListWindow(GraphTypeInfo[] typesInfoList, Action<GraphTypeInfo> action)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {

                var d = new SelectItemTypeWindow();
                d.DataContext = new SelectItemTypeViewModel(typesInfoList.Cast<IItem>(), null, (item) =>
                {
                    action((GraphTypeInfo)item);
                    d.Close();
                });
                d.ShowModal();
            }));
        }

        public void InitItemWindow<TItem>(IEnumerable<TItem> items, Action<TItem> action) where TItem : IItem
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {

                var d = new SelectItemTypeWindow();
                d.DataContext = new SelectItemTypeViewModel(items.Cast<IItem>(), null, (item) =>
                {
                    action((TItem)item);
                    d.Close();
                });
                d.ShowModal();
            }));
        }

        public void TypeInputWindow(Action<GraphTypeInfo> action)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {

                //var d = new SelectItemTypeWindow();
                //d.DataContext = new SelectItemTypeViewModel(items.Cast<IItem>(), null, (item) =>
                //{
                //    action((TItem)item);
                //    d.Close();
                //});
                //d.ShowModal();
            }));
        }

        public static Dispatcher Dispatcher { get; set; }
    }
}