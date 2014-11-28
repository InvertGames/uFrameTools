using System.IO;
using Invert.Core;
using Invert.Core.GraphDesigner;
using Invert.Core.GraphDesigner.Unity;
using Invert.uFrame.Code.Bindings;
using Invert.uFrame.Editor.ElementDesigner;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Invert.uFrame.Editor
{

    public static class uFrameEditor
    {

        public static INamespaceProvider NamespaceProvider
        {
            get
            {
                return Container.Resolve<INamespaceProvider>();
            }
        }

        private static IEnumerable<CodeGenerator> _generators;

        private static IDiagramPlugin[] _plugins;

        private static IToolbarCommand[] _toolbarCommands;
        private static IProjectRepository _repository;


        private static ProjectRepository[] _projects;
        private static UFrameSettings _settings;
        private static IUFrameTypeProvider _uFrameTypes;
       
        private static Type[] _codeGenerators;
        

        public static uFrameContainer Container
        {
            get
            {
                return InvertApplication.Container;
            }
            set { InvertApplication.Container = value; }
        }

 
        //public static DiagramViewModel CurrentDiagramViewModel
        //{
        //    get { return CurrentDiagram.DiagramViewModel; }
        //}

        public static DiagramDrawer CurrentDiagram
        {
            get
            {
                return DesignerWindow.DiagramDrawer;
            }
        }

        public static ElementsDesigner DesignerWindow
        {
            get { return InvertGraphEditor.DesignerWindow as ElementsDesigner; }
        }

        public static bool ShowHelp
        {
            get
            {
                return EditorPrefs.GetBool("UFRAME_ShowHelp", true);
            }
            set
            {
                EditorPrefs.SetBool("UFRAME_ShowHelp", value);
            }
        }

        public static bool ShowInfoLabels
        {
            get
            {
                return EditorPrefs.GetBool("UFRAME_ShowInfoLabels", true);
            }
            set
            {
                EditorPrefs.SetBool("UFRAME_ShowInfoLabels", value);
            }
        }

        public static IUFrameTypeProvider UFrameTypes
        {
            get { return _uFrameTypes ?? (_uFrameTypes = Container.Resolve<IUFrameTypeProvider>()); }
            set { _uFrameTypes = value; }
        }

        private static IBindingGenerator[] BindingGenerators { get; set; }

        public static IEnumerable<IBindingGenerator> GetBindingGeneratorsForView(ViewData view)
        {
            foreach (var binding in view.Bindings.ToArray())
            {
                var generator = Container.Resolve<IBindingGenerator>(binding.GeneratorType);
                if (generator == null)
                {
                    Debug.LogError("Binding Generator Not Found: " + binding.GeneratorType);
                    continue;
                }
                if (binding.Property == null)
                {
                    binding.PropertyIdentifier = null;
                    view.Bindings.Remove(binding);
                    Debug.Log("Couldnt find property for " + binding.Name);
                    continue;
                }
                generator.Element = view.ViewForElement;

                generator.Item = binding.Property;

                generator.GenerateDefaultImplementation = false;
                yield return generator;
            }

        }

        public static IEnumerable<IBindingGenerator> GetPossibleBindingGenerators(ViewData view, bool isOverride = true, bool generateDefaultBindings = true, bool includeBaseItems = true, bool callBase = true)
        {
            var mainElement = view.ViewForElement;
            
            if (view.BaseView != null)
            {
                foreach (var viewModelItem in mainElement.ViewModelItems)
                {
                    var bindingGenerators = Container.ResolveAll<IBindingGenerator>();
                    foreach (var bindingGenerator in bindingGenerators)
                    {
                        bindingGenerator.IsBase = mainElement != view.ViewForElement;
                        bindingGenerator.IsOverride = isOverride;
                        bindingGenerator.Item = viewModelItem;
                        bindingGenerator.Element = mainElement;
                        bindingGenerator.GenerateDefaultImplementation = generateDefaultBindings;
                        
                        if (bindingGenerator.IsApplicable)
                            yield return bindingGenerator;
                    }
                }
            }
            else
            {
                foreach (var element in mainElement.AllBaseTypes.Concat(new[] {mainElement}))
                {
                    foreach (var viewModelItem in element.ViewModelItems)
                    {
                        var bindingGenerators = Container.ResolveAll<IBindingGenerator>();
                        foreach (var bindingGenerator in bindingGenerators)
                        {
                            bindingGenerator.IsBase = mainElement != view.ViewForElement;
                            bindingGenerator.IsOverride = isOverride;
                            bindingGenerator.Item = viewModelItem;
                            bindingGenerator.Element = mainElement;
                            bindingGenerator.GenerateDefaultImplementation = generateDefaultBindings;

                            if (bindingGenerator.IsApplicable)
                                yield return bindingGenerator;
                        }
                    }
                }
               
            }

          


        }


        public static UFrameSettings Settings
        {
            get { return InvertGraphEditor.Settings as UFrameSettings; }
        }

        public static void Loaded()
        {
            BindingGenerators = Container.ResolveAll<IBindingGenerator>().ToArray();
            

        }


    }

}