using Invert.Common;
using Invert.Windows;
using UnityEditor;
using UnityEngine;

public class QuickAccessWindowSearchPanel : Area<QuickAccessWindowViewModel>
{

    public int selectedIndex = 0;
    private static GUIStyle _textFieldStyle;

    public static GUIStyle TextFieldStyle
    {
        get
        {
            if (_textFieldStyle == null)
                _textFieldStyle = new GUIStyle(EditorStyles.textField)
                {
                    fontSize = 20,
                };
            return _textFieldStyle;
        }
    }

    public override void Draw(QuickAccessWindowViewModel data)
    {

        GUILayout.BeginHorizontal();

        EditorGUI.BeginChangeCheck();
        data.SearchText = GUILayout.TextField(data.SearchText, TextFieldStyle);
        if (EditorGUI.EndChangeCheck())
        {
            data.UpdateSearch();
        }
        
        GUILayout.EndHorizontal();

        for (var i = 0; i < data.QuickLaunchItems.Count; i++)
        {
            var item = data.QuickLaunchItems[i];
            if (GUILayout.Button(item.Title,ElementDesignerStyles.ButtonStyle))
            {
                data.ItemSelected(item);
            }
        }

    }
}