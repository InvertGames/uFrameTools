using System;

namespace Invert.uFrame.Editor
{
    public interface IUFrameTypeProvider
    {
        Type ViewModel { get; }
        Type Controller { get; }
        Type SceneManager { get; }
        Type GameManager { get; }
        Type ViewComponent { get; }
        Type ViewBase { get; }

        Type UFToggleGroup { get; }
        Type UFGroup { get; }
        Type UFRequireInstanceMethod { get; }

        Type DiagramInfoAttribute { get; }

        Type UpdateProgressDelegate { get; }

        Type CommandWithSenderT { get; }
        Type CommandWith { get; }
        Type CommandWithSenderAndArgument { get; }
        Type YieldCommandWithSenderT { get; }
        Type YieldCommandWith { get; }
        Type YieldCommandWithSenderAndArgument { get; }
        Type YieldCommand { get; }
        Type Command { get; }
        Type ICommand { get; }
        Type ListOfViewModel { get; }

        Type ISerializerStream { get; }

        Type P { get; }
        Type ModelCollection { get; }
        Type Computed { get; }
    }
}