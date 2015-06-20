using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace Invert.uFrame.VS
{
    static class ProjectUtilities
    {
        private static IServiceProvider _serviceProvider;

        public static void SetServiceProvider(IServiceProvider provider)
        {
            _serviceProvider = provider;
        }

        static public IList<IVsProject> GetProjectsOfCurrentSelections()
        {
            List<IVsProject> results = new List<IVsProject>();

            int hr = VSConstants.S_OK;
            var selectionMonitor = _serviceProvider.GetService(typeof(SVsShellMonitorSelection)) as IVsMonitorSelection;
            if (selectionMonitor == null)
            {
                Debug.Fail("Failed to get SVsShellMonitorSelection service.");
                return results;
            }

            IntPtr hierarchyPtr = IntPtr.Zero;
            uint itemID = 0;
            IVsMultiItemSelect multiSelect = null;
            IntPtr containerPtr = IntPtr.Zero;
            hr = selectionMonitor.GetCurrentSelection(out hierarchyPtr, out itemID, out multiSelect, out containerPtr);
            if (IntPtr.Zero != containerPtr)
            {
                Marshal.Release(containerPtr);
                containerPtr = IntPtr.Zero;
            }
            Debug.Assert(hr == VSConstants.S_OK, "GetCurrentSelection failed.");

            if (itemID == (uint)VSConstants.VSITEMID.Selection)
            {
                uint itemCount = 0;
                int fSingleHierarchy = 0;
                hr = multiSelect.GetSelectionInfo(out itemCount, out fSingleHierarchy);
                System.Diagnostics.Debug.Assert(hr == VSConstants.S_OK, "GetSelectionInfo failed.");

                VSITEMSELECTION[] items = new VSITEMSELECTION[itemCount];
                hr = multiSelect.GetSelectedItems(0, itemCount, items);
                System.Diagnostics.Debug.Assert(hr == VSConstants.S_OK, "GetSelectedItems failed.");

                foreach (VSITEMSELECTION item in items)
                {
                    IVsProject project = GetProjectOfItem(item.pHier, item.itemid);
                    if (!results.Contains(project))
                    {
                        results.Add(project);
                    }
                }
            }
            else
            {
                // case where no visible project is open (single file)
                if (hierarchyPtr != System.IntPtr.Zero)
                {
                    IVsHierarchy hierarchy = (IVsHierarchy)Marshal.GetUniqueObjectForIUnknown(hierarchyPtr);
                    results.Add(GetProjectOfItem(hierarchy, itemID));
                }
            }

            return results;
        }

        private static IVsProject GetProjectOfItem(IVsHierarchy hierarchy, uint itemID)
        {
            return (IVsProject)hierarchy;
        }

        static public string GetProjectFilePath(IVsProject project)
        {
            string path = string.Empty;
            int hr = project.GetMkDocument((uint)VSConstants.VSITEMID.Root, out path);
            Debug.Assert(hr == VSConstants.S_OK || hr == VSConstants.E_NOTIMPL, "GetMkDocument failed for project.");

            return path;
        }

        static public string GetUniqueProjectNameFromFile(string projectFile)
        {
            IVsProject project = GetProjectByFileName(projectFile);

            if (project != null)
            {
                return GetUniqueUIName(project);
            }

            return null;
        }

        static public string GetUniqueUIName(IVsProject project)
        {
            var solution = _serviceProvider.GetService(typeof(SVsSolution)) as IVsSolution3;
            if (solution == null)
            {
                Debug.Fail("Failed to get SVsSolution service.");
                return null;
            }

            string name = null;
            int hr = solution.GetUniqueUINameOfProject((IVsHierarchy)project, out name);
            Debug.Assert(hr == VSConstants.S_OK, "GetUniqueUINameOfProject failed.");
            return name;
        }

        static public IEnumerable<IVsProject> LoadedProjects
        {
            get
            {
                var solution = _serviceProvider.GetService(typeof(SVsSolution)) as IVsSolution;
                if (solution == null)
                {
                    
                    yield break;
                }

                IEnumHierarchies enumerator = null;
                Guid guid = Guid.Empty;
                solution.GetProjectEnum((uint)__VSENUMPROJFLAGS.EPF_LOADEDINSOLUTION, ref guid, out enumerator);
                
                IVsHierarchy[] hierarchy = new IVsHierarchy[1] { null };
                uint fetched = 0;
                for (enumerator.Reset(); enumerator.Next(1, hierarchy, out fetched) == VSConstants.S_OK && fetched == 1; /*nothing*/)
                {
                    yield return (IVsProject)hierarchy[0];
                }
            }
        }

        static public IVsProject GetProjectByFileName(string projectFile)
        {
            return LoadedProjects.FirstOrDefault(
                p => string.Compare(projectFile, GetProjectFilePath(p), StringComparison.OrdinalIgnoreCase) == 0);
        }

        static public string[] GetAllProjectFiles()
        {



            return LoadedProjects.SelectMany(x => GetProjectFiles(x,x as IVsHierarchy).Where(p=>p.EndsWith(".ufgraph"))).ToArray();

        }

        public static IEnumerable<string> GetProjectFiles(IVsProject project, IVsHierarchy projectHierarchy)
        {
            var enumerateFactory = _serviceProvider.GetService(typeof (SVsEnumHierarchyItemsFactory)) as IVsEnumHierarchyItemsFactory;

            IEnumHierarchyItems enumerator;
            enumerateFactory.EnumHierarchyItems(projectHierarchy, (uint)__VSEHI.VSEHI_Nest, (uint)VSConstants.VSITEMID.Root,
                out enumerator);
            uint fetched = 0;
            VSITEMSELECTION[] selection = new VSITEMSELECTION[1] { new VSITEMSELECTION() };
            for (enumerator.Reset(); enumerator.Next(1, selection, out fetched) == VSConstants.S_OK && fetched == 1; /*nothing*/)
            {
                string path = string.Empty;
                int hr = project.GetMkDocument(selection[0].itemid, out path);
                if (path != null)
                {
                    yield return path;
                }
                //project.GetMkDocument()
                //yield return (VSITEMSELECTION)selection[0];
            }
          
        }

        public static IEnumerable<string> FindInProjectFast(string endsWith, IVsProject4 vsp4, IVsHierarchy projectHierarchy)
        {
            uint celt = 0;
            uint[] rgItemIds = null;
            uint pcActual = 0;
            
            //
            // call this method twice, first time is to get the count, second time is to get the data.
            //
            vsp4.GetFilesEndingWith(endsWith, celt, rgItemIds, out pcActual);
            
            if (pcActual > 0)
            {
                // now we know the actual size of the array to allocate, so invoke again
                celt = pcActual;
                rgItemIds = new uint[celt];
                vsp4.GetFilesEndingWith(endsWith, celt, rgItemIds, out pcActual);
                Debug.Assert(celt == pcActual, "unexpected number of entries returned from GetFilesEndingWith()");

                for (var i = 0; i < celt; i++)
                {
                    object pvar;
                    // NOTE:  in cpp, this property is not the full path.  It is the full path in c# & vb projects.
                    var hr = projectHierarchy.GetProperty(rgItemIds[i], (int)__VSHPROPID.VSHPROPID_SaveName, out pvar);
                    var path = pvar as string;

                    if (path != null)
                    {
                        yield return path;
                    }
                }
            }
        }
        //static public bool IsMSBuildProject(IVsProject project)
        //{
        //    return ProjectCollection.GlobalProjectCollection.GetLoadedProjects(GetProjectFilePath(project)).Any();
        //}
    }
}