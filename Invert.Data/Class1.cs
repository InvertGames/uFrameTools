using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Invert.Json;

namespace Invert.Data
{
    public interface IRepository
    {
        void Add(IDataRecord obj);
        TObjectType Create<TObjectType>() where TObjectType : class,IDataRecord, new();
        TObjectType GetSingle<TObjectType>(string identifier) where TObjectType : class,IDataRecord, new();
        TObjectType GetById<TObjectType>(string identifier);
        IEnumerable<TObjectType> All<TObjectType>() where TObjectType : class,IDataRecord;
        void Commit();

        IEnumerable<TObjectType> AllOf<TObjectType>() where TObjectType : IDataRecord;
        IEnumerable AllOf(Type o);
        void RemoveAll<TObjectType>();
        void Remove(IDataRecord record);
        void Reset();
        void RemoveAll<TObjectType>(Predicate<TObjectType> expression) where TObjectType : class;
        void MarkDirty(IDataRecord graphData);

        string GetUniqueName(string s);
        void Signal<TEventType>(Action<TEventType> perform);
        void AddListener<TEventType>(TEventType instance);
    }

    public interface IDataRecord
    {
        IRepository Repository { get; set; }
        string Identifier { get; set; }
        bool Changed { get; set; }
    }

    public static class DataRecordPropertyChangedExtensions
    {
        public static void Changed(this IDataRecord record, string propertyName,object previousValue, object nextValue)
        {
            record.Changed = true;
            if (record.Repository != null && previousValue != nextValue)
            {
                record.Repository.Signal<IDataRecordPropertyChanged>(_ => _.PropertyChanged(record, propertyName, previousValue, nextValue));
            }
            
        }
    }
    public interface IDataRecordPropertyChanged
    {
        void PropertyChanged(IDataRecord record, string name,object previousValue, object nextValue);
    }
    public interface IDataRecordInserted
    {
        void RecordInserted(IDataRecord record);
    }

    public interface IDataRecordRemoved
    {
        void RecordRemoved(IDataRecord record);
    }
    
    public interface IDataRecordManager
    {
        void Initialize(IRepository repository);
        Type For { get; }
        IDataRecord GetSingle(string identifier);
        IEnumerable<IDataRecord> GetAll();
        void Add(IDataRecord o);
        void Commit();
        void Remove(IDataRecord item);
    }

    public interface ITypeRepositoryFactory
    {
        IDataRecordManager CreateRepository(IRepository typeDatabase, Type type);
        IEnumerable<IDataRecordManager> CreateAllManagers(IRepository repository);
    }

    public class TypeDatabase : IRepository
    {
        public Dictionary<Type, List<object>> Listeners
        {
            get { return _listeners ?? (_listeners = new Dictionary<Type, List<object>>()); }
            set { _listeners = value; }
        }

        public void AddListener<TEventType>(TEventType instance)
        {
            if (!Listeners.ContainsKey(typeof (TEventType)))
            {
                Listeners.Add(typeof(TEventType), new List<object>());
            }
            Listeners[typeof(TEventType)].Add(instance);
        }
        public void Signal<TEventType>(Action<TEventType> perform)
        {
            foreach (var item in Repositories)
            {
                if (typeof (TEventType).IsAssignableFrom(item.Key))
                {
                    foreach (var record in item.Value.GetAll().OfType<TEventType>())
                    {
                        var record1 = record;
                        perform(record1);
                    }
                }
            }
            if (Listeners.ContainsKey(typeof (TEventType)))
            {
                foreach (var listener in Listeners[typeof (TEventType)])
                {
                    var listener1 = listener;
                    perform((TEventType) listener1);
                }
            }
        }

        private Dictionary<Type, IDataRecordManager> _repositories;
        private Dictionary<Type, List<object>> _listeners;

        public Dictionary<Type, IDataRecordManager> Repositories
        {
            get { return _repositories ?? (_repositories = new Dictionary<Type, IDataRecordManager>()); }
            set { _repositories = value; }
        }

        public ITypeRepositoryFactory Factory { get; set; }

        public TypeDatabase(ITypeRepositoryFactory factory)
        {
            Factory = factory;
            Repositories = factory.CreateAllManagers(this)
                .ToDictionary(k => k.For, v => v);
        }

        public void Add(IDataRecord obj)
        {
            var repo = GetRepositoryFor(obj.GetType());
            repo.Add(obj);
            MarkDirty(obj);
        }

        public TObjectType Create<TObjectType>() where TObjectType : class, IDataRecord, new()
        {
            var obj = new TObjectType();
            var repo = GetRepositoryFor(typeof (TObjectType));
            repo.Add(obj);
            return obj;
        }

        public TObjectType GetSingle<TObjectType>(string identifier) where TObjectType : class, IDataRecord, new()
        {
            if (string.IsNullOrEmpty(identifier)) return default(TObjectType);
            var repo = GetRepositoryFor(typeof(TObjectType));
            return repo.GetSingle(identifier) as TObjectType;
        }

        public TObjectType GetById<TObjectType>(string identifier)
        {
            if (string.IsNullOrEmpty(identifier)) return default(TObjectType);
            foreach (var item in Repositories)
            {
                if (typeof (TObjectType).IsAssignableFrom(item.Key))
                {
                    var result= (TObjectType)item.Value.GetSingle(identifier);
                    if (result != null)
                        return result;
                }
            }
            return default(TObjectType);
        }

        public IDataRecordManager GetRepositoryFor(Type type)
        {
            IDataRecordManager repo;
            if (!Repositories.TryGetValue(type, out repo))
            {
                repo = Factory.CreateRepository(this, type);
                Repositories.Add(type, repo);
            }
            return repo;
        }

        public IEnumerable<TObjectType> All<TObjectType>() where TObjectType : class, IDataRecord
        {
            var repo = GetRepositoryFor(typeof(TObjectType));
            return repo.GetAll().Cast<TObjectType>();
        }

        public IEnumerable<TObjectType> AllOf<TObjectType>() where TObjectType : IDataRecord
        {
            foreach (var repo in Repositories)
            {
                if (typeof(TObjectType).IsAssignableFrom(repo.Key))
                {
                    foreach (var item in repo.Value.GetAll())
                    {
                        yield return (TObjectType)item;
                    }
                }
            }
        }

        public IEnumerable AllOf(Type o)
        {

            if (!typeof (IDataRecord).IsAssignableFrom(o)) yield break;

            foreach (var repo in Repositories)
            {
                if (o.IsAssignableFrom(repo.Key))
                {
                    foreach (var item in repo.Value.GetAll())
                    {
                        yield return item;
                    }
                }
            }

        }

        public void Commit()
        {
            foreach (var item in Repositories)
            {
                item.Value.Commit();
            }
        }

        public void RemoveAll<TObjectType>()
        {
            var repo = GetRepositoryFor(typeof(TObjectType));
            foreach (var item in repo.GetAll())
            {
                repo.Remove(item);
            }
        }
        public void RemoveAll<TObjectType>(Predicate<TObjectType> expression) where TObjectType : class
        {
            var repo = GetRepositoryFor(typeof(TObjectType));
            foreach (var item in repo.GetAll().Where(x=>expression(x as TObjectType)))
            {
                repo.Remove(item);
            }
        }

        public void MarkDirty(IDataRecord graphData)
        {
            
        }

        public string GetUniqueName(string s)
        {
            // TODO 2.0 ??? Unique Names
            return s;

        }

        public void Remove(IDataRecord record)
        {
            var repo = GetRepositoryFor(record.GetType());
            repo.Remove(record);
        }

        public void Reset()
        {
            Repositories.Clear();
        }
    }

    public class JsonRepositoryFactory : ITypeRepositoryFactory
    {
        private DirectoryInfo _directory;
        public string RootPath { get; set; }

        public DirectoryInfo Directory
        {
            get { return _directory ?? (_directory = new DirectoryInfo(RootPath)); }
            set { _directory = value; }
        }
        public JsonRepositoryFactory(string rootPath)
        {
            RootPath = rootPath;
            if (!Directory.Exists)
            {
                Directory.Create();
            }
        }

        public IEnumerable<IDataRecordManager> CreateAllManagers(IRepository repository)
        {
            foreach (var item in Directory.GetDirectories())
            {
                var fullname = item.Name;
                Type type = null;
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
             
                    type = assembly.GetType(fullname);
                    if (type != null)
                    {
                     
                        break;
                    }
                }
                if (type == null)
                {
                    throw new Exception(string.Format("Database didn't load entirely, {0} type not found",fullname));
                    continue;
                }
                yield return CreateRepository(repository,type);
            }
        }

        public IDataRecordManager CreateRepository(IRepository repository, Type type)
        {
            return new JsonFileRecordManager(repository, RootPath, type);
        }
    }

    public class JsonFileRecordManager : IDataRecordManager
    {
        public IRepository Repository { get; set; }
        private Dictionary<string, IDataRecord> _cached;
        private DirectoryInfo _directoryInfo;
        private HashSet<string> _removed;

        public string RootPath { get; set; }

        public void Initialize(IRepository repository)
        {
            Repository = repository;
          
        }

        public Type For { get; set; }

        public string RecordsPath
        {
            get { return Path.Combine(RootPath, For.FullName); }
        }

        public JsonFileRecordManager(IRepository repository, string rootPath, Type @for)
        {
            RootPath = rootPath;
            For = @for;
            Repository = repository;
            Initialize(repository);
        }

        public DirectoryInfo DirectoryInfo
        {
            get { return _directoryInfo ?? (_directoryInfo = new DirectoryInfo(RecordsPath)); }
            set { _directoryInfo = value; }
        }

        private bool _loadedCached;
        private void LoadRecordsIntoCache()
        {
            if (_loadedCached) return;

            if (!DirectoryInfo.Exists)
            {
                DirectoryInfo.Create();
            }
            foreach (var file in DirectoryInfo.GetFiles())
            {
                LoadRecord(file);
            }
            _loadedCached = true;
        }

        private void LoadRecord(FileInfo file)
        {
            if (Cached.ContainsKey(Path.GetFileNameWithoutExtension(file.Name))) return;
            var record = InvertJsonExtensions.DeserializeObject(For, JSON.Parse(File.ReadAllText(file.FullName))) as IDataRecord;
            if (record != null)
            {
                record.Repository = this.Repository;
                
                Cached.Add(record.Identifier, record);
                record.Changed = false;
            }
        }

        public IDataRecord GetSingle(string identifier)
        {
 
                LoadRecordsIntoCache();
    
            if (!Cached.ContainsKey(identifier))
            {
   
                return null;
            }
            return Cached[identifier];
        }

        public IEnumerable<IDataRecord> GetAll()
        {
    
                LoadRecordsIntoCache();
        
            return Cached.Values.Where(p=>!Removed.Contains(p.Identifier));
        }

        public void Add(IDataRecord o)
        {
            o.Changed = true;
            if (string.IsNullOrEmpty(o.Identifier))
            {
                o.Identifier = Guid.NewGuid().ToString();
            }
            o.Repository = this.Repository;
            if (!Cached.ContainsKey(o.Identifier))
            {
                Cached.Add(o.Identifier, o);
                Repository.Signal<IDataRecordInserted>(_=>_.RecordInserted(o));
            }
        }

        public void Commit()
        {
            if (!DirectoryInfo.Exists)
            {
                DirectoryInfo.Create();
            }
            foreach (var item in Cached)
            {
                var filename = Path.Combine(RecordsPath, item.Key + ".json");
                if (Removed.Contains(item.Key))
                {
                    File.Delete(filename);
                }
                else
                {
                    if (item.Value.Changed)
                    {
                        var json = InvertJsonExtensions.SerializeObject(item.Value);
                        File.WriteAllText(filename, json.ToString(true));
                    }
                    item.Value.Changed = false;
                }
            }
        }

        public void Remove(IDataRecord item)
        {
            Removed.Add(item.Identifier);
            Repository.Signal<IDataRecordRemoved>(_ => _.RecordRemoved(item));
        }

        public HashSet<string> Removed
        {
            get { return _removed ?? (_removed = new HashSet<string>()); }
            set { _removed = value; }
        }

        public Dictionary<string, IDataRecord> Cached
        {
            get { return _cached ?? (_cached = new Dictionary<string, IDataRecord>(StringComparer.OrdinalIgnoreCase)); }
            set { _cached = value; }
        }
    }
}
