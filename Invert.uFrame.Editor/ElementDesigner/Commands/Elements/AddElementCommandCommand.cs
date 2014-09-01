﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Invert.uFrame.Editor.ElementDesigner.Commands
{
    public class AddElementCommandCommand : EditorCommand<ElementData>
    {
        public override void Perform(ElementData node)
        {
            var property = new ViewModelCommandData()
            {
                Node = node,
                Name = node.Data.GetUniqueName("NewCommand")
            };
            node.Commands.Add(property);
            uFrameEditor.CurrentDiagram.Refresh();
            property.IsSelected = true;
            property.BeginEditing();
        }

        public override string CanPerform(ElementData node)
        {
            if (node == null) return "Arg can't be null";
            return null;
        }
    }
}
