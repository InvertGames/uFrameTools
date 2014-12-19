using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Invert.Core;
using Invert.Core.GraphDesigner;

namespace Invert.GraphDesigner.WPF
{
    public class SelectItemTypeViewModel
    {
        public IEnumerable<IItem> Items { get; set; }

        public SelectItemTypeViewModel()
        {
        }
        public IItem Selected { get; set; }
        public SelectItemTypeViewModel(IEnumerable<IItem> items, IItem selected, Action<IItem> ok)
        {
            Items = items;
            Selected = selected;
            OkCommand = new SimpleEditorCommand<DiagramViewModel>(_ =>
            {
                ok(Selected);
            });
        }

        public ICommand OkCommand { get; set; }
    }
}
