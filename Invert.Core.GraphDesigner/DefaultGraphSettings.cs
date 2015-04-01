using UnityEngine;

namespace Invert.Core.GraphDesigner
{
    public class DefaultGraphSettings : IGraphEditorSettings
    {
        public Color BackgroundColor { get; set; }
        public Color TabTextColor { get; set; }

        public Color GridLinesColor { get; set; }

        public Color GridLinesColorSecondary { get; set; }

        public bool ShowGraphDebug { get; set; }

        public bool ShowHelp
        {
            get { return false; }
            set
            {
            }
        }

        public bool UseGrid
        {
            get { return true; }
            set { }
        }

        public DefaultGraphSettings()
        {
            BackgroundColor = new Color(0.13f, 0.13f, 0.13f);
            GridLinesColor = new Color(0.1f, 0.1f, 0.1f);
            GridLinesColorSecondary = new Color(0.08f, 0.08f, 0.08f);
        }
    }
}