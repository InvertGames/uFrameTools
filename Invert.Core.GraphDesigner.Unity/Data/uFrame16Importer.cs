using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Invert.IOC;
using Invert.Json;
using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using Invert.Data;

namespace Invert.Core.GraphDesigner.Unity
{
    public class uFrame16Importer : DiagramPlugin
    {

        public override void Initialize(UFrameContainer container)
        {
            base.Initialize(container);
            GraphDatas = GetAssetsOfType<UnityGraphData>(".asset");
        }

        public static UnityGraphData[] GraphDatas { get; set; }

        /// <summary>
        /// Used to get assets of a certain type and file extension from entire project
        /// </summary>
        /// <param name="fileExtension">The file extention the type uses eg ".prefab".</param>
        /// <returns>An Object array of assets.</returns>
        public static T[] GetAssetsOfType<T>(string fileExtension) where T : UnityEngine.Object
        {
            List<T> tempObjects = new List<T>();
            DirectoryInfo directory = new DirectoryInfo(Application.dataPath);
            FileInfo[] goFileInfo = directory.GetFiles("*" + fileExtension, SearchOption.AllDirectories);

            int i = 0; int goFileInfoLength = goFileInfo.Length;
            FileInfo tempGoFileInfo; string tempFilePath;
            T tempGO;
            for (; i < goFileInfoLength; i++)
            {
                tempGoFileInfo = goFileInfo[i];
                if (tempGoFileInfo == null)
                    continue;

                tempFilePath = tempGoFileInfo.FullName;
                tempFilePath = tempFilePath.Replace(@"\", "/").Replace(Application.dataPath, "Assets");
                tempGO = AssetDatabase.LoadAssetAtPath(tempFilePath, typeof(T)) as T;
                if (tempGO == null)
                {
                    continue;
                }
                else if (!(tempGO is T))
                {
                    continue;
                }

                tempObjects.Add(tempGO);
            }

            return tempObjects.ToArray();
        }


    }
}