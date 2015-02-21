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
    public static void Init(string title, IEnumerable<IItem> items, Action<IItem> selected, bool allowNone = false)
    {
        // Get existing open window or if none, make a new one:
        var window = (ItemSelectionWindow)GetWindow(typeof(ItemSelectionWindow));
        window.title = title;
        window.Items = items;
        window.SelectedAction = selected;
        window.ApplySearch();
        window.minSize = new Vector2(200, 200);
        window.AllowNode = allowNone;
        window.Show();
    }

    public bool AllowNode { get; set; }

    public IEnumerable<IItem> Items { get; set; }
    public IItem[] ItemsArray { get; set; }
    public Action<IItem> SelectedAction { get; set; }
    public bool IsClosing { get; set; }

    protected override void ApplySearch()
    {
        if (Items == null) return;
        if (!string.IsNullOrEmpty(_SearchText))
        {
            var text = _SearchText.ToLower();
            //ItemsArray = Items.Where(p => p.SearchTag != null && p.SearchTag.Contains(_SearchText)).ToArray();
            ItemGroups = Items.Where(
                delegate(IItem p)
                {

                    var st = p.SearchTag;
                    if (st == null) return false;
                    st = st.ToLower();
                    return (st.Contains(text) || st == text);
                }).OrderBy(p => p.Title).GroupBy(p => p.Group).ToArray();
        }
        else
        {
            //ItemsArray = Items.ToArray();
            ItemGroups = Items.OrderBy(p=> p.Title).GroupBy(p => p.Group).ToArray();
        }
    }

    public IGrouping<string, IItem>[] ItemGroups { get; set; }

    public override void OnGUIScrollView()
    {
        if (AllowNode)
        {
            if (
                           GUIHelpers.DoTriggerButton(new UFStyle()
                           {
                               Label = "[NONE]",
                               IsWindow = true,
                               FullWidth = true,
                               BackgroundStyle = ElementDesignerStyles.EventButtonStyleSmall
                           }))
            {
                SelectedAction(null);
                IsClosing = true;
            }
        }
        if (ItemGroups == null)
        {
            return;
        }

        foreach (var group in ItemGroups)
        {
            if (group.Any())
            {
                if (string.IsNullOrEmpty(_SearchText))
                {
                    if (GUIHelpers.DoToolbarEx(group.Key))
                    {   
                        foreach (var item in group)
                        {
                            if (item == null) continue;
                            if (
                                GUIHelpers.DoTriggerButton(new UFStyle()
                                {
                                    Label = item.Title,
                                    IsWindow = true,
                                    FullWidth = true,
                                    BackgroundStyle = ElementDesignerStyles.EventButtonStyleSmall
                                }))
                            {
                                SelectedAction(item);
                                IsClosing = true;
                            }
                        }
                    }
                }
                else
                {
                
                        foreach (var item in group)
                        {
                            if (item == null) continue;
                            if (GUIHelpers.DoTriggerButton(new UFStyle() { Label = item.Group + " : " + item.Title, IsWindow = true, FullWidth = true, BackgroundStyle = ElementDesignerStyles.EventButtonStyleSmall }))
                            {
                                SelectedAction(item);
                                IsClosing = true;
                            }
                        }
                    
                }
            

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