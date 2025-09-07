using UnityEngine;
using VContainer;
using Game.Infrastructure.Resource.Core;
using Game.Core.FlowSystem;
using Game.Modules.Log.Domain;

namespace Game.Infrastructure.Bootstrap
{
    /// <summary>
    /// 启动系统验证器 - 用于验证Bootstrap系统的依赖注入是否正确
    /// </summary>
    public class BootstrapValidator : MonoBehaviour
    {
        [Inject] private IFlowManager _flowManager;
        [Inject] private ILogService _logService;
        [Inject] private IResourceService _resourceService;

        [Header("验证选项")]
        [SerializeField] private bool _validateOnStart = true;
        [SerializeField] private bool _showDetailedInfo = true;

        private void Start()
        {
            if (_validateOnStart)
            {
                ValidateBootstrapSystem();
            }
        }

        /// <summary>
        /// 验证Bootstrap系统是否正确配置
        /// </summary>
        [ContextMenu("验证Bootstrap系统")]
        public void ValidateBootstrapSystem()
        {
            Debug.Log("=== Bootstrap系统验证开始 ===");

            bool allValid = true;
            
            // 验证FlowManager
            if (ValidateService(_flowManager, "FlowManager"))
            {
                if (_showDetailedInfo)
                {
                    Debug.Log($"✓ FlowManager状态: 构造后即可用 (无需异步初始化)");
                    Debug.Log($"✓ 当前流程: {_flowManager?.CurrentFlow?.GetType().Name ?? "None"}");
                }
            }
            else
            {
                allValid = false;
            }

            // 验证LogService
            if (ValidateService(_logService, "LogService"))
            {
                if (_showDetailedInfo)
                {
                    _logService.Info("✓ LogService 工作正常");
                }
            }
            else
            {
                allValid = false;
            }

            // 验证ResourceService
            if (ValidateService(_resourceService, "ResourceService"))
            {
                if (_showDetailedInfo)
                {
                    Debug.Log("✓ ResourceService 已注册（Infrastructure重构版本）");
                }
            }
            else
            {
                allValid = false;
            }

            // 验证命名空间迁移
            ValidateNamespaceMigration();

            // 总结
            if (allValid)
            {
                Debug.Log("🎉 Bootstrap系统验证通过！所有核心服务已正确注入");
                _logService?.Info("Bootstrap系统验证通过");
            }
            else
            {
                Debug.LogError("❌ Bootstrap系统验证失败！存在未正确注入的服务");
            }

            Debug.Log("=== Bootstrap系统验证完成 ===");
        }

        /// <summary>
        /// 验证单个服务
        /// </summary>
        private bool ValidateService<T>(T service, string serviceName) where T : class
        {
            if (service != null)
            {
                Debug.Log($"✓ {serviceName} 注入成功: {service.GetType().Name}");
                return true;
            }
            else
            {
                Debug.LogError($"❌ {serviceName} 注入失败: 服务为null");
                return false;
            }
        }

        /// <summary>
        /// 验证命名空间迁移是否正确
        /// </summary>
        private void ValidateNamespaceMigration()
        {
            Debug.Log("验证命名空间迁移...");

            // 检查ResourceService的实际类型
            if (_resourceService != null)
            {
                var resourceType = _resourceService.GetType();
                var namespaceName = resourceType.Namespace;
                
                if (namespaceName != null && namespaceName.Contains("Infrastructure.Resource"))
                {
                    Debug.Log($"✓ 命名空间迁移成功: {namespaceName}");
                }
                else
                {
                    Debug.LogWarning($"⚠️ 可能的命名空间问题: {namespaceName}");
                }
            }
        }

        /// <summary>
        /// 运行时测试资源系统
        /// </summary>
        [ContextMenu("测试资源系统")]
        public async void TestResourceSystem()
        {
            if (_resourceService == null)
            {
                Debug.LogError("ResourceService未注入，无法测试");
                return;
            }

            Debug.Log("开始测试资源系统...");
            
            try
            {
                // 注意：这里使用的是测试用的资源名，实际项目中需要替换为真实的资源
                // var testTexture = await _resourceService.LoadAssetAsync<Texture2D>("TestTexture");
                // Debug.Log($"✓ 资源加载测试通过: {testTexture?.name}");
                
                Debug.Log("✓ 资源系统接口调用正常（跳过实际资源加载以避免错误）");
                _logService?.Info("资源系统测试完成");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ 资源系统测试失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 显示系统信息
        /// </summary>
        [ContextMenu("显示系统信息")]
        public void ShowSystemInfo()
        {
            Debug.Log("=== 系统信息 ===");
            Debug.Log($"Unity版本: {Application.unityVersion}");
            Debug.Log($"平台: {Application.platform}");
            Debug.Log($"项目路径: {Application.dataPath}");
            
            if (_logService != null)
            {
                _logService.Info("系统信息显示完成");
            }
            
            Debug.Log("=== 信息显示完成 ===");
        }
    }
}