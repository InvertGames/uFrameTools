using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Invert.Core.GraphDesigner.Unity.KoinoniaSystem.Classes
{

    public interface IUFramePackage
    {

        string Id { get; set; }
        string VersionTag { get; set; }

        void Install();
        void Update();
        void Uninstall();

        bool NeedsInstall();
        bool NeedsUpdate();
    }

    public abstract class UFramePackage : IUFramePackage
    {
        private string _id;
        private string _versionTag;

        public string Id
        {
            get
            {
                if (_id == string.Empty)
                {

                    _id = GetGuidFromAttribute();
                }
                return _id;
            }
            set { _id = value; }
        }

        public string VersionTag
        {
            get { return _versionTag ?? (_versionTag = GetVersionTagFromAttribute()); }
            set { _versionTag = value; }
        }

        private string GetGuidFromAttribute()
        {
            var attribute = this.GetType().GetCustomAttributes(typeof(ProjectLinkAttribute), true).FirstOrDefault() as ProjectLinkAttribute;
            if (attribute != null)
            {
                return attribute.PackageId;
            }
            return string.Empty;
        }

        private string GetVersionTagFromAttribute()
        {
            var attribute = this.GetType().GetCustomAttributes(typeof(ProjectLinkAttribute), true).FirstOrDefault() as ProjectLinkAttribute;
            if (attribute != null)
            {
                return attribute.Version;
            }
            return null;
        }

        public virtual void Install()
        {

        }

        public virtual void Update()
        {

        }

        public virtual void Uninstall()
        {

        }

        public virtual bool NeedsInstall()
        {
            return false;
        }

        public virtual bool NeedsUpdate()
        {
            return false;
        }
    }


    public class ProjectLinkAttribute : Attribute
    {
        public string PackageId { get; set; }
        public string Version { get; set; }

        public ProjectLinkAttribute(string packageId, string version)
        {
            PackageId = packageId;
            Version = version;
        }
    }

}
