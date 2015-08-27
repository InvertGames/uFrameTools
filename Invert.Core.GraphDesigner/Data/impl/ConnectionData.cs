using System;
using System.Collections.Generic;
using System.Linq;
using Invert.Core.GraphDesigner;
using Invert.Data;
using Invert.Json;

namespace Invert.Core.GraphDesigner
{
    public interface IConnectableProvider : IDataRecord
    {
        IEnumerable<IConnectable> Connectables { get; }
    }
    public class ConnectionData : IJsonObject, IDataRecord, IDataRecordRemoved
    {

        private string _outputIdentifier;

        private string _inputIdentifier;

        public ConnectionData(string outputIdentifier, string inputIdentifier)
        {
            OutputIdentifier = outputIdentifier;
            InputIdentifier = inputIdentifier;
        }

        public ConnectionData()
        {
        }

        [JsonProperty]
        public string OutputIdentifier
        {
            get { return _outputIdentifier; }
            set
            {
                this.Changed("OutputIdentifier",ref  _outputIdentifier, value);
            }
        }

        [JsonProperty]
        public string InputIdentifier
        {
            get { return _inputIdentifier; }
            set
            {
                
                this.Changed("InputIdentifier",ref _inputIdentifier, value);
            }
        }

        public IGraphData Graph { get; set; }

        public IConnectable Output
        {
            get
            {
                foreach (var item in Repository.AllOf<IConnectableProvider>())
                {
                   
                    foreach (var child in item.Connectables)
                    {
                        if (child.Identifier == OutputIdentifier)
                        {
                            return child;
                        }
                    }
                }
                return Repository.GetById<IConnectable>(OutputIdentifier);
            }
        }

        public IConnectable Input
        {
            get
            {
                foreach (var item in Repository.AllOf<IConnectableProvider>())
                {
                    
                    foreach (var child in item.Connectables)
                    {
                        if (child.Identifier == InputIdentifier)
                        {
                            return child;
                        }
                    }
                }
                return Repository.GetById<IConnectable>(InputIdentifier);
            }
        }

        public void Remove()
        {
            Graph.RemoveConnection(Output, Input);
        }

        public void Serialize(JSONClass cls)
        {
            cls.Add("OutputIdentifier", OutputIdentifier ?? string.Empty);
            cls.Add("InputIdentifier", InputIdentifier ?? string.Empty);
        }

        public void Deserialize(JSONClass cls)
        {
            if (cls["InputIdentifier"] != null)
            {
                InputIdentifier = cls["InputIdentifier"].Value;
            }
            if (cls["OutputIdentifier"] != null)
            {
                OutputIdentifier = cls["OutputIdentifier"].Value;
            }
        }

        public IRepository Repository { get; set; }
        public string Identifier { get; set; }

        public bool Changed { get; set; }

        public void RecordRemoved(IDataRecord record)
        {
            if (record.Identifier == OutputIdentifier || record.Identifier == InputIdentifier)
                Repository.Remove(this);
        }
    }    
}


[Obsolete]
public interface IConnectionDataProvider
{
    IEnumerable<ConnectionData> Connections { get; }
}