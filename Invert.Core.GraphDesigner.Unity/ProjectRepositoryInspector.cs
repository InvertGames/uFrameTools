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
using Invert.uFrame;
using Invert.uFrame.Editor;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(ProjectRepository))]
public class ProjectRepositoryInspector : Editor
{
    private TypeMapping[] _generators;
    private CodeFileGenerator[] fileGenerators;
    private List<PropertyFieldDrawer> _selectedItemDrawers;
    private List<IDrawer> _generatorDrawers;

    public ProjectRepository Target
    {
        get { return target as ProjectRepository; }
    }

    public TypeMapping[] CodeGenerators
    {
        get { return _generators ?? (_generators = InvertApplication.Container.Mappings.Where(p => p.From == typeof(DesignerGeneratorFactory)).ToArray()); }
    }

    public void OnEnable()
    {

    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        Target.Diagrams.RemoveAll(p => p == null);

        serializedObject.Update();
        GUIHelpers.IsInsepctor = true;

        if (GUIHelpers.DoToolbarEx("Project Diagrams", ImportDiagram))
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
                        var items = Target.Diagrams.ToList();
                        items.RemoveAt(index1);

                        var so = new SerializedObject(target);
                        so.Update();
                        var property = serializedObject.FindProperty("_diagrams");
                        property.ClearArray();
                        property.arraySize = items.Count;
                        for (int i = 0; i < items.Count; i++)
                        {
                            var item = items[i];
                            property.GetArrayElementAtIndex(i).objectReferenceValue = item;
                        }
                        so.ApplyModifiedProperties();
                    }
                })
                ;
            }

        }

        //if (GUIHelpers.DoToolbarEx("Settings"))
        //{
        //    var settingsProperty = serializedObject.FindProperty("_generatorSettings");


        //    var property = settingsProperty.FindPropertyRelative("_namespaceProvider").FindPropertyRelative("_rootNamespace");
        //    EditorGUILayout.PropertyField(property);


        //    property = settingsProperty.FindPropertyRelative("_generateComments");
        //    var newValue = GUIHelpers.DoToggle("Generate Comments", property.boolValue);
        //    if (newValue != property.boolValue)
        //    {
        //        property.boolValue = newValue;

        //    }
        //}
      
        if (InvertGraphEditor.CurrentDiagramViewModel != null)
        DoDiagramInspector();


#if DEBUG
        if (GUIHelpers.DoToolbarEx("Project Nodes"))
        {

            foreach (var item in InvertGraphEditor.CurrentProject.AllGraphItems)
            {
                if (item == null) continue;
                GUIHelpers.DoTriggerButton(new UFStyle()
                {
                    Label = item.Label,
                    Enabled = true,
                    BackgroundStyle = ElementDesignerStyles.EventButtonStyleSmall,
                    TextAnchor = TextAnchor.MiddleRight,
                    //IconStyle = UBStyles.RemoveButtonStyle,
                    ShowArrow = true
                });

            }
        }
        if (GUIHelpers.DoToolbarEx("Project Connections"))
        {

            foreach (var item in InvertGraphEditor.CurrentProject.Connections)
            {
                if (item.Output == null || item.Input == null) continue;
                GUIHelpers.DoTriggerButton(new UFStyle()
                {
                    Label = string.Format("{0} -> {1}", item.Output.Label, item.Input.Label),
                    Enabled = true,
                    BackgroundStyle = ElementDesignerStyles.EventButtonStyleSmall,
                    TextAnchor = TextAnchor.MiddleRight,
                    //IconStyle = UBStyles.RemoveButtonStyle,
                    ShowArrow = true
                });

            }
        }
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

    private void DoDiagramInspector()
    {
        var selected = InvertGraphEditor.CurrentDiagramViewModel.SelectedGraphItem;
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
                item.CachedValue = item.ViewModel.Getter();
                item.DrawInspector(true);
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
                    
                    fileGenerator.Refresh(new Vector2(lastRect.x,lastRect.y + lastRect.height));
                    GUILayoutUtility.GetRect(fileGenerator.Bounds.width, fileGenerator.Bounds.height);
                    fileGenerator.Draw(1f);
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
        var item = SelectedItem;

        if (!(item is IDiagramNode))
        {
            var nodeItem = item as IDiagramNodeItem;
            if (nodeItem != null)
            {
                item = nodeItem.Node;
            }
            else
            {
                return;
            }
            
        }
        if (item == null) return;

        fileGenerators = InvertGraphEditor.GetAllFileGenerators(null,
            InvertGraphEditor.CurrentProject,true).ToArray();
        GeneratorDrawers.Clear();
        foreach (var fileGenerator in fileGenerators)
        {
            var list = fileGenerator.Generators.ToList();
            list.RemoveAll(p => p.ObjectData != item);
            fileGenerator.Generators = list.ToArray();
            if (fileGenerator.Generators.Length < 1) continue;

            var syntaxViewModel = new SyntaxViewModel(fileGenerator.ToString(), fileGenerator.Generators[0].Filename, 0);
            var syntaxDrawer = new SyntaxDrawer(syntaxViewModel);
            GeneratorDrawers.Add(syntaxDrawer);
        }
     
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
        var projectAssets = InvertGraphEditor.Projects.SelectMany(p => p.Graphs);
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
}
