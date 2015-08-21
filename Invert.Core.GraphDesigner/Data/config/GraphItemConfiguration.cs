using System;
using System.Collections.Generic;

namespace Invert.Core.GraphDesigner
{
    public class GraphItemConfiguration 
    {
        public int OrderIndex { get; set; }
        public Type ReferenceType { get; set; }
        public Type SourceType { get; set; }

  
        public SectionVisibility Visibility { get; set; }
        public bool IsInput { get; set; }
        public bool IsOutput { get; set; }


    }

    public class ConfigurationProxyConfiguration : GraphItemConfiguration
    {
        public Func<GenericNode, IEnumerable<GraphItemConfiguration>> ConfigSelector { get; set; }
    }
}