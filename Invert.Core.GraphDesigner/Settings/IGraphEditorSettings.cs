using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Invert.Core.GraphDesigner.Settings
{
    public interface IGraphEditorSettings
    {
        bool UseGrid { get; set; }
        bool ShowHelp { get; set; }
    }
}
