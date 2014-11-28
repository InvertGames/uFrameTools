using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Invert.Core.GraphDesigner
{
    public interface IPlatformOperations
    {
        void OpenScriptFile(string filePath);
        string GetAssetPath(object graphData);
        bool MessageBox(string title, string message, string ok);
        bool MessageBox(string title, string message, string ok, string cancel);
        void SaveAssets();
        void RefreshAssets();
    }

    public interface IPlatformPreferences
    {
        bool GetBool(string name, bool def);
        string GetString(string name, string def);
        float GetFloat(string name, float def);
        float GetInt(string name, int def);
        void SetBool(string name, bool value);
        void SetString(string name, string value);
        void SetFloat(string name, float value);
        void SetInt(string name, int value);
        bool HasKey(string name);
        void DeleteKey(string name);
        void DeleteAll();
    }

    public interface IPlatformDrawer
    {
        void DrawPolyLine(Vector3[] lines);

        void DrawBezier(Vector3 startPosition, Vector3 endPosition, Vector3 startTangent, Vector3 endTangent,
            Color color, float width);

        //void DrawConnector(float scale, ConnectorViewModel viewModel);
    }
}
