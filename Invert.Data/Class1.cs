using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Invert.Json;

namespace Invert.Data
{
    public interface IRepository
    {
        void Save(IDataRecord obj);
        TObjectType Create<TObjectType>() where TObjectType : class,IDataRecord, new();
        TObjectType GetSingle<TObjectType>(string identifier) where TObjectType : class,IDataRecord, new();
        IEnumerable<TObjectType> GetAll<TObjectType>() where TObjectType : class,IDataRecord, new();
        void Commit();
    }

    public interface IDataRecord
    {
        IRepository Repository { get; set; }
        string Id { get; set; }
    }

    public interface IDataRecordManager
    {
        void Initialize(IRepository repository);
        Type For { get; }
        IDataRecord GetSingle(string identifier);
        IEnumerable<IDataRecord> GetAll();
        void Save(IDataRecord o);
        void Commit();
    }

    public interface ITypeRepositoryFactory
    {
        IDataRecordManager CreateRepository(IRepository typeDatabase, Type type);
    }

    public class TypeDatabase : IRepository
    {
        private Dictionary<Type, IDataRecordManager> _repositories;

        public Dictionary<Type, IDataRecordManager> Repositories
        {
            get { return _repositories ?? (_repositories = new Dictionary<Type, IDataRecordManager>()); }
            set { _repositories = value; }
        }

        public ITypeRepositoryFactory Factory { get; set; }

        public TypeDatabase(ITypeRepositoryFactory factory)
        {
            Factory = factory;
        }

        public void Save(IDataRecord obj)
        {
            var repo = GetRepositoryFor(obj.GetType());
            repo.Save(obj);
        }

        public TObjectType Create<TObjectType>() where TObjectType : class, IDataRecord, new()
        {
            var obj= new TObjectType()
            {
                Repository = this,
                Id = Guid.NewGuid().ToString()
            };
            var repo = GetRepositoryFor(typeof (TObjectType));
            repo.Save(obj);
            return obj;
        }

        public TObjectType GetSingle<TObjectType>(string identifier) where TObjectType : class, IDataRecord, new()
        {
            var repo = GetRepositoryFor(typeof(TObjectType));
            return repo.GetSingle(identifier) as TObjectType;
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

        public IEnumerable<TObjectType> GetAll<TObjectType>() where TObjectType : class, IDataRecord, new()
        {
            var repo = GetRepositoryFor(typeof(TObjectType));
            return repo.GetAll().Cast<TObjectType>();
        }

        public IEnumerable<TObjectType> GetAny<TObjectType>() where TObjectType : class, IDataRecord, new()
        {
            foreach (var repo in Repositories)
            {
                if (typeof(TObjectType).IsAssignableFrom(repo.Key))
                {
                    foreach (var item in repo.Value.GetAll())
                    {
                        yield return item as TObjectType;
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
    }

    public class JsonRepositoryFactory : ITypeRepositoryFactory
    {
        public string RootPath { get; set; }

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

        public string RootPath { get; set; }

        public void Initialize(IRepository repository)
        {
            Repository = repository;
            LoadRecordsIntoCache();
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

        private void LoadRecordsIntoCache()
        {
            if (!DirectoryInfo.Exists)
            {
                DirectoryInfo.Create();
            }
            foreach (var file in DirectoryInfo.GetFiles())
            {
                LoadRecord(file);
            }
        }

        private void LoadRecord(FileInfo file)
        {
            var record = JsonExtensions.DeserializeObject(For, JSON.Parse(File.ReadAllText(file.FullName))) as IDataRecord;
            if (record != null)
            {
                record.Repository = this.Repository;
                Cached.Add(record.Id, record);
            }
        }

        public IDataRecord GetSingle(string identifier)
        {
            return Cached[identifier];
        }

        public IEnumerable<IDataRecord> GetAll()
        {
            return Cached.Values;
        }

        public void Save(IDataRecord o)
        {
            o.Repository = this.Repository;
            if (!Cached.ContainsKey(o.Id))
            {
                Cached.Add(o.Id, o);
            }
        }

        public void Commit()
        {
            foreach (var item in Cached)
            {
                var json = JsonExtensions.SerializeObject(item.Value);
                File.WriteAllText(Path.Combine(RecordsPath, item.Key + ".json"), json.ToString());
            }
        }
        public Dictionary<string, IDataRecord> Cached
        {
            get { return _cached ?? (_cached = new Dictionary<string, IDataRecord>()); }
            set { _cached = value; }
        }
    }
}
