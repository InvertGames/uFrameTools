using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Invert.Data;
using Invert.IOC;

namespace Invert.Core.GraphDesigner
{
    public interface IChangeDatabase
    {
        void ChangeDatabase(IGraphConfiguration configuration);
    }

    public interface ICreateDatabase
    {
        IGraphConfiguration CreateDatabase( string name, string codePath);
    }

    public class ChangeDatabaseCommand : Command
    {
       
    }

    public class SaveCommand : Command
    {
        
    }

    public class DatabaseService : DiagramPlugin,  
        IDataRecordInserted, 
        IDataRecordRemoved, 
        IDataRecordPropertyBeforeChange,
        IDataRecordPropertyChanged,
        IChangeDatabase,
        IToolbarQuery,
        IContextMenuQuery,
        IExecuteCommand<ChangeDatabaseCommand>,
        IExecuteCommand<SaveCommand>
    {

        public IGraphConfiguration CurrentConfiguration { get; set; }

        // Important make sure we intialize very late so that other plugins can register graph configurations
        public override decimal LoadPriority
        {
            get { return 50000; }
        }

        public override bool Required
        {
            get { return true; }
        }

        public override void Initialize(UFrameContainer container)
        {
            base.Initialize(container);
 
            // Get all the configurations
            Configurations = container.Instances
                .Where(p => p.Base == typeof(IGraphConfiguration))
                .ToDictionary(p => p.Name, v => v.Instance as IGraphConfiguration);

            CurrentConfiguration = Configurations.Values.FirstOrDefault(p => p.IsCurrent) ??
                                   Configurations.Values.FirstOrDefault();

            if (CurrentConfiguration == null)
            {
                InvertApplication.Log("Database not found, creating one now.");
                var createDatabase = InvertApplication.Container.Resolve<ICreateDatabase>();
                if (createDatabase != null)
                {
                    CurrentConfiguration = createDatabase.CreateDatabase("uFrameDB", "Code");
                }
                else
                {
                    InvertApplication.Log("A plugin the creates a default database is not available.  Please implement ICreateDatabase to provide this feature.");
                }
            }
            if (CurrentConfiguration != null)
            {
                container.RegisterInstance<IGraphConfiguration>(CurrentConfiguration);
                var typeDatabase = new TypeDatabase(new JsonRepositoryFactory(CurrentConfiguration.FullPath));
                container.RegisterInstance<IRepository>(typeDatabase);
                
                //var typeDatabase = container.Resolve<IRepository>();
                typeDatabase.AddListener<IDataRecordInserted>(this);
                typeDatabase.AddListener<IDataRecordRemoved>(this);
                typeDatabase.AddListener<IDataRecordPropertyChanged>(this);
                typeDatabase.AddListener<IDataRecordPropertyBeforeChange>(this);
            }
            else
            {
                InvertApplication.Log("A uFrameDatabase doesn't exist.");
            }
           
        }

        public override void Loaded(UFrameContainer container)
        {
            base.Loaded(container);
           
        }

        public Dictionary<string, IGraphConfiguration> Configurations { get; set; }

        public void RecordInserted(IDataRecord record)
        {
            InvertApplication.SignalEvent<IDataRecordInserted>(_ =>
            {
                if (_ != this) _.RecordInserted(record);
            });
        }

        public void RecordRemoved(IDataRecord record)
        {
            InvertApplication.SignalEvent<IDataRecordRemoved>(_ =>
            {
                if (_ != this) _.RecordRemoved(record);
            }); 
        }

        public void PropertyChanged(IDataRecord record, string name, object previousValue, object nextValue)
        {
            InvertApplication.SignalEvent<IDataRecordPropertyChanged>(_ =>
            {
                if (_ != this) _.PropertyChanged(record, name, previousValue, nextValue);
            });
        }
        public void BeforePropertyChanged(IDataRecord record, string name, object previousValue, object nextValue)
        {
            InvertApplication.SignalEvent<IDataRecordPropertyBeforeChange>(_ =>
            {
                if (_ != this) _.BeforePropertyChanged(record, name, previousValue, nextValue);
            });
        }
        public void ChangeDatabase(IGraphConfiguration configuration)
        {
            foreach (var graphConfig in InvertGraphEditor.Container.ResolveAll<IGraphConfiguration>())
            {
                graphConfig.IsCurrent = graphConfig == configuration;
            }
            InvertApplication.Container = null;
        }

        public void QueryToolbarCommands(ToolbarUI ui)
        {
            ui.AddCommand(new ToolbarItem()
            {
                Title = CurrentConfiguration == null ? "Change Database" : CurrentConfiguration.Title,
                Command = new ChangeDatabaseCommand(),
                Order = -1
            });
            ui.AddCommand(new ToolbarItem()
            {
                Title = "Save",
                Command = new SaveCommand(),
                Position = ToolbarPosition.Right
            });
        }

        public void QueryContextMenu(ContextMenuUI ui, MouseEvent evt, object obj)
        {
            
        }

        public void Execute(ChangeDatabaseCommand command)
        {
            var databaseService = InvertApplication.Container.Resolve<DatabaseService>();
            InvertGraphEditor.WindowManager.InitItemWindow(databaseService.Configurations.Values, _ =>
            {
                Signal<IChangeDatabase>(cd => cd.ChangeDatabase(_));
            });
        }

        public void Execute(SaveCommand command)
        {
            Container.Resolve<IRepository>().Commit();
        }
    }
}