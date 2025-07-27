using UnityEngine;
using Cysharp.Threading.Tasks;
using GameScript.Core.UI.Core;

namespace GameScript.Core.UI.Examples
{
    /// <summary>
    /// UI系统使用示例
    /// 展示如何在代码中使用新的UI系统
    /// </summary>
    public class UISystemUsageExample : MonoBehaviour
    {
        [Header("测试配置")]
        [SerializeField] private KeyCode showDemoUIKey = KeyCode.Space;
        [SerializeField] private KeyCode hideDemoUIKey = KeyCode.Escape;
        [SerializeField] private KeyCode toggleDemoUIKey = KeyCode.T;
        
        private void Start()
        {
            Debug.Log("=== UI系统使用示例 ===");
            Debug.Log($"按 {showDemoUIKey} 显示Demo UI");
            Debug.Log($"按 {hideDemoUIKey} 隐藏Demo UI");
            Debug.Log($"按 {toggleDemoUIKey} 切换Demo UI");
        }
        
        private void Update()
        {
            // 测试按键
            if (Input.GetKeyDown(showDemoUIKey))
            {
                ShowDemoUI().Forget();
            }
            
            if (Input.GetKeyDown(hideDemoUIKey))
            {
                HideDemoUI().Forget();
            }
            
            if (Input.GetKeyDown(toggleDemoUIKey))
            {
                ToggleDemoUI().Forget();
            }
        }
        
        /// <summary>
        /// 显示Demo UI的示例
        /// </summary>
        [ContextMenu("显示Demo UI")]
        public async UniTaskVoid ShowDemoUI()
        {
            Debug.Log("--- 显示Demo UI示例 ---");
            
            try
            {
                // 基本用法：显示UI并传递参数
                var demoUI = await UISystem.Instance.Show<DemoUI>("DemoUI", "Hello from UI System!");
                
                if (demoUI != null)
                {
                    Debug.Log("✅ Demo UI显示成功");
                    
                    // 可以对返回的UI实例进行操作
                    Debug.Log($"UI层级: {demoUI.Layer}");
                    Debug.Log($"是否模态: {demoUI.IsModal}");
                }
                else
                {
                    Debug.LogError("❌ Demo UI显示失败");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ 显示Demo UI时发生错误: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 隐藏Demo UI的示例
        /// </summary>
        [ContextMenu("隐藏Demo UI")]
        public async UniTaskVoid HideDemoUI()
        {
            Debug.Log("--- 隐藏Demo UI示例 ---");
            
            try
            {
                // 检查UI是否正在显示
                if (UISystem.Instance.IsShowing<DemoUI>())
                {
                    await UISystem.Instance.Hide<DemoUI>();
                    Debug.Log("✅ Demo UI隐藏成功");
                }
                else
                {
                    Debug.Log("ℹ️ Demo UI当前未显示");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ 隐藏Demo UI时发生错误: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 切换Demo UI显示状态的示例
        /// </summary>
        [ContextMenu("切换Demo UI")]
        public async UniTaskVoid ToggleDemoUI()
        {
            Debug.Log("--- 切换Demo UI示例 ---");
            
            try
            {
                await UISystem.Instance.Toggle<DemoUI>("DemoUI", "切换显示的消息");
                
                bool isShowing = UISystem.Instance.IsShowing<DemoUI>();
                Debug.Log($"✅ Demo UI切换完成，当前状态: {(isShowing ? "显示" : "隐藏")}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ 切换Demo UI时发生错误: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 获取UI实例的示例
        /// </summary>
        [ContextMenu("获取Demo UI实例")]
        public void GetDemoUIInstance()
        {
            Debug.Log("--- 获取Demo UI实例示例 ---");
            
            var demoUI = UISystem.Instance.Get<DemoUI>();
            
            if (demoUI != null)
            {
                Debug.Log("✅ 获取到Demo UI实例");
                Debug.Log($"UI名称: {demoUI.name}");
                Debug.Log($"是否显示: {demoUI.IsShowing}");
                Debug.Log($"UI层级: {demoUI.Layer}");
                Debug.Log($"自动销毁: {demoUI.AutoDestroy}");
            }
            else
            {
                Debug.Log("ℹ️ Demo UI实例不存在或已销毁");
            }
        }
        
        /// <summary>
        /// 演示多个UI同时显示
        /// </summary>
        [ContextMenu("显示多个UI")]
        public async UniTaskVoid ShowMultipleUIs()
        {
            Debug.Log("--- 显示多个UI示例 ---");
            
            try
            {
                // 显示主UI
                await UISystem.Instance.Show<DemoUI>("DemoUI", "主UI面板");
                
                // 延迟一下，然后显示弹窗
                await UniTask.Delay(1000);
                await UISystem.Instance.Show<DemoPopupUI>("DemoPopup", "这是一个弹窗");
                
                Debug.Log("✅ 多个UI显示完成");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ 显示多个UI时发生错误: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 演示关闭所有UI
        /// </summary>
        [ContextMenu("关闭所有UI")]
        public async UniTaskVoid HideAllUIs()
        {
            Debug.Log("--- 关闭所有UI示例 ---");
            
            try
            {
                await UISystem.Instance.HideAll();
                Debug.Log("✅ 所有UI已关闭");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ 关闭所有UI时发生错误: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 演示UI状态查询
        /// </summary>
        [ContextMenu("查询UI状态")]
        public void QueryUIStatus()
        {
            Debug.Log("--- UI状态查询示例 ---");
            
            Debug.Log($"DemoUI 是否显示: {UISystem.Instance.IsShowing<DemoUI>()}");
            Debug.Log($"DemoPopupUI 是否显示: {UISystem.Instance.IsShowing<DemoPopupUI>()}");
            
            var demoUI = UISystem.Instance.Get<DemoUI>();
            var popupUI = UISystem.Instance.Get<DemoPopupUI>();
            
            Debug.Log($"DemoUI 实例存在: {demoUI != null}");
            Debug.Log($"DemoPopupUI 实例存在: {popupUI != null}");
        }
    }
}