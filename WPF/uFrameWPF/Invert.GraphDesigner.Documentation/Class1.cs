using System;
using Invert.Core.GraphDesigner;

namespace Invert.GraphDesigner.Documentation
{
    public class DocumentationGenerator<TData> : OutputGenerator
    {
        
        public override string ToString()
        {
               

            return base.ToString();
        }

        public override Type GeneratorFor
        {
            get { return typeof (TData); }
            set
            {
                
            }
        }
    }
}
