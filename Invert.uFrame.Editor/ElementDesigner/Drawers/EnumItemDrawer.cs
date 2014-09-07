using Invert.uFrame.Editor.ViewModels;

public class EnumItemDrawer : ItemDrawer
{
    public EnumItemDrawer(EnumItemViewModel viewModel)
    {
        DataContext = viewModel;
    }
    
    public override void Draw(float scale)
    {
        base.Draw(scale);
    }
}