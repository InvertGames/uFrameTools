namespace Invert.Core.GraphDesigner
{
    public interface IToolbarCommand : IEditorCommand
    {
        ToolbarPosition Position { get; }
    }

    public interface IDropDownCommand
    {
        
    }
}