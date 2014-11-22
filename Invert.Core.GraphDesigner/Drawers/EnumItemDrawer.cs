using Invert.uFrame.Editor.ViewModels;

namespace Invert.Core.GraphDesigner
{
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
}