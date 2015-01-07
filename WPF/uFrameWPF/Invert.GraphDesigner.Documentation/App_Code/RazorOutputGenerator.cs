//using System;
//using System.IO;
//using System.Threading.Tasks;
//using System.Web.Mvc;
//using Invert.Core.GraphDesigner;
//using Microsoft.AspNet.Razor;


//namespace Invert.GraphDesigner.Documentation
//{
//    //public class RazorOutputGenerator<T> : OutputGenerator
//    //{
//    //    public override Type GeneratorFor
//    //    {
//    //        get { throw new NotImplementedException(); }
//    //    }

//    //    public override void Execute()
//    //    {
//    //        throw new NotImplementedException();
//    //    }
//    //}

//    public static class uFrameRazorExtension
//    {
//        public static void Render(string templateFilename)
//        {
     
//            var language = new CSharpRazorCodeLanguage();
//            var host = new RazorEngineHost(language)
//            {
//                DefaultBaseClass = "uFrameRazorTemplateBase",
//                DefaultClassName = "uFrameRazorTemplate",
//                DefaultNamespace = "CompiledRazorTemplates",
//            };

//            // Everyone needs the System namespace, right?
//            host.NamespaceImports.Add("System");
//            RazorTemplateEngine engine = new RazorTemplateEngine(host);
//            GeneratorResults razorResult = engine.GenerateCode(
//                new StringReader(File.ReadAllText(templateFilename)));

//        }
//    }

//    public class UFRazorTemplate<TModel> : uFrameRazorTemplateBase<TModel>
//    {
//        public override Task ExecuteAsync()
//        {
            
//        }
//    }
//    public abstract class uFrameRazorTemplateBase<TModel> : OutputGenerator
//    {
//        public override Type GeneratorFor
//        {
//            get { return typeof(TModel); }
//            set {  }
//        }

//        public abstract Task ExecuteAsync();

//        public virtual void Write(object value)
//        { /* TODO: Write value */ }

//        public virtual void WriteLiteral(object value)
//        { /* TODO: Write literal */ }
//    }
//}