using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Invert.Json;

namespace Invert.Core.GraphDesigner.Unity.KoinoniaSystem.Data
{
    public class UFramePackageRevisionDescriptor
    {
        [JsonProperty]
        public string Id { get; set; }
        [JsonProperty]
        public string SnapshotUri { get; set; }
        [JsonProperty]
        public string VersionTag { get; set; }
        //public IList<UFramePackageRevisionDescriptor> DependentRevisions { get; set; }
        //public IList<UFramePackageRevisionDescriptor> Dependencies { get; set; }
        [JsonProperty]
        public bool IsPublic { get; set; }
    }
}
