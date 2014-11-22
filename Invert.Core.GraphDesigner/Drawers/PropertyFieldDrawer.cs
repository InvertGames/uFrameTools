using System;
using System.Linq;
using Invert.Common;
using UnityEditor;
using UnityEngine;

namespace Invert.Core.GraphDesigner
{
    public class PropertyFieldDrawer : ItemDrawer<PropertyFieldViewModel>
    {
        public PropertyFieldDrawer(PropertyFieldViewModel viewModelObject) : base(viewModelObject)
        {
        }

        public object CachedValue { get; set; }

        public override GUIStyle TextStyle
        {
            get { return EditorStyles.label; }
        }

        public override void Refresh(Vector2 position)
        {
            base.Refresh(position);
            CachedValue = ViewModel.Getter();
            var bounds = new Rect(Bounds);
            bounds.width *= 2;
            Bounds = bounds;
        }

        public override void Draw(float scale)
        {
            //base.Draw(scale);
            GUILayout.BeginArea(Bounds.Scale(scale), ElementDesignerStyles.SelectedItemStyle);
            EditorGUIUtility.labelWidth = this.Bounds.width*0.55f;
            DrawInspector();
            GUILayout.EndArea();
        }

        public void DrawInspector(bool refreshGraph = false)
        {
            EditorGUI.BeginChangeCheck();
            if (ViewModel.Type == typeof (string))
            {
                if (ViewModel.InspectorType == InspectorType.TextArea)
                {
                    EditorGUILayout.LabelField(ViewModel.Name);
                    CachedValue = EditorGUILayout.TextArea( (string)CachedValue,GUILayout.Height(50));
                }
                else if (ViewModel.InspectorType == InspectorType.TypeSelection)
                {
                    if (GUILayout.Button((string) CachedValue))
                    {
                        InvertGraphEditor.WindowManager.InitTypeListWindow(InvertApplication.GetDerivedTypes<System.Object>(true,true).Select(p=>new GraphTypeInfo()
                        {
                            Name = p.Name,
                            Group = p.Namespace,
                            Label = p.Name,
                            
                        }).ToArray()
                        , (type) =>
                        {
                            InvertGraphEditor.ExecuteCommand(d=>ViewModel.Setter(type.Name));
                        });
                    }
                }
                else
                {
                    CachedValue = EditorGUILayout.TextField(ViewModel.Name, (string)CachedValue);
                }
                
            }
            else if (ViewModel.Type == typeof (int))
            {
                CachedValue = EditorGUILayout.IntField(ViewModel.Name, (int) CachedValue);
            }
            else if (ViewModel.Type == typeof (float))
            {
                CachedValue = EditorGUILayout.FloatField(ViewModel.Name, (float) CachedValue);
            }
            else if (ViewModel.Type == typeof (Vector2))
            {
                CachedValue = EditorGUILayout.Vector2Field(ViewModel.Name, (Vector3) CachedValue);
            }

            else if (ViewModel.Type == typeof (Vector3))
            {
                CachedValue = EditorGUILayout.Vector3Field(ViewModel.Name, (Vector3) CachedValue);
            }

            else if (ViewModel.Type == typeof (Vector4))
            {
                CachedValue = EditorGUILayout.Vector4Field(ViewModel.Name, (Vector4) CachedValue);
            }
            else if (ViewModel.Type == typeof (bool))
            {
                CachedValue = EditorGUILayout.Toggle(ViewModel.Name, (bool) CachedValue);
            }
            else if (typeof (Enum).IsAssignableFrom(ViewModel.Type))
            {
                CachedValue = EditorGUILayout.EnumPopup(ViewModel.Name, (Enum) CachedValue);
            }
            else if (ViewModel.Type == typeof (Type))
            {
                //InvertGraphEditor.WindowManager.InitTypeListWindow();
            }

            if (EditorGUI.EndChangeCheck())
            {

                ViewModel.Setter(CachedValue);
                if (refreshGraph)
                {

                    InvertGraphEditor.DesignerWindow.RefreshContent();
                }
            }
        }
    }
}