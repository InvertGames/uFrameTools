using System.Linq;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Invert.Core.GraphDesigner;

namespace Invert.GraphDesigner.WPF
{
    public class WPFContextMenu : ContextMenuUI
    {
        public override void Go()
        {
            base.Go();
            var menu = new ContextMenu();

            foreach (var item in Commands)
            {
                var arg = Handler.ContextObjects.FirstOrDefault(p => p != null && item.For.IsAssignableFrom(p.GetType()) && item.CanPerform(p) == null);
                if (arg == null) continue;
                //if (item.CanPerform(arg) != null) continue;

                var dynamicOptions = item as IDynamicOptionsCommand;
                if (dynamicOptions != null)
                {
                    var menuItem = new MenuItem()
                    {
                        Header = item.Name
                    };
                    var options =
                        dynamicOptions.GetOptions(arg);
                    foreach (var option in options)
                    {
                        var option1 = option;
                        
                        menuItem.Items.Add(new MenuItem()
                        {
                            Header = option.Name,//.Split('/').LastOrDefault(),
                            IsChecked = option.Checked,
                            DataContext = option.Value,
                            Command = new SimpleEditorCommand<DiagramViewModel>(_ =>
                            {
                                dynamicOptions.SelectedOption = option1;
                                InvertGraphEditor.ExecuteCommand(dynamicOptions as IEditorCommand);
                            })
                        });
                        if (option.Checked)
                        {
                            menuItem.Header += string.Format(" ( {0} )", option.Name);
                        }
                    }
                    //if (menu.Items.Count > 0)
                    menu.Items.Add(menuItem);
                }
                else
                {
                    if (item.CanPerform(arg) != null) continue;
                    menu.Items.Add(new MenuItem()
                    {
                        Header = item.Name,
                        Command = item as ICommand
                    });
                }
            }
            var window = InvertGraphEditor.DesignerWindow as Control;
            window.ContextMenu = menu;
            menu.Placement = PlacementMode.MousePoint;
            menu.IsOpen = true;
        }
    }
}