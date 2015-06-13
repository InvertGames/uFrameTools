using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Invert.Common;
using Invert.Common.UI;
using Invert.Core;
using Invert.Core.GraphDesigner;
using Invert.Core.GraphDesigner.Unity;
using Invert.Core.GraphDesigner.Unity.Refactoring;
using Invert.IOC;
using Invert.uFrame;
using Invert.uFrame.Editor;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(ProjectRepository))]
public class ProjectRepositoryInspector : Editor , ICommandEvents
{
    private TypeMapping[] _generators;
    private CodeFileGenerator[] fileGenerators;
    private List<PropertyFieldDrawer> _selectedItemDrawers;
    private List<IDrawer> _generatorDrawers;
    private IEnumerable<ErrorInfo> _issues;

    public ProjectRepository Target
    {
        get { return target as ProjectRepository; }
    }

    public TypeMapping[] CodeGenerators
    {
        get { return _generators ?? (_generators = InvertApplication.Container.Mappings.Where(p => p.From == typeof(DesignerGeneratorFactory)).ToArray()); }
    }

    private Action destory;
    public void OnEnable()
    {
        _issues = null;
        destory = InvertApplication.ListenFor<ICommandEvents>(this);
    }

    public void OnDisable()
    {
  
        _issues = null;
        if (destory != null)
        destory();
    }

    public void OnDestroy()
    {

        if (destory != null)
        destory();
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        Target.Diagrams.RemoveAll(p => p == null);

        serializedObject.Update();
        GUIHelpers.IsInsepctor = true;


        if (GUIHelpers.DoToolbarEx("Project Graphs", ImportDiagram))
        {
            for (int index = 0; index < Target.Diagrams.Count; index++)
            {
                var diagram = Target.Diagrams[index];
                int index1 = index;
                GUIHelpers.DoTriggerButton(new UFStyle()
                {
                    Label = diagram.name,
                    Enabled = true,
                    BackgroundStyle = ElementDesignerStyles.EventButtonStyleSmall,
                    TextAnchor = TextAnchor.MiddleRight,
                    IconStyle = ElementDesignerStyles.RemoveButtonStyle,
                    ShowArrow = true,
                    OnShowOptions = () =>
                    {
                        //var removeFile = InvertGraphEditor.Platform.MessageBox("Confirm Delete",
                        //    "Would you like to remove the file as well?", "Yes",
                        //    "No");
                  


                        var items = Target.Diagrams.ToList();
                        var so = new SerializedObject(target);
                        so.Update();
                        var property = serializedObject.FindProperty("_diagrams");
                        property.ClearArray();      
                        items.RemoveAt(index1);
                        property.arraySize = items.Count;
                        for (int i = 0; i < items.Count; i++)
                        {
                            var item = items[i];
                            property.GetArrayElementAtIndex(i).objectReferenceValue = item;
                        }

                        //if (removeFile)
                        //{
                        //    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(items[index1]));
                        //}
                        //Target.Diagrams.Remove(items[index1]);
                  
                        so.ApplyModifiedProperties();
                        EditorUtility.SetDirty(target);
                        var project = InvertApplication.Container.Resolve<ProjectService>();
                        project.RefreshProjects();
                    }
                })
                ;
            }
      

        }
       
        if (InvertGraphEditor.CurrentDiagramViewModel != null)
        if (GUIHelpers.DoToolbarEx("Project Issues"))
        {
            foreach (var item in Issues)
            {
                if (GUIHelpers.DoTriggerButton(new UFStyle()
                {
                    Label = item.Message,
                    Enabled = true,
                    BackgroundStyle = ElementDesignerStyles.EventButtonStyleSmall,
                    TextAnchor = TextAnchor.MiddleRight,
                    IconStyle = ElementDesignerStyles.BreakpointButtonStyle,
                    //IconStyle = UBStyles.RemoveButtonStyle,
                    ShowArrow = true
                }))
                {
                    InvertGraphEditor.NavigateTo(item.Identifier); 
                }
            }
        }
        if (GUIHelpers.DoToolbarEx("Changes"))
        {
            foreach (var graph in Target.Graphs)
            {
                foreach (var change in graph.ChangeData)
                {
                    if (GUIHelpers.DoTriggerButton(new UFStyle()
                    {
                        Label = change.ToString(),
                        Enabled = true,
                        BackgroundStyle = ElementDesignerStyles.EventButtonStyleSmall,
                        TextAnchor = TextAnchor.MiddleRight,
                        IconStyle = ElementDesignerStyles.BreakpointButtonStyle,
                        //IconStyle = UBStyles.RemoveButtonStyle,
                        ShowArrow = true
                    }))
                    {
                        InvertGraphEditor.NavigateTo(change.ItemIdentifier);
                    }
                }
            }
            if (GUILayout.Button("Clear Change Data"))
            {
                foreach (var item in Target.Graphs)
                {
                    item.ChangeData.Clear();
                }
            }
        }
        DoDiagramInspector();
        InvertApplication.SignalEvent<IProjectInspectorEvents>(_=>_.DoInspector(Target));
#if DEBUG
        //if (GUIHelpers.DoToolbarEx("Project Nodes"))
        //{
        //    if (InvertGraphEditor.CurrentDiagramViewModel != null)
        //        foreach (var item in InvertGraphEditor.CurrentDiagramViewModel.CurrentRepository.AllGraphItems)
        //        {
        //            if (item == null) continue;
        //            GUIHelpers.DoTriggerButton(new UFStyle()
        //            {
        //                Label = item.Label + " " +item.Identifier,
        //                Enabled = true,
        //                BackgroundStyle = ElementDesignerStyles.EventButtonStyleSmall,
        //                TextAnchor = TextAnchor.MiddleRight,
        //                //IconStyle = UBStyles.RemoveButtonStyle,
        //                ShowArrow = true
        //            });

        //        }
        //}
        //if (GUIHelpers.DoToolbarEx("Project Connections"))
        //{
        //    if (InvertGraphEditor.CurrentDiagramViewModel != null)
        //        foreach (var item in InvertGraphEditor.CurrentDiagramViewModel.CurrentRepository.Connections)
        //        {
        //            if (item.Output == null || item.Input == null) continue;
        //            GUIHelpers.DoTriggerButton(new UFStyle()
        //            {
        //                Label = string.Format("{0} -> {1}", item.Output.Label, item.Input.Label),
        //                Enabled = true,
        //                BackgroundStyle = ElementDesignerStyles.EventButtonStyleSmall,
        //                TextAnchor = TextAnchor.MiddleRight,
        //                //IconStyle = UBStyles.RemoveButtonStyle,
        //                ShowArrow = true
        //            });

        //        }
        //}
 
#endif
        //if (GUIHelpers.DoToolbarEx("Code Generators"))
        //{
        //    var codeGenerators = InvertGraphEditor.CodeGenerators;
        //    var groupList = new List<ShowInSettings>();

        //    foreach (var codeGenerator in codeGenerators)
        //    {
        //        var customAttribute = codeGenerator.GetCustomAttributes(typeof(ShowInSettings), true).OfType<ShowInSettings>().FirstOrDefault();
        //        if (customAttribute == null) continue;
        //        groupList.Add(customAttribute);

        //    }
        //    foreach (var item in groupList.Select(p => p.Group).Distinct())
        //    {
        //        var isOn = Target[item, true];
        //        var result = GUIHelpers.DoToggle(item, isOn);
        //        if (isOn != result)
        //        {
        //            Target[item] = result;
        //        }
        //    }
        //}
        serializedObject.ApplyModifiedProperties();
        GUIHelpers.IsInsepctor = false;
    }

    public IEnumerable<ErrorInfo> Issues
    {
        get { return _issues ?? (_issues = InvertGraphEditor.CurrentDiagramViewModel.Issues.ToArray()); }
    }
    private void DoDiagramInspector()
    {
        if (InvertGraphEditor.CurrentDiagramViewModel == null) return;
        var selected = InvertGraphEditor.CurrentDiagramViewModel.SelectedGraphItem;
        //if (selected == null)
        //{
        //    selected = InvertGraphEditor.CurrentDiagramViewModel.SelectedNode;
        //}
        if (selected == null) return;
        if (SelectedItem != selected.DataObject)
        {
            SelectedItem = selected.DataObject as IGraphItem;

            if (SelectedItem != null)
            SelectedItemChanged();
       
        }
        if (GUIHelpers.DoToolbarEx("Selected"))
        {
            foreach (var item in SelectedItemDrawers)
            {
                item.ViewModel.CachedValue = item.ViewModel.Getter();
                var unityDrawer = InvertGraphEditor.PlatformDrawer as UnityDrawer;
                unityDrawer.DrawInspector(item.ViewModel);
            }

            var drawer = InvertGraphEditor.DesignerWindow.DiagramDrawer;
            if (drawer != null)
            {
                var dr = drawer.Children.OfType<IInspectorDrawer>().FirstOrDefault(p => p.ViewModelObject.IsSelected);
                if (dr != null)
                {
                    dr.DrawInspector(InvertGraphEditor.PlatformDrawer);
                }
            }

        }

        return;
        //var connectable = SelectedItem as IConnectable;
        //if (connectable != null)
        //{
        //    if (GUIHelpers.DoToolbarEx("Outputs"))
        //    {
        //        foreach (var item in connectable.Outputs)
        //        {
        //            GUIHelpers.DoTriggerButton(new UFStyle()
        //            {
        //                Label = item.Input.GetType().Name,
        //                Enabled = true,
        //                BackgroundStyle = UBStyles.EventButtonStyleSmall,
        //                TextAnchor = TextAnchor.MiddleRight,
        //                //IconStyle = UBStyles.RemoveButtonStyle,
        //                ShowArrow = true
        //            });
        //        }
        //    }
        //    if (GUIHelpers.DoToolbarEx("Inputs"))
        //    {
        //        foreach (var item in connectable.Inputs)
        //        {
        //            GUIHelpers.DoTriggerButton(new UFStyle()
        //            {
        //                Label = item.Output.GetType().Name,
        //                Enabled = true,
        //                BackgroundStyle = UBStyles.EventButtonStyleSmall,
        //                TextAnchor = TextAnchor.MiddleRight,
        //                //IconStyle = UBStyles.RemoveButtonStyle,
        //                ShowArrow = true
        //            });
        //        }
        //    }
        //}
        
        if (fileGenerators != null)
        {
            foreach (var fileGenerator in GeneratorDrawers)
            {
                if (GUIHelpers.DoToolbarEx(fileGenerator.ViewModelObject.Name))
                {
                    var lastRect = GUILayoutUtility.GetLastRect();

                    fileGenerator.Refresh(InvertGraphEditor.PlatformDrawer, new Vector2(lastRect.x, lastRect.y + lastRect.height));
                    GUILayoutUtility.GetRect(fileGenerator.Bounds.width, fileGenerator.Bounds.height);
                    fileGenerator.Draw(InvertGraphEditor.PlatformDrawer, 1f);
                    //EditorGUILayout.TextArea(fileGenerator.ToString());
                }
            }
            //foreach (var fileGenerator in fileGenerators)
            //{
             

            //    if (fileGenerator.Generators.Length < 1) continue;
            //    if (GUIHelpers.DoToolbarEx(fileGenerator.Generators[0].Filename))
            //    {
            //        EditorGUILayout.TextArea(fileGenerator.ToString());
            //    }
            //}
        }
    }

    private void SelectedItemChanged()
    {
        SelectedItemDrawers.Clear();
        GeneratorDrawers.Clear();

        fileGenerators = null;
        foreach (var property in SelectedItem.GetPropertiesWithAttribute<InspectorProperty>())
        {
            PropertyInfo property1 = property.Key;
            var propertyViewModel = new PropertyFieldViewModel(null)
            {
                CustomDrawerType = property.Value.CustomDrawerType,
                InspectorType = property.Value.InspectorType,
                Type = property1.PropertyType,
                Name = property1.Name,
                Getter = () => property1.GetValue(SelectedItem, null),
                Setter = v => property1.SetValue(SelectedItem, v, null)
            };
            var drawer = InvertGraphEditor.Container.CreateDrawer(propertyViewModel) as PropertyFieldDrawer;
      
            SelectedItemDrawers.Add(drawer);
        }
        return;
        //var item = SelectedItem;

        //if (!(item is IDiagramNode))
        //{
        //    var nodeItem = item as IDiagramNodeItem;
        //    if (nodeItem != null)
        //    {
        //        item = nodeItem.Node;
        //    }
        //    else
        //    {
        //        return;
        //    }
            
        //}
        //if (item == null) return;

        //fileGenerators = InvertGraphEditor.GetAllFileGenerators(null,
        //    InvertGraphEditor.CurrentProject,true).ToArray();
        //GeneratorDrawers.Clear();
        //foreach (var fileGenerator in fileGenerators)
        //{
        //    var list = fileGenerator.Generators.ToList();
        //    list.RemoveAll(p => p.ObjectData != item);
        //    fileGenerator.Generators = list.ToArray();
        //    if (fileGenerator.Generators.Length < 1) continue;

        //    var syntaxViewModel = new SyntaxViewModel(fileGenerator.ToString(), fileGenerator.Generators[0].Filename, 0);
        //    var syntaxDrawer = new SyntaxDrawer(syntaxViewModel);
        //    GeneratorDrawers.Add(syntaxDrawer);
        //}
     
    }

    public List<IDrawer> GeneratorDrawers
    {
        get { return _generatorDrawers ?? (_generatorDrawers = new List<IDrawer>()); }
        set { _generatorDrawers = value; }
    }

    public List<PropertyFieldDrawer> SelectedItemDrawers
    {
        get { return _selectedItemDrawers ?? (_selectedItemDrawers = new List<PropertyFieldDrawer>()); }
        set { _selectedItemDrawers = value; }
    }

    public IGraphItem SelectedItem { get; set; }
    private void ImportDiagram()
    {
        var projectService = InvertGraphEditor.Container.Resolve<ProjectService>();
        var projectAssets = projectService.Projects.SelectMany(p => p.Graphs);
        var assets = InvertGraphEditor.AssetManager.GetAssets(typeof(ScriptableObject)).OfType<IGraphData>().Where(p => !projectAssets.Contains(p)).ToArray();


        ItemSelectionWindow.Init("Select Diagram", assets, (item) =>
        {
            // Mimic the new list
            var list = Target.Graphs.ToList();
            var selectedAsset = item as IGraphData;
            list.Add(selectedAsset);

            var so = new SerializedObject(Target); so.Update();


            // Adjust the array
            var property = so.FindProperty("_diagrams");
            property.arraySize = list.Count;

            for (int index = 0; index < list.Count; index++)
            {
                var diagram = list[index];
                property.GetArrayElementAtIndex(index).objectReferenceValue = diagram as ScriptableObject;
            }

            so.ApplyModifiedProperties();
        });
    }

    public void CommandExecuting(ICommandHandler handler, IEditorCommand command, object o)
    {
        
    }

    public void CommandExecuted(ICommandHandler handler, IEditorCommand command, object o)
    {
        _issues = null;
    }
}
