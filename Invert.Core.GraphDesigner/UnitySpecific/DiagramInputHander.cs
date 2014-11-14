using System.Linq;
using Invert.uFrame.Editor;
using Invert.uFrame.Editor.ViewModels;

public class DiagramInputHander : IInputHandler
{
    public GraphItemViewModel ViewModelAtMouse { get; set; }
    public DiagramViewModel ViewModel { get; set; }

    public DiagramInputHander(DiagramViewModel viewModel)
    {
        ViewModel = viewModel;
    }

    public virtual void OnMouseDoubleClick(MouseEvent e)
    {

    }

    public virtual void OnMouseDown(MouseEvent e)
    {

    }

    public virtual void OnMouseMove(MouseEvent e)
    {
        ViewModelAtMouse = ViewModel.GraphItems.Reverse().FirstOrDefault(p => p.Bounds.Contains(e.MousePosition));


    }

    public virtual void OnMouseUp(MouseEvent e)
    {

    }

    public void OnRightClick(MouseEvent mouseEvent)
    {

    }
}