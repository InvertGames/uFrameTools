using System;
using System.Collections.Generic;
using System.Linq;
using Invert.Core;
using Invert.Core.GraphDesigner;
using Invert.Core.GraphDesigner.Unity;
using UnityEngine;

public class QuickAccessWindowViewModel : IWindow
{
    private readonly QuickAccessContext _context;

    public QuickAccessWindowViewModel(QuickAccessContext context)
    {
        _context = context;
        UpdateSearch();
    }

    private List<QuickAccessItem> _quickLaunchItems;
    private string _searchText = "";
    public string Identifier { get; set; }

    public string SearchText
    {
        get { return _searchText; }
        set { _searchText = value; }
    }

    public void UpdateSearch()
    {
        QuickLaunchItems.Clear();
        var launchItems = new List<IEnumerable<QuickAccessItem>>();

        InvertApplication.SignalEvent<IQuickAccessEvents>(_ => _.QuickAccessItemsEvents(_context, launchItems));
        
        foreach (var item in launchItems.SelectMany(p => p))
        {
            if (item.Title.Contains(SearchText))
            {
                QuickLaunchItems.Add(item);
            }
            if (QuickLaunchItems.Count >= 10)
            {
                break;
            }
        }
    }

    public List<QuickAccessItem> QuickLaunchItems
    {
        get { return _quickLaunchItems ?? (_quickLaunchItems = new List<QuickAccessItem>()); }
        set { _quickLaunchItems = value; }
    }

    public void ItemSelected(QuickAccessItem item)
    {
        item.Action();
        InvertApplication.SignalEvent<IWindowsEvents>(i=>i.WindowRequestCloseWithViewModel(this));
    }

}