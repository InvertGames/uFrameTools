using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Invert.Core.GraphDesigner
{
    public interface IGraphEditorSettings
    {
        bool UseGrid { get; set; }
        bool ShowHelp { get; set; }
        bool ShowGraphDebug { get; set; }
        Color BackgroundColor { get; set; }
        Color GridLinesColor { get; set; }
        Color GridLinesColorSecondary { get; set; }
    }
}
