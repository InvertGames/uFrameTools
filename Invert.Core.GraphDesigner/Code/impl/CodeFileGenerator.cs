using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CSharp;

namespace Invert.Core.GraphDesigner
{
    public abstract class FileGeneratorBase
    {
        public string SystemPath { get; set; }
        public string AssetPath { get; set; }
        public abstract string CreateOutput();
        public override string ToString()
        {
            return CreateOutput();
        }
        public abstract bool CanGenerate(FileInfo fileInfo);
        
    }

    public class CodeFileGenerator : FileGeneratorBase
    {
        public CodeNamespace Namespace { get; set; }
        public CodeCompileUnit Unit { get; set; }

        public bool RemoveComments { get; set; }
        public string NamespaceName { get; set; }
        public CodeFileGenerator(string ns = null)
        {
            NamespaceName = ns;
        }

        public OutputGenerator[] Generators
        {
            get;
            set;
        }

        public override string CreateOutput()
        {
            RemoveComments = Generators.Any(p => !p.AlwaysRegenerate);

            Namespace = new CodeNamespace(NamespaceName);
            Unit = new CodeCompileUnit();
            Unit.Namespaces.Add(Namespace);
      
            foreach (var codeGenerator in Generators)
            {
               // UnityEngine.Debug.Log(codeGenerator.GetType().Name + " is generating");
                codeGenerator.Initialize(this);
            }
            var provider = new CSharpCodeProvider();

            var sb = new StringBuilder();
            var tw1 = new IndentedTextWriter(new StringWriter(sb), "    ");
            provider.GenerateCodeFromCompileUnit(Unit, tw1, new CodeGeneratorOptions());
            tw1.Close();
            if (RemoveComments)
            {
                var removedLines = sb.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.None).Skip(10).ToArray();
                return string.Join(Environment.NewLine, removedLines);
            }
            return sb.ToString();
        }

        public override bool CanGenerate(FileInfo fileInfo)
        {
            if (Generators.Any(p => p.AlwaysRegenerate)) return true;

            var doesTypeExist = Generators.Any(p => !p.IsValid(fileInfo));
            if (doesTypeExist || fileInfo.Exists)
            {
                return false;
            }

            return true;
        }
    }
}