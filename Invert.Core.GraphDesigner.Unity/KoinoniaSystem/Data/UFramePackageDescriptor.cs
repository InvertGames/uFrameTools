using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Invert.Data;

namespace Invert.Core.GraphDesigner.Unity.KoinoniaSystem.Data
{
    public class UFramePackageDescriptor : IDataRecord 
    {

        public string Id { get; set; }
        
        public string Title { get; set; }
        
        public string Description { get; set; }

        public UFramePackageManagementType ManagementType { get; set; }

        public string ProjectIconUrl { get; set; }


//        public string Code; // done using based on Title            
//        
//        public string Slug { get; set; }
        
        public IList<UFramePackageRevisionDescriptor> Revisions { get; set; }

        public string RepositoryUrl { get; set; }
        
        public bool IsPublic { get; set; }
        
        public PackageState State { get; set; }

        public override bool Equals(object obj)
        {
            var package = obj as UFramePackageDescriptor;
            if (package != null) return package.Id == Id;
            return false;
        }

        public IRepository Repository { get; set; }

        public string Identifier
        {
            get { return Id; }
            set { }
        }

        public bool Changed { get; set; }
    }
}
