using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Invert.Core.GraphDesigner.Unity;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Invert.Core.GraphDesigner.Documentation
{
    public class DocumentationBuilder
    {

    }

    public class DocumentationPlugin : DiagramPlugin
    {
        public override bool Required
        {
            get { return true; }
        }

        public override void Initialize(uFrameContainer container)
        {
            //InvertGraphEditor.Container.RegisterInstance<IToolbarCommand>(new SaveNodeToImage(), "SaveNodeToImage");

        }
    }

    public class SaveNodeToImage : ToolbarCommand<DiagramViewModel>
    {
        public override void Perform(DiagramViewModel node)
        {
            foreach (var item in node.AllViewModels.OfType<ScreenshotNodeViewModel>())
            {
                item.SaveImage = true;
            }
            
        }

        public override string CanPerform(DiagramViewModel node)
        {
            return null;
        }

    }
    public static class Extensions
    {
        private static RenderTexture m_PreviousActiveTexture;
        public  static void SaveScreenshot(ScreenshotNodeViewModel node, Rect region)
        {
            if ((double)((Rect)region).height < 1.0)
                return;
            Rect local1 = @region;
            Rect position = node.Bounds;
            // ISSUE: explicit reference operation
            // ISSUE: explicit reference operation
            double num1 = (double)((Rect)@position).height - (double)((Rect)@region).height - 52.0;
            local1.y = ((float)num1);
            // ISSUE: explicit reference operation
            // ISSUE: variable of a reference type
            Rect local2 = @region;
            double num2 = (double)((Rect)local2).height + 10.0;
            local2.height = (float)num2;
            // ISSUE: explicit reference operation
            // ISSUE: explicit reference operation
            Texture2D texture2D = new Texture2D((int)((Rect)@region).width, (int)((Rect)@region).height, (TextureFormat)3, false);
            var rect = new Rect(region);
            rect.x += node.Bounds.x;
            rect.y += node.Bounds.y;
            texture2D.ReadPixels(rect, 0, 0);
            texture2D.Apply();
            //string fullPath = Path.GetFullPath(Application.get_dataPath() + "/../" + Path.Combine(this.screenshotsSavePath, actionName) + ".png");
            //if (!FsmEditorUtility.CreateFilePath(fullPath))
            //    return;
            byte[] bytes = texture2D.EncodeToPNG();
            Object.DestroyImmediate((Object)texture2D, true);
            File.WriteAllBytes("image.png", bytes);
        }
        public static void GetScreenshot(ScreenshotNodeViewModel node)
        {
            
            var x = new RenderTexture( node.GraphItem.Width , node.GraphItem.Height, 0);
            //Graphics.SetRenderTarget(x);
            //BeginRenderTextureGUI(x);
  
         

            var rect = new Rect(node.GraphItem.Location.x, node.GraphItem.Location.y, node.GraphItem.Width, node.GraphItem.Height);
            Texture2D ss = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGB24, false, true);
            ss.ReadPixels(rect, 0, 0);
            InvertGraphEditor.DesignerWindow.DiagramDrawer.Draw(InvertGraphEditor.PlatformDrawer, 1f);
            ss.Apply();
            //ss.SetPixel(m_PreviousActiveTexture.);
            //EndRenderTextureGUI();
            File.WriteAllBytes("image.png", ss.EncodeToPNG());
        }
        public static void BeginRenderTextureGUI(RenderTexture targetTexture)
        {
            //if (Event.current.type == EventType.Repaint)
            //{
                m_PreviousActiveTexture = RenderTexture.active;
                if (targetTexture != null)
                {
                    RenderTexture.active = targetTexture;
                    GL.Clear(false, true, Color.white);
                    
                }
            //}
        }


        public static void EndRenderTextureGUI()
        {
            //if (Event.current.type == EventType.Repaint)
            //{
                RenderTexture.active = null;
            //}
        }
    }
}
