# VCGameFramework 流程系统

## 概述

VCGameFramework流程系统是一个强大且灵活的游戏状态管理解决方案，支持主流程和子流程的嵌套管理，完美集成VContainer依赖注入和MessagePipe事件系统。

## 核心特性

✅ **分层流程管理** - 主流程管理游戏阶段，子流程管理游戏内状态  
✅ **异步支持** - 全面使用UniTask进行异步操作  
✅ **依赖注入集成** - 深度集成VContainer  
✅ **事件驱动架构** - 集成MessagePipe事件系统  
✅ **完善的管理器** - 时间、音频、输入管理器  
✅ **数据传递机制** - 强类型的流程上下文系统  
✅ **错误处理** - 完善的异常捕获和恢复机制  

## 系统架构

```
FlowSystemModule (VContainer模块)
├── 核心组件
│   ├── IFlowManager & FlowManager (主流程管理)
│   ├── ISubFlowManager & SubFlowManager (子流程管理)
│   ├── IFlowEventPublisher & FlowEventPublisher (事件发布)
│   └── FlowContext & FlowContextBuilder (数据传递)
├── 管理器组件
│   ├── ITimeManager & TimeManager (时间控制)
│   ├── IAudioManager & AudioManager (音频管理)
│   └── IInputManager & InputManager (输入处理)
└── 初始化器
    └── FlowSystemInitializer (系统启动和配置)
```

## 快速开始

### 1. 注册模块

在您的项目中注册FlowSystemModule：

```csharp
// 在ModuleLoader中或LifetimeScope中注册
public class ProjectLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // 注册流程系统模块
        var flowModule = new FlowSystemModule();
        flowModule.Configure(builder);
        
        // 注册您的游戏流程模块
        var gameFlowModule = new GameFlowModule();
        gameFlowModule.Configure(builder);
    }
}
```

### 2. 创建自定义流程

#### 主流程示例

```csharp
public class LaunchFlow : BaseMainFlow
{
    private readonly ITimeManager timeManager;
    private readonly ILogService logService;
    
    public LaunchFlow(ITimeManager timeManager, ILogService logService)
    {
        this.timeManager = timeManager;
        this.logService = logService;
    }
    
    public override int Priority => 0; // 最高优先级
    
    protected override async UniTask OnEnterInternal(FlowContext context)
    {
        logService.Info("Game launching...");
        
        // 初始化核心系统
        await InitializeCoreSystem();
        
        // 加载基础资源
        await LoadEssentialResources();
        
        // 自动切换到热更新流程
        var flowManager = context.GetTyped<IFlowManager>();
        await flowManager.SwitchToFlow<HotUpdateFlow>();
    }
    
    private async UniTask InitializeCoreSystem()
    {
        // 初始化逻辑
        await UniTask.Delay(1000);
    }
    
    private async UniTask LoadEssentialResources()
    {
        // 资源加载逻辑
        await UniTask.Delay(500);
    }
}
```

#### 子流程示例

```csharp
public class PauseMenuSubFlow : BaseSubFlow
{
    private readonly ITimeManager timeManager;
    private readonly IAudioManager audioManager;
    private readonly IUIManager uiManager;
    
    public override bool ShouldPauseParent => true;
    
    public PauseMenuSubFlow(
        ITimeManager timeManager, 
        IAudioManager audioManager,
        IUIManager uiManager)
    {
        this.timeManager = timeManager;
        this.audioManager = audioManager;
        this.uiManager = uiManager;
    }
    
    protected override async UniTask OnEnterInternal(FlowContext context)
    {
        // 暂停游戏
        timeManager.PauseGame();
        audioManager.PauseAllSFX();
        
        // 显示暂停UI
        await uiManager.ShowUIAsync<PauseMenuUI>();
    }
    
    protected override async UniTask OnExitInternal()
    {
        // 恢复游戏
        timeManager.ResumeGame();
        audioManager.ResumeAllSFX();
        
        // 隐藏暂停UI
        await uiManager.HideUIAsync<PauseMenuUI>();
    }
}
```

### 3. 注册流程

```csharp
public class GameFlowModule : IModuleWithOrder
{
    public int Order => -50; // 在FlowSystemModule之后
    
    public void Configure(IContainerBuilder builder)
    {
        // 注册主流程
        FlowSystemModule.RegisterFlow<LaunchFlow>(builder);
        FlowSystemModule.RegisterFlow<HotUpdateFlow>(builder);
        FlowSystemModule.RegisterFlow<LoginFlow>(builder);
        FlowSystemModule.RegisterFlow<GameMainFlow>(builder);
        
        // 注册子流程
        FlowSystemModule.RegisterSubFlow<GamePlaySubFlow>(builder);
        FlowSystemModule.RegisterSubFlow<PauseMenuSubFlow>(builder);
        FlowSystemModule.RegisterSubFlow<SettingsSubFlow>(builder);
        FlowSystemModule.RegisterSubFlow<InventorySubFlow>(builder);
    }
}
```

### 4. 使用流程系统

```csharp
public class GameController : MonoBehaviour
{
    private IFlowManager flowManager;
    private ISubFlowManager subFlowManager;
    private IInputManager inputManager;
    
    [Inject]
    public void Construct(
        IFlowManager flowManager,
        ISubFlowManager subFlowManager,
        IInputManager inputManager)
    {
        this.flowManager = flowManager;
        this.subFlowManager = subFlowManager;
        this.inputManager = inputManager;
        
        // 监听输入事件
        inputManager.OnPausePressed += OnPausePressed;
    }
    
    private async void Start()
    {
        // 启动游戏流程
        await flowManager.SwitchToFlow<LaunchFlow>();
    }
    
    private async void OnPausePressed()
    {
        // 切换到暂停菜单
        if (subFlowManager.IsCurrentSubFlow<GamePlaySubFlow>())
        {
            await subFlowManager.PushSubFlow<PauseMenuSubFlow>();
        }
        else if (subFlowManager.IsCurrentSubFlow<PauseMenuSubFlow>())
        {
            await subFlowManager.PopSubFlow();
        }
    }
}
```

## 数据传递

使用FlowContext在流程间传递数据：

```csharp
// 创建上下文数据
var context = FlowContextBuilder.Create()
    .WithData("PlayerName", "Alice")
    .WithData("Level", 5)
    .WithTypedData(new PlayerData { Health = 100 })
    .Build();

// 切换流程并传递数据
await flowManager.SwitchToFlow<GameMainFlow>(context);

// 在目标流程中获取数据
protected override async UniTask OnEnterInternal(FlowContext context)
{
    var playerName = context.Get<string>("PlayerName");
    var level = context.Get<int>("Level");
    var playerData = context.GetTyped<PlayerData>();
    
    // 使用数据进行初始化
}
```

## 事件监听

监听流程事件：

```csharp
public class FlowEventLogger
{
    [Inject]
    public void Construct(ISubscriber<FlowEvent> subscriber)
    {
        subscriber.Subscribe(OnFlowEvent);
    }
    
    private void OnFlowEvent(FlowEvent flowEvent)
    {
        switch (flowEvent.EventType)
        {
            case FlowEventType.FlowEntered:
                Debug.Log($"进入流程: {flowEvent.FlowName}");
                break;
                
            case FlowEventType.MainFlowSwitched:
                Debug.Log($"主流程切换: {flowEvent}");
                break;
                
            case FlowEventType.FlowError:
                Debug.LogError($"流程错误: {flowEvent.Error?.Message}");
                break;
        }
    }
}
```

## 管理器使用

### 时间管理器

```csharp
// 暂停游戏
timeManager.PauseGame();

// 设置慢动作
timeManager.SetTimeScale(0.5f);

// 嵌套时间控制
timeManager.PushTimeScale(0.1f); // 更慢
timeManager.PopTimeScale(); // 恢复到0.5f
```

### 音频管理器

```csharp
// 播放音效
audioManager.PlaySFX(clickSound);

// 播放3D音效
audioManager.PlaySFXAtPosition(explosionSound, enemyPosition);

// 播放音乐并淡入
audioManager.PlayMusic(backgroundMusic, loop: true, fadeInDuration: 2f);

// 切换音乐
audioManager.SwitchMusic(newMusic, fadeOutDuration: 1f, fadeInDuration: 1f);
```

### 输入管理器

```csharp
// 设置输入状态
inputManager.SetUIOnlyMode();
inputManager.SetGameOnlyMode();
inputManager.DisableInput();

// 添加输入屏蔽（模态对话框）
inputManager.PushInputBlockLayer("ModalDialog");
inputManager.RemoveInputBlockLayer("ModalDialog");

// 检查输入
if (inputManager.GetKeyDown(KeyCode.Space))
{
    // 处理空格键
}

var movement = inputManager.GetMovementVector();
```

## 最佳实践

### 1. 流程设计原则

- **单一职责**: 每个流程只负责一个明确的游戏状态
- **松耦合**: 流程间通过FlowContext传递数据，避免直接引用
- **错误处理**: 在OnEnterInternal/OnExitInternal中妥善处理异常

### 2. 性能优化

- 流程中避免创建大量临时对象
- 使用UniTask避免不必要的协程开销
- 及时释放不需要的资源

### 3. 调试技巧

- 开启Debug模式查看详细日志
- 使用InputManager的OnGUI显示实时状态
- 监听FlowEvent进行流程跟踪

## 扩展指南

### 添加新管理器

1. 创建接口和实现类
2. 在FlowSystemModule中注册
3. 在FlowSystemInitializer中初始化

### 创建自定义事件

1. 扩展FlowEventType枚举
2. 在FlowEvent中添加创建方法
3. 在需要的地方发布事件

## 故障排除

### 常见问题

1. **流程切换失败**: 检查流程是否已注册到容器
2. **依赖注入失败**: 确保所有依赖都已正确注册
3. **事件不触发**: 检查MessagePipe配置和订阅

### 调试工具

- 使用FlowSystemUsageExample进行功能测试
- 查看Unity Console中的详细日志
- 使用VContainer的诊断工具检查注册情况

## 示例项目

查看`Examples`文件夹中的示例代码：

- `FlowSystemUsageExample.cs` - 完整的使用示例
- `GameFlowModule.cs` - 模块注册示例

---

该流程系统为您的游戏开发提供了坚实的基础架构，让您能够专注于游戏逻辑的实现，而不需要担心状态管理的复杂性。