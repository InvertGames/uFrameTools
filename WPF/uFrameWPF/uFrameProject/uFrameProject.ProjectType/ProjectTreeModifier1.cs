﻿using System;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Reflection;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Utilities;
using Microsoft.VisualStudio.ProjectSystem.Designers;
using Microsoft.VisualStudio.ProjectSystem.Utilities.Designers;

namespace Invert.uFrame.VisualStudio.Project
{
    [Export(typeof(IProjectTreeModifier))]
    [AppliesTo(MyUnconfiguredProject.UniqueCapability)]
    internal class ProjectTreeModifier1 : IProjectTreeModifier
    {
        public IProjectTree ApplyModifications(IProjectTree tree, IProjectTreeProvider projectTreeProvider)
        {
            // Only set the icon for the root project node.  We could choose to set different icons for nodes based
            // on various criteria, not just Capabilities, if we wished.
            if (tree.Capabilities.Contains(ProjectTreeCapabilities.ProjectRoot))
            {
                tree = tree.SetIcon(KnownMonikers.JSProjectNode.ToProjectSystemType());
            }

            return tree;
        }
    }
}