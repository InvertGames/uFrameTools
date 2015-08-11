using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Invert.Core.GraphDesigner.Unity.KoinoniaSystem.Data
{
    public enum UFramePackageManagementType
    {
        Manual,
        Github
    }

    public enum PackageState
    {
        NotInstalled,
        Copying,
        Installing,
        Installed,
        Updating,
        Uninstalling,
     
    }
}
