using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace Invert.uFrame.VS
{
    public static class IVsHierarchyExtensions
    {
        public static T GetProperty<T>(this IVsHierarchy h, __VSHPROPID property, uint itemId = 0)
        {
            object name;
            
            h.GetProperty(itemId == 0 ? VSConstants.VSITEMID_ROOT : itemId, (int)property, out name);
            return (T)name;
        }
        public static string GetProjectNamespace(this IVsHierarchy h)
        {
            object name;
            h.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_DefaultNamespace, out name);
            return (string)name;
        }
        public static string GetProjectDirectory(this IVsHierarchy h)
        {
            object name;
            h.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ProjectDir, out name);
            return (string)name;
        }
        public static string GetProjectGuid(this IVsHierarchy h)
        {
            object name;
            h.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ProjectIDGuid, out name);
            return (string)name;
        }
        public static string GetProjectName(this IVsHierarchy h)
        {
            object name;
            h.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ProjectName, out name);
            return (string)name;
        }
        public static string GetName(this IVsHierarchy h)
        {
            object name;
            h.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_Name, out name);
            return (string)name;
        }
        public static string GetFilename(this IVsHierarchy h)
        {
            object name;
            h.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_SaveName, out name);
            return (string)name;
        }
    }
}