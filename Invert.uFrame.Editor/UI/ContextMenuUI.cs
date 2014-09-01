using System.Collections.Generic;
using System.Linq;
using Invert.uFrame.Editor.ElementDesigner;
using UnityEditor;
using UnityEngine;

namespace Invert.uFrame.Editor
{
    public class ContextMenuUI : ICommandUI
    {
        public bool Flatten { get; set; }
        public List<IEditorCommand> Commands { get; set; }

        public ContextMenuUI()
        {
            Commands = new List<IEditorCommand>();
        }

        public void AddCommand(IEditorCommand command)
        {
            Commands.Add(command);
        }

        public void CreateMenuItems(GenericMenu genericMenu)
        {
            var groups = Commands.GroupBy(p => p.Group).OrderBy(p => p.Key == "Default").ToArray();

            foreach (var group in groups)
            {
                
                //genericMenu.AddDisabledItem(new GUIContent(group.Key));
                var groupCount = 0;
                foreach (var editorCommand in group.OrderBy(p => p.Order))
                {
                    

                    IEditorCommand command = editorCommand;
                    var argument = Handler.ContextObjects.FirstOrDefault(p => p != null && command.For.IsAssignableFrom(p.GetType()));

                    var dynamicCommand = command as IDynamicOptionsCommand;
                    if (dynamicCommand != null)
                    {
                        foreach (var option in dynamicCommand.GetOptions(argument).OrderBy(p=>p.Name))
                        {
                            groupCount++;
                            UFContextMenuItem option1 = option;
                            genericMenu.AddItem(new GUIContent(Flatten ? editorCommand.Title : option.Name), option.Checked, () =>
                            {
                                dynamicCommand.SelectedOption = option1;
                                Handler.ExecuteCommand(command);
                            });
                        }
                    }
                    else
                    {
                        var canPerform = command.CanPerform(argument);
                        if (canPerform != null)
                        {
                            if (command.ShowAsDiabled)
                                genericMenu.AddDisabledItem(new GUIContent((Flatten ? editorCommand.Title : editorCommand.Path) + " : " +canPerform));
                        }
                        else
                        {
                            groupCount ++;
                            genericMenu.AddItem(new GUIContent(Flatten ? editorCommand.Title : editorCommand.Path), editorCommand.IsChecked(argument), () =>
                            {
                                Handler.ExecuteCommand(command);
                            });
                        }
                    }


                }
                if (group != groups.Last() && groupCount > 0)
                genericMenu.AddSeparator("");
            }
        }
        public void Go()
        {
            var genericMenu = new GenericMenu();
            CreateMenuItems(genericMenu);
            genericMenu.ShowAsContext();
        }

        public ICommandHandler Handler { get; set; }
        public void GoBottom()
        {
                
        }
    }
}