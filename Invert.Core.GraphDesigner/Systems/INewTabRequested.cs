using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Invert.Core.GraphDesigner
{
    public interface INewTabRequested
    {
        void NewTabRequested();
    }

    public interface IQueryGraphsActions
    {
        void QueryGraphsAction(List<ActionItem> items);
    }

    public interface IHideSelectionMenu
    {
        void HideSelection();
    }

}
