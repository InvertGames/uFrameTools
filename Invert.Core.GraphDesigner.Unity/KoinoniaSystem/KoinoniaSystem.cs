using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using Invert.Core.GraphDesigner.Unity.KoinoniaSystem.Commands;
using Invert.Core.GraphDesigner.Unity.KoinoniaSystem.Data;
using Invert.Core.GraphDesigner.Unity.KoinoniaSystem.Drawers;
using Invert.Core.GraphDesigner.Unity.KoinoniaSystem.Events;
using Invert.Core.GraphDesigner.Unity.KoinoniaSystem.ViewModels;
using Invert.Core.GraphDesigner.Unity.WindowsPlugin;
using Invert.IOC;
using Invert.Json;
using UnityEditor;
using UnityEngine;
using MessageType = Invert.Core.GraphDesigner.Unity.WindowsPlugin.MessageType;

namespace Invert.Core.GraphDesigner.Unity.KoinoniaSystem
{

    public class KoinoniaSystem : DiagramPlugin, IExecuteCommand<LoginCommand>, ILoggedInEvent
    {
        private List<UFramePackagePreviewDescriptor> _previews;
        private List<UFramePackageDescriptor> _packages;
        private Dictionary<string, List<UFramePackageRevisionDescriptor>> _revisions;

        public string AccessToken { get; set; }

        public string Username { get; set; }

        public AuthorizationState AuthorizationState { get; set; }

        public string GlobalProgressMessage { get; set; }

        public List<UFramePackagePreviewDescriptor> Previews
        {
            get { return _previews ?? (_previews = new List<UFramePackagePreviewDescriptor>(GetPreviews())); }
            set { _previews = value; }
        }
       
        public List<UFramePackageDescriptor> Packages
        {
            get { return _packages ?? (_packages = new List<UFramePackageDescriptor>(GetPackages())); }
            set { _packages = value; }
        }

        public Dictionary<string, List<UFramePackageRevisionDescriptor>> Revisions
        {
            get { return _revisions ?? (_revisions = new Dictionary<string, List<UFramePackageRevisionDescriptor>>()); }
            set { _revisions = value; }
        }


        public List<UFramePackageRevisionDescriptor> GetPackageRevisions(string id)
        {

            if (!Revisions.ContainsKey(id))
            {
                Revisions.Add(id, new List<UFramePackageRevisionDescriptor>());
                //Do some async shit here to get the project
                var project = Packages.FirstOrDefault(p => p.Id == id);
                if (project != null)
                {
                    var revs = GetAllRevisions().Where(r => project.RevisionIds.Contains(r.Id)).ToArray();
                    foreach (var revisionId in project.RevisionIds)
                    {
                        Debug.Log("Will search for revision "+revisionId);
                    }
                    
                    Debug.Log(string.Format("Founds {0} revisions for {1}", revs.Length, project.Title));
                    Revisions[id].AddRange(revs);
                }
                else
                {
                    Debug.Log("PROJECT IS NULL OH MY GOD NO ALL SYSTEMS DOWN WE ARE DOOMED");
                }
            }

            return Revisions[id];
        } 

        private List<UFramePackageDescriptor> GetPackages()
        {
            var text = Resources.Load<TextAsset>("Package");
            var packs = InvertJsonExtensions.DeserializeObject<List<UFramePackageDescriptor>>(text.text);
            return packs;
        }

        private List<UFramePackageRevisionDescriptor> GetAllRevisions()
        {
            var text = Resources.Load<TextAsset>("Revisions");
            var packs = InvertJsonExtensions.DeserializeObject<List<UFramePackageRevisionDescriptor>>(text.text);
            return packs;
        } 

        private List<UFramePackagePreviewDescriptor> GetPreviews()
        {
            var text = Resources.Load<TextAsset>("PackagePreview");
            var previes = InvertJsonExtensions.DeserializeObject<List<UFramePackagePreviewDescriptor>>(text.text);
            return previes;
        }

        public override void Initialize(UFrameContainer container)
        {
            base.Initialize(container);
            AuthorizationState = AuthorizationState.Unauthorized;
        }

        public void Execute(LoginCommand command)
        {
            AuthorizationState = AuthorizationState.InProgress;
            GlobalProgressMessage = "Logging in...";
            Thread.Sleep(2000);
            InvertApplication.SignalEvent<ILoggedInEvent>(_=>_.LoggedIn());
        }

        public void LoggedIn()
        {
            AuthorizationState = AuthorizationState.LoggedIn;
            GlobalProgressMessage = null;
            InvertApplication.SignalEvent<ILogEvents>(_=>_.Log("Koinonia Logged In",MessageType.Info));
        }


    }



}

