namespace Invert.uFrame.Editor.Refactoring
{
    public class RenameViewRefactorer : RenameRefactorer
    {
        public RenameFileRefactorer ViewFileRenamer { get; set; }
        public RenameIdentifierRefactorer View { get; set; }
        public RenameIdentifierRefactorer ViewBase { get; set; }
        public RenameViewRefactorer(ViewData data)
        {
            View = new RenameIdentifierRefactorer() { From = data.NameAsView };
            ViewBase = new RenameIdentifierRefactorer() { From = data.NameAsViewBase };
            ViewFileRenamer = new RenameFileRefactorer();
            var strategy = data.GetPathStrategy();
            ViewFileRenamer.RootPath = strategy.AssetPath;
            ViewFileRenamer.From = strategy.GetEditableViewFilename(data);

        }
        public override void Set(ISelectable data)
        {
            Set((ViewData)data);
        }
        public void Set(ViewData data)
        {
            var strategy = data.GetPathStrategy();
            View.To = data.NameAsView;
            ViewBase.To = data.NameAsViewBase;
            ViewFileRenamer.To =
                strategy.GetEditableViewFilename(data);
        }
        public override void PreProcess(RefactorContext refactorContext)
        {
            ViewFileRenamer.Process(refactorContext);
        }
        public override void Process(RefactorContext context)
        {
           
            View.Process(context);
            ViewBase.Process(context);
        }
        public override void PostProcess(RefactorContext context)
        {
            View.PostProcess(context);
            ViewBase.PostProcess(context);
        }
    }
}