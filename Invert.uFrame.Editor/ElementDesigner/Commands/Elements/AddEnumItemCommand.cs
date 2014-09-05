using System;
using System.Diagnostics;
using Invert.uFrame.Editor.ViewModels;

namespace Invert.uFrame.Editor.ElementDesigner.Commands
{
    public class AddEnumItemCommand : EditorCommand<EnumNodeViewModel>
    {
        public override void Perform(EnumNodeViewModel node)
        {
            node.AddNew();
         
        }

        public override string CanPerform(EnumNodeViewModel node)
        {
            if (node == null) return "Arg can't be null";
            return null;
        }

    }
}