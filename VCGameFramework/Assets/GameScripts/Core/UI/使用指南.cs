/*
 * ========================================
 * 最小UI系统 - 快速使用指南
 * ========================================
 * 
 * 这是一个专为快速demo制作设计的最小UI系统，具有以下特点：
 * 
 * 📋 核心功能：
 * - 简洁的API设计，易于使用
 * - 4层UI层级管理（Background/Window/Popup/Top）
 * - 丰富的UI动画效果
 * - 与现有资源系统集成
 * - 自动资源管理
 * 
 * 🏗️ 架构概览：
 * Core/
 * ├── UISystem.cs      # 主要管理器，单例模式
 * ├── UIPanel.cs       # UI面板基类
 * └── UILayer.cs       # UI层级枚举
 * Extensions/
 * └── UIAnimations.cs  # 动画扩展
 * Examples/
 * ├── DemoUI.cs        # 示例UI面板
 * └── UISystemUsageExample.cs  # 使用示例
 * 
 * 🚀 快速开始：
 * 
 * 1. 创建UI面板类：
 * ```csharp
 * public class MyUI : UIPanel
 * {
 *     protected override async UniTask OnShow(params object[] args)
 *     {
 *         // UI显示时的逻辑
 *         Debug.Log("MyUI显示了！");
 *     }
 * }
 * ```
 * 
 * 2. 在代码中使用：
 * ```csharp
 * // 显示UI
 * await UISystem.Instance.Show<MyUI>("MyUIPrefab", "参数1", "参数2");
 * 
 * // 隐藏UI
 * await UISystem.Instance.Hide<MyUI>();
 * 
 * // 切换显示状态
 * await UISystem.Instance.Toggle<MyUI>("MyUIPrefab");
 * 
 * // 检查UI状态
 * bool isShowing = UISystem.Instance.IsShowing<MyUI>();
 * 
 * // 获取UI实例
 * var myUI = UISystem.Instance.Get<MyUI>();
 * ```
 * 
 * 🎨 动画配置：
 * 在Inspector中可以配置：
 * - Show Animation: 显示动画类型
 * - Hide Animation: 隐藏动画类型
 * - Animation Duration: 动画持续时间
 * 
 * 支持的动画类型：
 * - None: 无动画
 * - Fade: 淡入淡出
 * - Scale: 缩放效果
 * - SlideFromLeft/Top: 滑动效果
 * - PopInOut: 弹出效果
 * 
 * 🏷️ UI层级：
 * - Background (0): 背景层
 * - Window (1000): 窗口层
 * - Popup (2000): 弹窗层
 * - Top (3000): 顶层
 * 
 * 📱 实际使用示例：
 * 
 * ```csharp
 * // 游戏主界面
 * public class MainMenuUI : UIPanel
 * {
 *     [SerializeField] private Button startButton;
 *     [SerializeField] private Button settingsButton;
 *     
 *     protected override void Initialize()
 *     {
 *         uiLayer = UILayer.Window;
 *         showAnimation = UIAnimationType.Fade;
 *         
 *         startButton.onClick.AddListener(() => {
 *             UISystem.Instance.Show<GameUI>("GameUI").Forget();
 *         });
 *     }
 * }
 * 
 * // 游戏设置界面
 * public class SettingsUI : UIPanel
 * {
 *     protected override void Initialize()
 *     {
 *         uiLayer = UILayer.Popup;
 *         isModal = true;
 *         showAnimation = UIAnimationType.PopInOut;
 *         hideAnimation = UIAnimationType.PopInOut;
 *     }
 * }
 * ```
 * 
 * 💡 最佳实践：
 * 
 * 1. UI预制体命名：使用类名作为资源键名
 * 2. 层级选择：根据UI功能选择合适的层级
 * 3. 模态设置：重要确认框使用模态显示
 * 4. 动画选择：根据UI类型选择合适的动画
 * 5. 资源管理：利用自动资源释放特性
 * 
 * 🔧 调试技巧：
 * - 在Examples目录中有完整的使用示例
 * - 使用ContextMenu快速测试UI功能
 * - 查看Console日志了解UI状态变化
 * 
 * 🎯 适用场景：
 * ✅ 快速原型制作
 * ✅ 小型demo项目
 * ✅ 学习UI框架设计
 * ✅ 简单的游戏UI需求
 * 
 * ❌ 不适用场景：
 * ❌ 复杂的企业级应用
 * ❌ 需要复杂UI状态管理
 * ❌ 高性能要求的场景
 * 
 * 祝你快速制作出优秀的demo！🎉
 */