﻿
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Invert.Common.UI
{
    public class GUIHelpers
    {
        public static Texture2D GetEditorUFrameResource(string name, int width, int height)
        {
#if !ASSEMBLY
            var asset = AssetDatabase.LoadAssetAtPath(@"Assets/uFrameComplete/uFrame/Editor/Resources/" + name, typeof(Texture2D)) as Texture2D;
            if (asset != null)
            {
                //asset._Width = _Width;
                //asset.height = height;
                return asset;
            }
            return null;
#else
                return LoadDllResource(resourceName, _Width, height);
#endif
        }
        public static Texture2D LoadDllResource(string resourceName, int width, int height)
        {
            // also lets you override dll resources locally for rapid iteration
            Texture2D texture = (Texture2D)Resources.Load(resourceName);
            if (texture != null)
            {
                Debug.Log("Loaded local resource: " + resourceName);
                return texture;
            }
            // if unavailable, try assembly
            Assembly myAssembly = Assembly.GetExecutingAssembly();
            Stream myStream = myAssembly.GetManifestResourceStream("assemblypathhere" + resourceName + ".png");
            texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
            texture.LoadImage(ReadToEnd(myStream));
            if (texture == null)
            {
                Debug.LogError("Missing Dll resource: " + resourceName);
            }
            return texture;
        }
        // loads a png resources from the dll
        private static byte[] ReadToEnd(Stream stream)
        {
            long originalPosition = stream.Position;
            stream.Position = 0;
            try
            {
                var readBuffer = new byte[4096];
                int totalBytesRead = 0;
                int bytesRead;
                while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;
                    if (totalBytesRead == readBuffer.Length)
                    {
                        int nextByte = stream.ReadByte();
                        if (nextByte != -1)
                        {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }
                byte[] buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }
                return buffer;
            }
            finally
            {
                stream.Position = originalPosition;
            }
        }
        public static Rect GetRect(GUIStyle style, bool fullWidth = true, params GUILayoutOption[] options)
        {
            var rect = GUILayoutUtility.GetRect(GUIContent.none, style, options);
            if (!fullWidth) return rect;
            //var indentAmount = (Indent * 25);
            rect.x -= 13;
            //rect.x += +(indentAmount);
            rect.width += 17;
            //rect.width -= indentAmount;
            rect.y += 3;
            return rect;
        }
        public static bool DoToolbar(string label, bool open, Action add = null, Action leftButton = null, Action paste = null, GUIStyle addButtonStyle = null, GUIStyle pasteButtonStyle = null,bool fullWidth = true)
        {
            var rect = GetRect(open ? UBStyles.ToolbarStyle : UBStyles.ToolbarStyleCollapsed, GUIHelpers.IsInsepctor);
            GUI.Box(rect, "", open ? UBStyles.ToolbarStyle : UBStyles.ToolbarStyleCollapsed);
            var labelStyle = new GUIStyle(EditorStyles.label)
            {
                normal = new GUIStyleState() {textColor = UBStyles.ToolbarStyle.normal.textColor },
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Bold,
                fontSize = 11
            };
            var labelRect = new Rect(rect.x + 2, rect.y + (rect.height / 2) - 8, rect.width - 50, 16);
            var result = open;
            if (leftButton == null)
            {
                result = GUI.Button(labelRect,
                    new GUIContent(label,
                        open ? UBStyles.ArrowDownTexture : UBStyles.CollapseRightArrowTexture),
                    labelStyle);
            }
            else
            {
                if (GUI.Button(labelRect, new GUIContent(label, UBStyles.ArrowLeftTexture), labelStyle))
                {
                    leftButton();
                }
            }

            if (paste != null)
            {
                var addButtonRect = new Rect(rect.x + rect.width - 42, rect.y + (rect.height / 2) - 8, 16, 16);
                if (GUI.Button(addButtonRect, "", pasteButtonStyle ?? UBStyles.PasteButtonStyle))
                {
                    paste();
                }
            }

            if (add != null)
            {
                var addButtonRect = new Rect(rect.x + rect.width - 21, rect.y + (rect.height / 2) - 8, 16, 16);
                if (GUI.Button(addButtonRect, "", addButtonStyle ?? UBStyles.AddButtonStyle))
                {
                    add();
                }
            }
            return result;
        }

        public static bool IsInsepctor { get; set; }

        public static bool DoToolbar(string label, Action add = null, Action leftButton = null, Action paste = null)
        {
            return DoToolbar(label, true, add, leftButton, paste);
        }
        public static bool DoToolbarEx(string label, Action add = null, Action leftButton = null, Action paste = null)
        {
            var tBar = DoToolbar(label, EditorPrefs.GetBool(label, false), add, leftButton, paste);
            if (tBar)
            {
                EditorPrefs.SetBool(label,!EditorPrefs.GetBool(label));
            }
            return EditorPrefs.GetBool(label);
        }
        public static bool DoTriggerButton(UFStyle ubTriggerContent)
        {
            var hasSubLabel = !String.IsNullOrEmpty(ubTriggerContent.SubLabel);

            var rect = GetRect(ubTriggerContent.BackgroundStyle,ubTriggerContent.FullWidth && !ubTriggerContent.IsWindow);

            var style = ubTriggerContent.BackgroundStyle;

            if (UFStyle.MouseDownStyle != null && ubTriggerContent.IsMouseDown(rect))
                style = UFStyle.MouseDownStyle;

            if (!ubTriggerContent.Enabled)
            {
                style = GUIStyle.none;
            }

            GUI.Box(rect, "", style);

            if (ubTriggerContent.MarkerStyle != null)
            {
                var rectIndicator = new Rect(rect);
                rectIndicator.width = 2;
                rectIndicator.y -= 2;
                rectIndicator.x = rect.width - 2;
                rectIndicator.height -= 3;
                GUI.Box(rectIndicator, "", ubTriggerContent.MarkerStyle);
            }
            if (ubTriggerContent.IconStyle != null )
            {
                var eventOptionsButtonRect = new Rect(rect.x + 5, rect.y + ((rect.height / 2) - 8), 16, 16);
                if (GUI.Button(eventOptionsButtonRect, "", ubTriggerContent.IconStyle))
                {
                    if (ubTriggerContent.OnShowOptions != null)
                    ubTriggerContent.OnShowOptions();
                }
                var seperatorRect = new Rect(rect) {width = 3};
                seperatorRect.y += 2;
                seperatorRect.height -= 5;
                seperatorRect.x = eventOptionsButtonRect.x + 17;
                GUI.Box(seperatorRect, String.Empty, UBStyles.SeperatorStyle);
            }

            var labelStyle =  new GUIStyle(EditorStyles.label) { alignment = ubTriggerContent.TextAnchor, fontSize = 11,fontStyle = FontStyle.Bold};
            if (!ubTriggerContent.Enabled)
            {
                labelStyle.normal.textColor = new Color(0.4f,0.4f,0.4f);
                
            }
            var labelRect = new Rect(rect.x, rect.y - (hasSubLabel ? 6 : 0), rect.width - 30, rect.height);
            var lbl = ubTriggerContent.Label;
            var result = GUI.Button(labelRect, lbl, labelStyle);

            if (hasSubLabel)
            {
                var subLabelRect = new Rect(labelRect);
                subLabelRect.y += 18;
                subLabelRect.width -= 26;
                subLabelRect.x += 13;
                GUI.Label(subLabelRect, ubTriggerContent.SubLabel, UFStyle.SubLabelStyle);
            }
            if (ubTriggerContent.ShowArrow)
                GUI.DrawTexture(new Rect(rect.x + rect.width - 18f, rect.y + ((rect.height / 2) - 8), 16, 16), UBStyles.ArrowRightTexture);
            if (ubTriggerContent.Enabled)
            {
                return result;
            }
            return result;
        }

        public static bool DoToggle(string name, bool on)
        {
            if (DoTriggerButton(new UFStyle()
            {
                Label = name,
                BackgroundStyle = UBStyles.EventButtonStyleSmall,
                IconStyle = on ? UBStyles.TriggerActiveButtonStyle : UBStyles.TriggerInActiveButtonStyle
            }))
            {
                return !on;
            }
            return on;
        }
    }
}
