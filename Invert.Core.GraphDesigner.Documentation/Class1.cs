using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Invert.Core.GraphDesigner.Unity;
using UnityEditor;
using UnityEngine;

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
            InvertGraphEditor.Container.RegisterInstance<IToolbarCommand>(new SaveNodeToImage(), "SaveNodeToImage");

        }
    }

    public class SaveNodeToImage : ToolbarCommand<DiagramNodeViewModel>
    {
        public override void Perform(DiagramNodeViewModel node)
        {
            Extensions.GetScreenshot(node);
        }

        public override string CanPerform(DiagramNodeViewModel node)
        {
            return null;
        }

    }
    public static class Extensions
    {
        private static RenderTexture m_PreviousActiveTexture;

        public static void GetScreenshot(DiagramNodeViewModel node)
        {
            
            var x = new RenderTexture(Screen.width   , Screen.height, 0);
            //Graphics.SetRenderTarget(x);
            //BeginRenderTextureGUI(x);
          
          //  GUILayout.Box("This box goes on a render texture!");
            
       //  GUI.Label(new Rect(0f,0f,500,500),"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA",EditorStyles.largeLabel );
            var rect = new Rect(node.Bounds.x - 25, node.Bounds.y + 75, node.Bounds.width + 50, node.Bounds.height + 50);
            Texture2D ss = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGB24, false, true);
            ss.ReadPixels(rect, 0, 0);
        
            ss.Apply(false);
            //ss.SetPixel(m_PreviousActiveTexture.);
            //EndRenderTextureGUI();
            File.WriteAllBytes("image.png", ss.EncodeToPNG());
         //   var diagramViewModel = new DiagramViewModel(node.Diagram, node.Project);
         //   var vm = InvertGraphEditor.Container.GetNodeViewModel(node, diagramViewModel);
         //   var drawer = InvertGraphEditor.Container.CreateDrawer(vm);
         //    drawer.Refresh(new Vector2(0f,0f));


         //   var width = Mathf.RoundToInt(vm.Bounds.width) + 50;
         //   var height = Mathf.RoundToInt(vm.Bounds.height + 50);

         //   var oldrt = RenderTexture.active;
         //   var temp = new RenderTexture(width, height,0);
         //   RenderTexture.active = temp;
         //   //RenderTexture.active = temp;
       
         //   Texture2D ss = new Texture2D(width, height,TextureFormat.RGB24,false,false);
         //   var newBoundes = new Rect(drawer.Bounds);
         //   newBoundes.x = 0;
         //   newBoundes.y = 0;
         //   vm.Bounds = newBoundes;

          

         //   Debug.Log(string.Format("X: {2} Y: {3} Width: {0} Height: {1}", width, height, vm.Bounds.x, vm.Bounds.y));
         //   drawer.Draw(1f);
         
            
         //  // ss.Apply();
            
         //   //RenderTexture.ReleaseTemporary(temp);
         //   var pngBytes = ss.EncodeToPNG();
         //   File.WriteAllBytes("image.png",pngBytes);
         //RenderTexture.active = oldrt;
            //EditorApplication.



        }
        public static void BeginRenderTextureGUI(RenderTexture targetTexture)
        {
            //if (Event.current.type == EventType.Repaint)
            //{
                m_PreviousActiveTexture = RenderTexture.active;
                if (targetTexture != null)
                {
                    RenderTexture.active = targetTexture;
                    GL.Clear(false, true, Color.gray);
                    
                }
            //}
        }


        public static void EndRenderTextureGUI()
        {
            //if (Event.current.type == EventType.Repaint)
            //{
                RenderTexture.active = m_PreviousActiveTexture;
            //}
        }
    }
}
