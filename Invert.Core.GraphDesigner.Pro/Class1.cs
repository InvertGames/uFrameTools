using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Invert.Core.GraphDesigner.Pro
{
    
    public class TemplateClass : GenericNode, ITemplateClass<ShellNodeTypeNode>
    {

        [TemplateMember()] 
        public string _MyProperty;

        [TemplateMember()]
        public string MyProperty { get; set; }

        public void MyMethod()
        {
            
        }

        public TemplateContext<ShellNodeTypeNode> Context { get; set; }
        
    }

    public class TemplateMember : Attribute
    {
        public string Group { get; set; }

    }
    public interface ITemplateClass<TData>
    {
        TemplateContext<TData> Context { get; set; }
       
    }

    public interface ITemplate
    {

    }

    public class TemplateContext<TData>
    {
        public TData Data { get; set; }
        public object CurrentItem { get; set; }

        public TAs Item<TAs>()
        {
            return (TAs)CurrentItem;
        }
        public CodeMemberMethod CurrentMethod { get; set; }
        public CodeMemberProperty CurrentProperty { get; set; }

    }
}
