using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.Ast;
using ICSharpCode.NRefactory.PrettyPrinter;
using ICSharpCode.NRefactory.Visitors;
using Invert.Common;
using Invert.Common.UI;
using UnityEditor;
using UnityEngine;

namespace Invert.Core.GraphDesigner.Unity.Refactoring
{
    public class GetMethodsVisitor : AbstractAstVisitor
    {
        public List<MethodDeclaration> Methods { get; set; }

        public override object VisitMethodDeclaration(MethodDeclaration methodDeclaration, object data)
        {
            Methods.Add(methodDeclaration);
            return base.VisitMethodDeclaration(methodDeclaration, data);
        }
    }
}
