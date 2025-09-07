using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace Game.Editor.Tools
{
    /// <summary>
    /// 资源数据库辅助工具
    /// 提供项目资源管理相关的工具方法
    /// </summary>
    public static class AssetDatabaseHelper
    {
        /// <summary>
        /// 获取指定目录下所有指定类型的资源
        /// </summary>
        public static T[] GetAssetsAtPath<T>(string path) where T : Object
        {
            var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { path });
            return guids
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<T>)
                .Where(asset => asset != null)
                .ToArray();
        }
        
        /// <summary>
        /// 获取项目中所有脚本文件
        /// </summary>
        public static string[] GetAllScriptPaths()
        {
            return AssetDatabase.FindAssets("t:MonoScript")
                .Select(AssetDatabase.GUIDToAssetPath)
                .ToArray();
        }
        
        /// <summary>
        /// 创建文件夹（如果不存在）
        /// </summary>
        public static void CreateFolderIfNotExists(string path)
        {
            var folders = path.Split('/').Skip(1).ToArray(); // 跳过 "Assets"
            var currentPath = "Assets";
            
            foreach (var folder in folders)
            {
                var newPath = $"{currentPath}/{folder}";
                if (!AssetDatabase.IsValidFolder(newPath))
                {
                    AssetDatabase.CreateFolder(currentPath, folder);
                }
                currentPath = newPath;
            }
        }
        
        /// <summary>
        /// 安全删除资源（带确认）
        /// </summary>
        public static bool SafeDeleteAsset(string assetPath)
        {
            if (EditorUtility.DisplayDialog("确认删除", $"确定要删除资源: {assetPath}?", "删除", "取消"))
            {
                return AssetDatabase.DeleteAsset(assetPath);
            }
            return false;
        }
        
        /// <summary>
        /// 批量重命名资源
        /// </summary>
        public static void BatchRenameAssets(string[] assetPaths, string newNameTemplate)
        {
            for (int i = 0; i < assetPaths.Length; i++)
            {
                var path = assetPaths[i];
                var directory = Path.GetDirectoryName(path);
                var extension = Path.GetExtension(path);
                var newName = string.Format(newNameTemplate, i);
                var newPath = Path.Combine(directory, newName + extension).Replace('\\', '/');
                
                AssetDatabase.RenameAsset(path, newName);
            }
        }
        
        /// <summary>
        /// 获取资源依赖信息
        /// </summary>
        public static string[] GetAssetDependencies(string assetPath, bool recursive = true)
        {
            return AssetDatabase.GetDependencies(assetPath, recursive);
        }
        
        /// <summary>
        /// 查找引用指定资源的其他资源
        /// </summary>
        public static string[] FindAssetReferences(string assetPath)
        {
            var allAssets = AssetDatabase.GetAllAssetPaths();
            var references = new List<string>();
            
            foreach (var asset in allAssets)
            {
                var dependencies = AssetDatabase.GetDependencies(asset, false);
                if (dependencies.Contains(assetPath))
                {
                    references.Add(asset);
                }
            }
            
            return references.ToArray();
        }
        
        /// <summary>
        /// 获取项目统计信息
        /// </summary>
        public static ProjectStats GetProjectStats()
        {
            var stats = new ProjectStats();
            var allAssets = AssetDatabase.GetAllAssetPaths();
            
            foreach (var asset in allAssets)
            {
                if (!asset.StartsWith("Assets/"))
                    continue;
                    
                var extension = Path.GetExtension(asset).ToLower();
                
                switch (extension)
                {
                    case ".cs":
                        stats.ScriptCount++;
                        break;
                    case ".prefab":
                        stats.PrefabCount++;
                        break;
                    case ".mat":
                        stats.MaterialCount++;
                        break;
                    case ".png":
                    case ".jpg":
                    case ".jpeg":
                    case ".tga":
                        stats.TextureCount++;
                        break;
                    case ".fbx":
                    case ".obj":
                        stats.ModelCount++;
                        break;
                    case ".wav":
                    case ".mp3":
                    case ".ogg":
                        stats.AudioCount++;
                        break;
                    case ".unity":
                        stats.SceneCount++;
                        break;
                }
            }
            
            return stats;
        }
        
        /// <summary>
        /// 项目统计信息
        /// </summary>
        public struct ProjectStats
        {
            public int ScriptCount;
            public int PrefabCount;
            public int MaterialCount;
            public int TextureCount;
            public int ModelCount;
            public int AudioCount;
            public int SceneCount;
            
            public override string ToString()
            {
                return $"脚本: {ScriptCount}, 预制体: {PrefabCount}, 材质: {MaterialCount}, " +
                       $"纹理: {TextureCount}, 模型: {ModelCount}, 音频: {AudioCount}, 场景: {SceneCount}";
            }
        }
    }
}