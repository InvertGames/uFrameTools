using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Invert.Core.GraphDesigner.Unity.KoinoniaSystem.Data
{
    public class UFramePackageRevisionDescriptor
    {
        public Guid Id { get; set; }
        public string SnapshotUri { get; set; }
        public string VersionTag { get; set; }
        public IList<UFramePackageRevisionDescriptor> DependentRevisions { get; set; }
        public IList<UFramePackageRevisionDescriptor> Dependencies { get; set; }
        public bool IsPublic { get; set; }
    }
}
