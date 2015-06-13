using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Invert.Core.GraphDesigner;
using UnityEngine;

namespace Invert.Platform.Gwen
{
    public class WindowsContextMenu : ContextMenuUI
    {
        public static ContextMenuStrip ContextMenu { get; set; }
        public static Control Control { get; set; }
        public override void AddSeparator(string empty)
        {
            base.AddSeparator(empty);
            Commands.Add(null);
        }

        public override void Go()
        {
            base.Go();
             
            ContextMenu.Items.Clear();
            foreach (var item in this.Commands)
            {
                if (item == null)
                {
                    ContextMenu.Items.Add(new ToolStripSeparator());
                    continue;
                }
                var obj = Handler.ContextObjects.FirstOrDefault(p => item.For.IsAssignableFrom(p.GetType()));

                var item1 = item;
                var dynamicCommand = item as IDynamicOptionsCommand;
                if (dynamicCommand != null)
                {
                   
                    if (obj == null) continue;

                    foreach (var option in dynamicCommand.GetOptions(obj))
                    {
                        var option1 = option;
                        AddByPath(option.Name, () =>
                        {
                            dynamicCommand.SelectedOption = option1;
                            InvertGraphEditor.ExecuteCommand(item1);
                        });
                    }
                }
                else
                {
                    if (item.CanPerform(obj) == null)
                    {
                        AddByPath(item.Path, () =>
                        {
                            InvertGraphEditor.ExecuteCommand(item1);
                        });
                    }
                }
               
            }
            if (InvertGraphEditor.DesignerWindow.DiagramViewModel.LastMouseEvent != null)
            {
                var mousePosition = InvertGraphEditor.DesignerWindow.DiagramViewModel.LastMouseEvent.MousePosition;
                ContextMenu.Show(Control, new Point(Mathf.RoundToInt(mousePosition.x)
                    , Mathf.RoundToInt(mousePosition.y)));
            }
            
        }

        public void AddByPath(string commandPath, Action execute)
        {
            var path = commandPath.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            ToolStripMenuItem menuItem = null;
            var i = 0;
            for (int index = 0; index < path.Length; index++, i++)
            {
                var item = path[index];
                if (menuItem != null)
                {
                    var find = menuItem.DropDownItems.Find(item, true).FirstOrDefault() as ToolStripMenuItem;
                    if (find == null)
                    {
                        break;
                    }
                    menuItem = find;
                }
                else
                {
                    var find = ContextMenu.Items.Find(item, true).FirstOrDefault() as ToolStripMenuItem;
                    if (find == null) break;
                    menuItem = find;
                }
            }

            for (int index = i; index < path.Length; index++)
            {
                var item = path[index];
                var oldMenuItem = menuItem;

                menuItem = new ToolStripMenuItem(item, null, (s, e) => { execute(); })
                {
                    Name = item
                };
                if (oldMenuItem != null)
                {
                    oldMenuItem.DropDownItems.Add(menuItem);
                }
                else
                {
                    ContextMenu.Items.Add(menuItem);
                }
            }
        }
       
    }
}