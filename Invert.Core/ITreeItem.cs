using System.Collections.Generic;

namespace Invert.Core
{
    public interface ITreeItem : IItem
    {
        IItem ParentItem { get; }
        IEnumerable<IItem> Children { get; }
        bool Expanded { get; set; }
    }

    public class DefaultTreeItem : DefaultItem, ITreeItem
    {
        public DefaultTreeItem(string title, string @group) : base(title, @group)
        {
        }

        public IItem ParentItem { get; set; }
        public IEnumerable<IItem> Children { get; set; }
        public bool Expanded { get; set; }
    }
}