using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Invert.Core.GraphDesigner;
using Invert.uFrame.Editor.ViewModels;

namespace Invert.uFrame.Editor.ElementDesigner.Commands
{
    public class AddElementCommandCommand : EditorCommand<ElementNodeViewModel>
    {
        public override void Perform(ElementNodeViewModel node)
        {
            node.AddCommand();
        }

        public override string CanPerform(ElementNodeViewModel node)
        {
            if (node == null) return "Arg can't be null";
            return null;
        }
    }
}
