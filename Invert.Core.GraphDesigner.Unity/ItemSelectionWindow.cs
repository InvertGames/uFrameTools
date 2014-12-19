using System;
using System.Collections.Generic;
using System.Linq;
using Invert.Common;
using Invert.Common.UI;
using Invert.Core;
using Invert.uFrame.Editor;
using UnityEngine;

public class ItemSelectionWindow : SearchableScrollWindow
{
    public static void Init(string title, IEnumerable<IItem> items, Action<IItem> selected)
    {
        // Get existing open window or if none, make a new one:
        var window = (ItemSelectionWindow)GetWindow(typeof(ItemSelectionWindow));
        window.title = title;
        window.Items = items;
        window.SelectedAction = selected;
        window.ApplySearch();
        window.minSize = new Vector2(200, 200);
        window.Show();
    }

    public IEnumerable<IItem> Items { get; set; }
    public IItem[] ItemsArray { get; set; }
    public Action<IItem> SelectedAction { get; set; }
    public bool IsClosing { get; set; }

    protected override void ApplySearch()
    {
        if (string.IsNullOrEmpty(_SearchText))
        {
            ItemsArray = Items.Where(p => p.SearchTag != null && p.SearchTag.Contains(_SearchText)).ToArray();
        }
        else
        {
            ItemsArray = Items.ToArray();
        }
    }

    public override void OnGUIScrollView()
    {
        foreach (var item in ItemsArray)
        {
            if (item == null) continue;
            if (GUIHelpers.DoTriggerButton(new UFStyle() { Label = item.Title, IsWindow = true, FullWidth = true,BackgroundStyle = ElementDesignerStyles.EventButtonStyleSmall}))
            {
                SelectedAction(item);
                IsClosing = true;
            }
        }
    }

    public override void OnGUI()
    {
        base.OnGUI();
        if (IsClosing == true)
        {
            IsClosing = false;
            Close();
        }
    }
}