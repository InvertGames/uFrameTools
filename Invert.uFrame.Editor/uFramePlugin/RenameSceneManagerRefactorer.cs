namespace Invert.uFrame.Editor.Refactoring
{
    public class RenameSceneManagerRefactorer : RenameRefactorer
    {
        public RenameFileRefactorer SceneManagerFileRenamer { get; set; }
        public RenameFileRefactorer SceneManagerSettingsFileRenamer { get; set; }
        public RenameIdentifierRefactorer SceneManager { get; set; }
        public RenameIdentifierRefactorer SceneManagerBase { get; set; }
        public RenameIdentifierRefactorer Settings { get; set; }
        public RenameIdentifierRefactorer SettingsField { get; set; }

        public RenameSceneManagerRefactorer(SceneManagerData data)
        {
            SceneManagerFileRenamer = new RenameFileRefactorer();
            SceneManagerSettingsFileRenamer = new RenameFileRefactorer();
            SceneManager = new RenameIdentifierRefactorer() {From = data.NameAsSceneManager};
            SceneManagerBase = new RenameIdentifierRefactorer() { From = data.NameAsSceneManagerBase };
            Settings = new RenameIdentifierRefactorer() { From = data.NameAsSettings };
            SettingsField = new RenameIdentifierRefactorer() { From = data.NameAsSettingsField };
            var strategy = data.GetPathStrategy();
            SceneManagerSettingsFileRenamer.RootPath = strategy.AssetPath;
            SceneManagerFileRenamer.RootPath = strategy.AssetPath;
            SceneManagerSettingsFileRenamer.From =
                strategy.GetEditableFilePath(data);
            SceneManagerFileRenamer.From =
                strategy.GetEditableFilePath(data);
           
        }
        public override void Set(ISelectable data)
        {
            Set((SceneManagerData)data);
        }
        public void Set(SceneManagerData data)
        {
            SceneManager.To = data.NameAsSceneManager;
            SceneManagerBase.To = data.NameAsSceneManagerBase;
            Settings.To = data.NameAsSettings;
            SettingsField.To = data.NameAsSettingsField;
            var strategy = data.GetPathStrategy();
            SceneManagerSettingsFileRenamer.To =
               strategy.GetEditableFilePath(data, "Settings");
            SceneManagerFileRenamer.To =
                strategy.GetEditableFilePath(data);
           
         
        }

        public override void PreProcess(RefactorContext context)
        {
            SceneManagerFileRenamer.Process(context);
            SceneManagerSettingsFileRenamer.Process(context);
        }

        public override void Process(RefactorContext context)
        {
            SceneManager.Process(context);
            SceneManagerBase.Process(context);
            Settings.Process(context);
            SettingsField.Process(context);
          
        }

        public override void PostProcess(RefactorContext context)
        {
            SceneManager.PostProcess(context);
            SceneManagerBase.PostProcess(context);
            Settings.PostProcess(context);
            SettingsField.PostProcess(context);
        }
    }
}