using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using HSE.Editor.Attributes;
using Unity.Entities.Build;
using Unity.Entities.Content;
using Unity.Scenes.Editor;
using UnityEditor;
using UnityEngine;

namespace HSE.Editor.Mods
{
    [CreateAssetMenu(fileName = "Modpack", menuName = "HSEMods/Modpack", order = 0)]
    public class ModPackSO : ScriptableObject
    {
        public string Guid => guid;
        [ReadOnly] [SerializeField] [Delayed] private string guid;
        [ReadOnly] [SerializeField] [Delayed] private string buildPath;
        [ReadOnly] [SerializeField] [Delayed] private string streamingAssetsPath;
#if UNITY_EDITOR
        
        [SerializeField] private List<SceneAsset> scenes;
        public List<SceneAsset> Scenes => scenes;

        [Button(nameof(SetBuildPath))]
        public bool setBuildPath;
        [Button(nameof(BuildSubSceneData))]
        public bool buildSubSceneData;
        [Button(nameof(PublishStreamingAssets))]
        public bool publishStreamingAssets;
        
        public void SetBuildPath()
        {
            var buildFolder = EditorUtility.OpenFolderPanel("Select Build To Publish",
                Path.GetDirectoryName(Application.dataPath), "Builds");
 
            if (string.IsNullOrEmpty(buildFolder))
                return;
 
            buildPath           = buildFolder;
            streamingAssetsPath = $"{buildFolder}/{PlayerSettings.productName}_Data/StreamingAssets/{Guid}";
        }
        
        public void BuildSubSceneData()
        {
            var tmpBuildFolder = PathCombine(Path.GetDirectoryName(Application.dataPath),
                $"/Library/ContentUpdateBuildDir/{PlayerSettings.productName}");
            var instance   = DotsGlobalSettings.Instance;
            var playerGuid = instance.GetPlayerType() == DotsGlobalSettings.PlayerType.Client ? instance.GetClientGUID() : instance.GetServerGUID();
            if (!playerGuid.IsValid)
                throw new Exception("Invalid Player GUID");
 
            // dredge the sub scenes from the scenes
            var subSceneGuids = new HashSet<Unity.Entities.Hash128>();
            foreach (SceneAsset scene in Scenes)
            {
                string scenePath = AssetDatabase.GetAssetPath(scene);
                GUID sceneGuid = AssetDatabase.GUIDFromAssetPath(scenePath);
                
                var subScenesHashes = EditorEntityScenes.GetSubScenes(sceneGuid);
                foreach (var subSceneHash in subScenesHashes)
                    subSceneGuids.Add(subSceneHash);
            }
 
            // build the data
            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            RemoteContentCatalogBuildUtility.BuildContent(subSceneGuids, playerGuid, buildTarget, tmpBuildFolder);
 
            // publish the data
            var publishFolder = PathCombine(Path.GetDirectoryName(Application.dataPath), "Builds");
            publishFolder = PathCombine(publishFolder, $"{buildPath}-RemoteContent");
 
            RemoteContentCatalogBuildUtility.PublishContent(tmpBuildFolder, publishFolder, _ => new[] { "all" });
        }
 
        private static string PathCombine(string path1, string path2)
        {
            if (!Path.IsPathRooted(path2)) return Path.Combine(path1, path2);
 
            path2 = path2.TrimStart(Path.DirectorySeparatorChar);
            path2 = path2.TrimStart(Path.AltDirectorySeparatorChar);
 
            return Path.Combine(path1, path2);
        }
        
        public void PublishStreamingAssets()
        {
            RemoteContentCatalogBuildUtility.PublishContent(streamingAssetsPath,
                $"{buildPath}-RemoteContent",
                f => new string[] { "all" }, true);
        }
#endif
    }
}
