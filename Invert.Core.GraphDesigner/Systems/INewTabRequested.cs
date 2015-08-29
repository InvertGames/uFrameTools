using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

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

    public interface IEnableQuickAccess
    {
        void EnableQuickAccess(QuickAccessContext context, Vector2? position = null);
    }

    public interface IDisableQuickAccess
    {
        void DisableQuickAcess();
    }

    public interface IUpdateQuickAccess
    {
        void UpdateQuickAcess(QuickAccessContext context);
    }
}
