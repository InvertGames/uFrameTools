using System;
using Invert.Common;
using UnityEditor;
using UnityEngine;

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
        EditorGUIUtility.labelWidth = this.Bounds.width* 0.55f;
        EditorGUI.BeginChangeCheck();
        if (ViewModel.Type == typeof (string))
        {
            CachedValue = EditorGUILayout.TextField(ViewModel.Name, (string)CachedValue);
        }
        else if (ViewModel.Type == typeof (int))
        {
            CachedValue = EditorGUILayout.IntField(ViewModel.Name, (int)CachedValue);
        }
        else if (ViewModel.Type == typeof (float))
        {
            CachedValue = EditorGUILayout.FloatField(ViewModel.Name, (float)CachedValue);
        }
        else if (ViewModel.Type == typeof(Vector2))
        {
            CachedValue = EditorGUILayout.Vector2Field(ViewModel.Name, (Vector3)CachedValue);
        }

        else if (ViewModel.Type == typeof(Vector3))
        {
            CachedValue = EditorGUILayout.Vector3Field(ViewModel.Name, (Vector3)CachedValue);
        }

        else if (ViewModel.Type == typeof(Vector4))
        {
            CachedValue = EditorGUILayout.Vector4Field(ViewModel.Name, (Vector4)CachedValue);
        }
        else if (ViewModel.Type == typeof(bool))
        {
            CachedValue = EditorGUILayout.Toggle(ViewModel.Name, (bool)CachedValue);
        }
        else if (typeof (Enum).IsAssignableFrom(ViewModel.Type))
        {
            CachedValue = EditorGUILayout.EnumPopup(ViewModel.Name, (Enum) CachedValue);
        }

        if (EditorGUI.EndChangeCheck())
        {
            ViewModel.Setter(CachedValue);
        }
        GUILayout.EndArea();
    }
}