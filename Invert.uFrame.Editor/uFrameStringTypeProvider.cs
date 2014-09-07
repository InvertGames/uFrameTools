using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Invert.uFrame.Editor;

namespace Invert.uFrame.Editor
{
    public class uFrameStringTypeProvider : IUFrameTypeProvider
    {
        public Type _ViewModel;
        public virtual Type ViewModel { get { return _ViewModel ?? (_ViewModel = uFrameEditor.FindType("ViewModel")); } }
        public Type _Controller;
        public virtual Type Controller { get { return _Controller ?? (_Controller = uFrameEditor.FindType("Controller")); } }
        public Type _SceneManager;
        public virtual Type SceneManager { get { return _SceneManager ?? (_SceneManager = uFrameEditor.FindType("SceneManager")); } }
        public Type _GameManager;
        public virtual Type GameManager { get { return _GameManager ?? (_GameManager = uFrameEditor.FindType("GameManager")); } }
        public Type _ViewComponent;
        public virtual Type ViewComponent { get { return _ViewComponent ?? (_ViewComponent = uFrameEditor.FindType("ViewComponent")); } }
        public Type _ViewBase;
        public virtual Type ViewBase { get { return _ViewBase ?? (_ViewBase = uFrameEditor.FindType("ViewBase")); } }
        public Type _UFToggleGroup;
        public virtual Type UFToggleGroup { get { return _UFToggleGroup ?? (_UFToggleGroup = uFrameEditor.FindType("UFToggleGroup")); } }
        public Type _UFGroup;
        public virtual Type UFGroup { get { return _UFGroup ?? (_UFGroup = uFrameEditor.FindType("UFGroup")); } }
        public Type _UFRequireInstanceMethod;
        public virtual Type UFRequireInstanceMethod { get { return _UFRequireInstanceMethod ?? (_UFRequireInstanceMethod = uFrameEditor.FindType("UFRequireInstanceMethod")); } }
        public Type _DiagramInfoAttribute;
        public virtual Type DiagramInfoAttribute { get { return _DiagramInfoAttribute ?? (_DiagramInfoAttribute = uFrameEditor.FindType("DiagramInfoAttribute")); } }
        public Type _UpdateProgressDelegate;

        public virtual Type UpdateProgressDelegate { get { return _UpdateProgressDelegate ?? (_UpdateProgressDelegate = uFrameEditor.FindType("UpdateProgressDelegate")); } }
        public Type _CommandWithSenderT;
        public virtual Type CommandWithSenderT { get { return _CommandWithSenderT ?? (_CommandWithSenderT = uFrameEditor.FindType("CommandWithSender`1")); } }
        public Type _CommandWith;
        public virtual Type CommandWith { get { return _CommandWith ?? (_CommandWith = uFrameEditor.FindType("CommandWith`1")); } }
        public Type _CommandWithSenderAndArgument;
        public virtual Type CommandWithSenderAndArgument { get { return _CommandWithSenderAndArgument ?? (_CommandWithSenderAndArgument = uFrameEditor.FindType("CommandWithSenderAndArgument`2")); } }
        public Type _YieldCommandWithSenderT;
        public virtual Type YieldCommandWithSenderT { get { return _YieldCommandWithSenderT ?? (_YieldCommandWithSenderT = uFrameEditor.FindType("YieldCommandWithSender`1")); } }
        public Type _YieldCommandWith;
        public virtual Type YieldCommandWith { get { return _YieldCommandWith ?? (_YieldCommandWith = uFrameEditor.FindType("YieldCommandWith`1")); } }
        public Type _YieldCommandWithSenderAndArgument;
        public virtual Type YieldCommandWithSenderAndArgument { get { return _YieldCommandWithSenderAndArgument ?? (_YieldCommandWithSenderAndArgument = uFrameEditor.FindType("YieldCommandWithSenderAndArgument`2")); } }
        public Type _YieldCommand;
        public virtual Type YieldCommand { get { return _YieldCommand ?? (_YieldCommand = uFrameEditor.FindType("YieldCommand")); } }
        public Type _Command;
        public virtual Type Command { get { return _Command ?? (_Command = uFrameEditor.FindType("Command")); } }
        public Type _ICommand;
        public virtual Type ICommand { get { return _ICommand ?? (_ICommand = uFrameEditor.FindType("ICommand")); } }
        public Type _ListOfViewModel;
        public virtual Type ListOfViewModel { get { return _ListOfViewModel ?? (_ListOfViewModel = uFrameEditor.FindType("System.Collections.Generic.List`1[[ViewModel, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]]")); } }
        public Type _ISerializerStream;
        public virtual Type ISerializerStream { get { return _ISerializerStream ?? (_ISerializerStream = uFrameEditor.FindType("ISerializerStream")); } }
        public Type _P;
        public virtual Type P { get { return _P ?? (_P = uFrameEditor.FindType("P`1")); } }
        private  Type _Computed;
        public virtual Type Computed { get { return _Computed ?? (_Computed = uFrameEditor.FindType("Computed`1")); } }
        public Type _ModelCollection;
 
        public Type ModelCollection { get { return _ModelCollection ?? (_ModelCollection = uFrameEditor.FindType("ModelCollection`1")); } }
    }

}
