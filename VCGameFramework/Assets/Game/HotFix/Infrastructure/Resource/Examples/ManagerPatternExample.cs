using Cysharp.Threading.Tasks;
using Game.Infrastructure.Resource.Core;
using UnityEngine;
using YooAsset;

namespace Game.Infrastructure.Resource.Examples
{
    /// <summary>
    /// Manager模式资源管理示例
    /// 展示如何在Manager类中正确管理Prefab资源，避免复制实例导致的资源泄漏问题
    /// </summary>
    public class ManagerPatternExample : MonoBehaviour
    {
        [Header("测试用Prefab资源")]
        [SerializeField] private string enemyPrefabName = "EnemyPrefab";
        [SerializeField] private string playerPrefabName = "PlayerPrefab";
        [SerializeField] private string effectPrefabName = "EffectPrefab";

        // Manager持有的资源加载器，管理所有Prefab资源
        private ResourceLoader _resourceLoader = new();
        
        // 预加载的Prefab资源Handle
        private AssetHandle _enemyPrefabHandle;
        private AssetHandle _playerPrefabHandle;
        private AssetHandle _effectPrefabHandle;

        // 生成的实例列表（用于演示）
        private readonly System.Collections.Generic.List<GameObject> _instances = new();

        private async void Start()
        {
            Debug.Log("=== Manager模式资源管理示例 ===");
            
            await PreloadAllPrefabs();
            await DemonstrateManagerPattern();
            
            Debug.Log("=== Manager模式示例完成 ===");
        }

        /// <summary>
        /// 预加载所有Prefab资源
        /// Manager在初始化时预加载需要的Prefab资源
        /// </summary>
        private async UniTask PreloadAllPrefabs()
        {
            Debug.Log("--- 预加载Prefab资源 ---");
            
            try
            {
                // 使用LoadPrefabForInstantiate方法，专门用于需要实例化的Prefab
                _enemyPrefabHandle = await _resourceLoader.LoadPrefabForInstantiate<GameObject>(enemyPrefabName);
                _playerPrefabHandle = await _resourceLoader.LoadPrefabForInstantiate<GameObject>(playerPrefabName);
                _effectPrefabHandle = await _resourceLoader.LoadPrefabForInstantiate<GameObject>(effectPrefabName);
                
                Debug.Log($"✅ 预加载完成，ResourceLoader管理的Handle数量: {_resourceLoader.HandleCount}");
                
                // 验证资源加载状态
                ValidatePrefabHandles();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ 预加载失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 演示Manager模式的各种操作
        /// </summary>
        private async UniTask DemonstrateManagerPattern()
        {
            // 1. 创建实例
            CreateInstances();
            
            await UniTask.Delay(2000);
            
            // 2. 复制实例（安全操作）
            CloneInstances();
            
            await UniTask.Delay(2000);
            
            // 3. 批量销毁实例
            DestroyAllInstances();
            
            // 4. 重新创建（展示资源复用）
            await UniTask.Delay(1000);
            ReuseResources();
        }

        /// <summary>
        /// 创建游戏对象实例
        /// </summary>
        private void CreateInstances()
        {
            Debug.Log("--- 创建游戏对象实例 ---");
            
            // 创建敌人实例
            if (_enemyPrefabHandle.IsValid)
            {
                var enemy = Instantiate(_enemyPrefabHandle.AssetObject as GameObject);
                enemy.name = "Enemy_001";
                enemy.transform.position = new Vector3(-2, 0, 0);
                _instances.Add(enemy);
                Debug.Log($"🏹 创建敌人实例: {enemy.name}");
            }
            
            // 创建玩家实例
            if (_playerPrefabHandle.IsValid)
            {
                var player = Instantiate(_playerPrefabHandle.AssetObject as GameObject);
                player.name = "Player_001";
                player.transform.position = new Vector3(0, 0, 0);
                _instances.Add(player);
                Debug.Log($"🎮 创建玩家实例: {player.name}");
            }
            
            // 创建特效实例
            if (_effectPrefabHandle.IsValid)
            {
                var effect = Instantiate(_effectPrefabHandle.AssetObject as GameObject);
                effect.name = "Effect_001";
                effect.transform.position = new Vector3(2, 0, 0);
                _instances.Add(effect);
                Debug.Log($"✨ 创建特效实例: {effect.name}");
            }
            
            Debug.Log($"📊 当前实例数量: {_instances.Count}");
        }

        /// <summary>
        /// 复制实例 - 展示安全的复制操作
        /// 由于Prefab资源由Manager管理，复制实例是安全的
        /// </summary>
        private void CloneInstances()
        {
            Debug.Log("--- 复制实例（安全操作）---");
            
            var originalCount = _instances.Count;
            var clonedInstances = new System.Collections.Generic.List<GameObject>();
            
            foreach (var instance in _instances.ToArray())
            {
                if (instance != null)
                {
                    // 安全复制：原始资源Handle由Manager持有，不会因为实例销毁而释放
                    var clone = Instantiate(instance);
                    clone.name = instance.name + "_Clone";
                    clone.transform.position = instance.transform.position + Vector3.right * 4;
                    
                    clonedInstances.Add(clone);
                    _instances.Add(clone);
                    
                    Debug.Log($"📋 复制实例: {clone.name}");
                }
            }
            
            Debug.Log($"✅ 复制完成，原有{originalCount}个，新增{clonedInstances.Count}个，总计{_instances.Count}个");
        }

        /// <summary>
        /// 销毁所有实例
        /// </summary>
        private void DestroyAllInstances()
        {
            Debug.Log("--- 销毁所有实例 ---");
            
            var count = _instances.Count;
            foreach (var instance in _instances)
            {
                if (instance != null)
                {
                    DestroyImmediate(instance);
                }
            }
            
            _instances.Clear();
            Debug.Log($"🗑️ 已销毁 {count} 个实例，Prefab资源仍然由Manager持有");
            
            // 验证资源Handle仍然有效
            ValidatePrefabHandles();
        }

        /// <summary>
        /// 重用资源 - 展示资源的可重复使用
        /// </summary>
        private void ReuseResources()
        {
            Debug.Log("--- 重用资源 ---");
            
            // 资源可以重复使用，无需重新加载
            if (_playerPrefabHandle.IsValid)
            {
                var newPlayer = Instantiate(_playerPrefabHandle.AssetObject as GameObject);
                newPlayer.name = "Player_Reused";
                newPlayer.transform.position = new Vector3(0, 2, 0);
                _instances.Add(newPlayer);
                
                Debug.Log($"♻️ 重用玩家Prefab创建: {newPlayer.name}");
            }
            
            Debug.Log($"📊 重用后实例数量: {_instances.Count}");
        }

        /// <summary>
        /// 验证Prefab Handle的有效性
        /// </summary>
        private void ValidatePrefabHandles()
        {
            Debug.Log("--- 验证Prefab资源状态 ---");
            
            Debug.Log($"🏹 Enemy Prefab Handle有效: {_enemyPrefabHandle?.IsValid == true}");
            Debug.Log($"🎮 Player Prefab Handle有效: {_playerPrefabHandle?.IsValid == true}");
            Debug.Log($"✨ Effect Prefab Handle有效: {_effectPrefabHandle?.IsValid == true}");
            Debug.Log($"📊 ResourceLoader管理的Handle总数: {_resourceLoader.HandleCount}");
        }

        /// <summary>
        /// 获取指定类型的实例数量
        /// </summary>
        /// <param name="namePrefix">名称前缀</param>
        /// <returns>实例数量</returns>
        private int GetInstanceCount(string namePrefix)
        {
            int count = 0;
            foreach (var instance in _instances)
            {
                if (instance != null && instance.name.StartsWith(namePrefix))
                {
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// Manager销毁时释放所有资源
        /// </summary>
        private void OnDestroy()
        {
            Debug.Log($"🔄 ManagerPatternExample销毁");
            
            // 清理所有实例
            DestroyAllInstances();
            
            // 释放ResourceLoader，这会释放所有Prefab资源Handle
            _resourceLoader?.Dispose();
            
            Debug.Log("✅ Manager资源清理完成");
        }

        /// <summary>
        /// Inspector测试方法
        /// </summary>
        [ContextMenu("显示资源状态")]
        private void ShowResourceStatus()
        {
            ValidatePrefabHandles();
            Debug.Log($"当前实例分布:");
            Debug.Log($"  - Enemy实例: {GetInstanceCount("Enemy")}个");
            Debug.Log($"  - Player实例: {GetInstanceCount("Player")}个");
            Debug.Log($"  - Effect实例: {GetInstanceCount("Effect")}个");
            Debug.Log($"  - 总实例数: {_instances.Count}个");
        }

        [ContextMenu("测试重用资源")]
        private void TestReuseResources()
        {
            ReuseResources();
        }

        [ContextMenu("清理所有实例")]
        private void TestDestroyAllInstances()
        {
            DestroyAllInstances();
        }
    }
}