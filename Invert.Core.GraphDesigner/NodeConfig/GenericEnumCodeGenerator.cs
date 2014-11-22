using System;
using System.CodeDom;
using System.Collections.Generic;

namespace Invert.Core.GraphDesigner
{
    public class GenericEnumCodeGenerator<TData, TItem> : CodeGenerator
        where TData : IDiagramNodeItem
        where TItem : IDiagramNodeItem
    {
        public TData Data { get; set; }
        public Func<TData, IEnumerable<TItem>> Selector { get; set; }
        public override void Initialize(CodeFileGenerator fileGenerator)
        {
            base.Initialize(fileGenerator);
            //if (IsDesignerFile)
            //{
            UnityEngine.Debug.Log("HERE");
            var enumDecleration = new CodeTypeDeclaration(Data.Name) { IsEnum = true };
            foreach (var item in Selector(Data))
            {
                enumDecleration.Members.Add(new CodeMemberField(enumDecleration.Name, item.Name));
            }
            Namespace.Types.Add(enumDecleration);
            //}
            
        }
    }
}