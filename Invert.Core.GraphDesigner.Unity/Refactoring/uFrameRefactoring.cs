using System;
using System.IO;
using System.Linq;
using UnityEditor;

namespace Invert.Core.GraphDesigner.Unity.Refactoring
{
    public class uFrameRefactoring : DiagramPlugin, ICompileEvents, INodeItemEvents
    {
        public override void Initialize(uFrameContainer container)
        {
            ListenFor<INodeItemEvents>();
        }

        public void PreCompile(INodeRepository repository, IGraphData diagramData)
        {
           
        }

        public void FileGenerated(CodeFileGenerator generator)
        {
            
        }

        public void PostCompile(INodeRepository repository, IGraphData diagramData)
        {
            
        }

        public void FileSkipped(CodeFileGenerator codeFileGenerator)
        {
            
        }

        public void Deleted(IDiagramNodeItem node)
        {
            
        }

        public void Hidden(IDiagramNodeItem node)
        {
        
        }

        public void Renamed(IDiagramNodeItem node, string previousName, string newName)
        {
            var graphNode = node;

          
            // Grab all the generated files
            var generators = graphNode.GetAllEditableFilesForNode().ToArray();
            
            var hasChanges = false;
            foreach (var item in generators)
            {
                var newFilename = item.FullPathName;
                // Set the node back to what it was for a second
                node.Name = previousName;
                var oldFilename = item.FullPathName;

                if (!File.Exists(oldFilename))
                {
                    //InvertApplication.Log(string.Format("Skipping {0} because it doesn't exist.", item.FullPathName));
                    continue;
                }
                InvertApplication.Log(string.Format("Moving {0} to {1}", oldFilename, newFilename));
                File.Move(oldFilename,newFilename);
                if (File.Exists(oldFilename + ".meta"))
                File.Move(oldFilename + ".meta",newFilename + ".meta");


                hasChanges = true;
            }
            if (hasChanges)
            {
                InvertGraphEditor.ExecuteCommand(new SaveCommand());
                AssetDatabase.Refresh();

            }
            node.Name = newName;

        }
    }
}