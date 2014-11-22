using System.Collections.Generic;
using System.Linq;
using Invert.uFrame.Editor.ViewModels;
using UnityEditor;
using UnityEngine;

namespace Invert.Core.GraphDesigner
{
    public class OpenCommand : EditorCommand<DiagramNodeViewModel>, IDynamicOptionsCommand, IDiagramNodeCommand
    {
        public override string Group
        {
            get { return "File"; }
        }
        public override void Perform(DiagramNodeViewModel node)
        {
            var generator = SelectedOption.Value as CodeGenerator;
            if (generator == null) return;
            var pathStrategy = InvertGraphEditor.CurrentDiagramViewModel.DiagramData.CodePathStrategy;
            var filePath = generator.FullPathName;
            //var filename = repository.GetControllerCustomFilename(this.Name);
            var scriptAsset = AssetDatabase.LoadAssetAtPath(filePath, typeof(TextAsset));
            AssetDatabase.OpenAsset(scriptAsset);
        }

        public override string CanPerform(DiagramNodeViewModel node)
        {
            return null;
        }

        public IEnumerable<UFContextMenuItem> GetOptions(object item)
        {
            var diagramItem = item as DiagramNodeViewModel;
            if (diagramItem == null) yield break;
            var generators = diagramItem.CodeGenerators.ToArray();

            foreach (var codeGenerator in generators.Where(p=>!p.IsDesignerFile))
            {
                yield return new UFContextMenuItem()
                {
                    Name = "Open/" + codeGenerator.Filename,
                    Value = codeGenerator
                };
            }
            foreach (var codeGenerator in generators.Where(p => p.IsDesignerFile))
            {
                yield return new UFContextMenuItem()
                {
                    Name = "Open/Designer Files/" + codeGenerator.Filename,
                    Value = codeGenerator
                };
            }
        }

        public UFContextMenuItem SelectedOption { get; set; }
        public MultiOptionType OptionsType { get; private set; }
    }
}