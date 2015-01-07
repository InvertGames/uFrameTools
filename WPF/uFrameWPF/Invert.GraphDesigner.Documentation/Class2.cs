using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Invert.Core.GraphDesigner;

namespace Invert.GraphDesigner.Documentation
{
    public class DocumentationBuilder
    {
        private List<ScreenshotInfo> _screenshots;
        public ShellNodeTypeNode[] NodeTypes { get; set; }
        public ShellGraphTypeNode[] ShellGraphType { get; set; }

        public List<ScreenshotInfo> Screenshots
        {
            get { return _screenshots ?? (_screenshots = new List<ScreenshotInfo>()); }
            set { _screenshots = value; }
        }

    }


    public class ScreenshotInfo
    {
        public string Name { get; set; }
        public string ImageUrl { get; set; }

        
        public GraphItemViewModel ViewModel { get; set; }
    }

    public class GraphDocumentationViewModel
    {
        public string Title { get; set; }
        public ShellGraphTypeNode GraphType { get; set; }
        
        public string ImagePath { get; set; }


    }
    public class NodeDocumentationViewModel
    {
        public string Title { get; set; }
        public ShellNodeTypeNode NodeType { get; set; }
        public Type Type { get; set; }
        


        public string ImagePath { get; set; }

    }
}
