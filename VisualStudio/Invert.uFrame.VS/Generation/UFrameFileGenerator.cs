using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Invert.Core;
using Invert.Core.GraphDesigner;
using Microsoft.Samples.VisualStudio.GeneratorSample;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using VSLangProj80;

namespace Invert.uFrame.VS
{
    [ComVisible(true)]
    [Guid("52B316AA-1997-4c81-9969-83604C09EEBA")]
    [CodeGeneratorRegistration(typeof(UFrameFileGenerator), "C# uFrame Generator", "{FAE04EC1-301F-11D3-BF4B-00C04F79EFBC}", GeneratesDesignTimeSource = false)]
    [ProvideObject(typeof(UFrameFileGenerator))]
    public class UFrameFileGenerator : BaseCodeGeneratorWithSite
    {
        protected override string GetDefaultExtension()
        {
            return ".cs";
        }

        protected override byte[] GenerateCode(string inputFileContent)
        {
            var sbOutput = new StringBuilder();
            InvertGraph graph;
            if (!VisualStudioProjectRepository.LoadGraphFromString(this.InputFilePath, inputFileContent, out graph))
            {
                GeneratorError(0,"Couldn't parse graph file.",0,0);
                return null;
            }
            var project = InvertGraphEditor.Projects.FirstOrDefault(p => p.Graphs.Any(x => x.Identifier == graph.Identifier)) as VisualStudioProjectRepository;
            graph.SetProject(project);
           // project.Project.AddItem(0,VSADDITEMOPERATION.VSADDITEMOP_LINKTOFILE, )

            InvertGraphEditor.Platform.Progress(0, "Refactoring");
            //// Go ahead and process any code refactors
            //ProcessRefactorings(diagram);

            //var codeGenerators = uFrameEditor.GetAllCodeGenerators(item.Data).ToArray();
            //var generatorSettings = InvertGraphEditor.CurrentProject.GeneratorSettings;
            var fileGenerators = InvertGraphEditor.GetAllFileGenerators(null, graph, false, System.IO.Path.GetDirectoryName(this.InputFilePath)).ToArray();
            var length = 100f / (fileGenerators.Length + 1);
            // Debug.Log(fileGenerators.Length);
            var index = 0;
            List<string> directories = new List<string>();
            List<CodeGenerator> designerGenerators = new List<CodeGenerator>();
            foreach (var codeFileGenerator in fileGenerators)
            {
                index++;
                InvertGraphEditor.Platform.Progress(length * index, "Generating " + System.IO.Path.GetFileName(codeFileGenerator.AssetPath));

        
                codeFileGenerator.NamespaceName = this.FileNameSpace;
                
                // Grab the information for the file
                var fileInfo = new FileInfo(codeFileGenerator.SystemPath);
                //Debug.Log(codeFileGenerator.SystemPath + ": " + fileInfo.Exists);
                
                // Make sure we are allowed to generate the file
                if (!codeFileGenerator.CanGenerate(fileInfo))
                {
                    //Debug.Log("Can't generate " + fileInfo.FullName);
                    continue;
                }
                if (codeFileGenerator.Generators.Any(p => p.IsDesignerFile))
                {
                    designerGenerators.AddRange(codeFileGenerator.Generators);
                   
                    continue;
                }
                // Get the path to the directory
                var directory = System.IO.Path.GetDirectoryName(fileInfo.FullName);
                if (!directories.Contains(directory))
                {
                    directories.Add(directory);
                }
                // Create it if it doesn't exist
                if (directory != null && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    
                }
                try
                {
             
                    File.WriteAllText(fileInfo.FullName, codeFileGenerator.ToString());
                    this.GetVSProject().Project.ProjectItems.AddFromFile(fileInfo.FullName);
                }
                catch (Exception ex)
                {
                    InvertApplication.LogException(ex);
                    InvertApplication.Log("Coudln't create file " + fileInfo.FullName);
                }

            }
       
            foreach (var allDiagramItem in graph.NodeItems)
            {
                allDiagramItem.IsNewNode = false;
            }
            //RefactorApplied(diagram.DiagramData);
            var designerFileGenerator = new CodeFileGenerator(this.FileNameSpace)
            {
                Generators = designerGenerators.ToArray()
            };

            return System.Text.Encoding.Default.GetBytes( designerFileGenerator.ToString()); // failed
        }
    }
}
