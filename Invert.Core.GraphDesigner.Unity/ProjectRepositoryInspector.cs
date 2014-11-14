using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Invert.Common;
using Invert.Common.UI;
using Invert.Core;
using Invert.Core.GraphDesigner;
using Invert.uFrame;
using Invert.uFrame.Editor;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(ProjectRepository))]
public class ProjectRepositoryInspector : Editor
{
    private TypeMapping[] _generators;

    public ProjectRepository Target
    {
        get { return target as ProjectRepository; }
    }

    public TypeMapping[] CodeGenerators
    {
        get { return _generators ?? (_generators = InvertApplication.Container.Mappings.Where(p => p.From == typeof(DesignerGeneratorFactory)).ToArray()); }
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
                    Label = diagram.Name,
                    Enabled = true,
                    BackgroundStyle = UBStyles.EventButtonStyleSmall,
                    TextAnchor = TextAnchor.MiddleRight,
                    IconStyle = UBStyles.RemoveButtonStyle,
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

        if (GUIHelpers.DoToolbarEx("Settings"))
        {
            var settingsProperty = serializedObject.FindProperty("_generatorSettings");


            var property = settingsProperty.FindPropertyRelative("_namespaceProvider").FindPropertyRelative("_rootNamespace");
            EditorGUILayout.PropertyField(property);


            property = settingsProperty.FindPropertyRelative("_generateComments");
            var newValue = GUIHelpers.DoToggle("Generate Comments", property.boolValue);
            if (newValue != property.boolValue)
            {
                property.boolValue = newValue;

            }
        }
        if (GUIHelpers.DoToolbarEx("Project Nodes"))
        {

            foreach (var item in Target.NodeItems)
            {
                GUIHelpers.DoTriggerButton(new UFStyle()
                {
                    Label = item.Name,
                    Enabled = true,
                    BackgroundStyle = UBStyles.EventButtonStyleSmall,
                    TextAnchor = TextAnchor.MiddleRight,
                    //IconStyle = UBStyles.RemoveButtonStyle,
                    ShowArrow = true
                });

            }
        }
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

    private void ImportDiagram()
    {
        var assets = InvertGraphEditor.AssetManager.GetAssets(typeof(GraphData)).OfType<GraphData>().ToArray();

        ItemSelectionWindow.Init("Select Diagram", assets, (item) =>
        {
            // Mimic the new list
            var list = Target.Diagrams.ToList();
            var selectedAsset = item as GraphData;
            list.Add(selectedAsset);

            var so = new SerializedObject(Target); so.Update();


            // Adjust the array
            var property = so.FindProperty("_diagrams");
            property.arraySize = list.Count;

            for (int index = 0; index < list.Count; index++)
            {
                var diagram = list[index];
                property.GetArrayElementAtIndex(index).objectReferenceValue = diagram;
            }

            so.ApplyModifiedProperties();
        });
    }
}
