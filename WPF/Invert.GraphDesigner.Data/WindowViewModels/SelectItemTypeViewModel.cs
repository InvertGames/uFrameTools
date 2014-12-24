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
    public class SelectItemTypeViewModel : ViewModel
    {
        private string _searchTerms;
        private IEnumerable<IItem> _filteredItems;
        public IEnumerable<IItem> Items { get; set; }

        public IEnumerable<IItem> FilteredItems
        {
            get
            {
                if (_filteredItems == null)
                    return Items.Take(10);
                return _filteredItems;
            }
            set
            {
                _filteredItems = value;
                OnPropertyChanged("FilteredItems");
            }
        }

        public string SearchTerms
        {
            get { return _searchTerms; }
            set
            {
                _searchTerms = value;
                if (string.IsNullOrEmpty(value))
                {
                    FilteredItems = null;
                }
                else
                {
                    FilteredItems = Items.Where(p => p.SearchTag.Contains(value) || p.SearchTag == value).Take(10);
                }
                
                this.OnPropertyChanged("SearchTerms");
            }
        }

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
