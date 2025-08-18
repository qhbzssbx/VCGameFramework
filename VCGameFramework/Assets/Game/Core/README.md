# VCGameFramework 模块系统使用文档

## 📋 目录
- [系统概述](#系统概述)
- [快速开始](#快速开始)
- [核心接口详解](#核心接口详解)
- [创建模块指南](#创建模块指南)
- [模块加载机制](#模块加载机制)
- [最佳实践](#最佳实践)
- [示例模块分析](#示例模块分析)
- [故障排除](#故障排除)
- [迁移指南](#迁移指南)

## 🎯 系统概述

VCGameFramework 的模块系统是一个基于**反射自动发现**和**依赖注入**的模块化架构，旨在简化大型Unity项目的模块管理和依赖注册。

### 核心特性
- ✅ **自动发现**: 通过反射自动扫描并加载所有模块
- ✅ **优先级控制**: 支持模块加载顺序控制
- ✅ **异步初始化**: 支持复杂模块的异步初始化
- ✅ **零配置**: 新增模块无需手动注册
- ✅ **类型安全**: 基于强类型接口的设计
- ✅ **易于测试**: 完全基于依赖注入，便于单元测试

### 系统架构
```
ModuleLoader (反射扫描) → 模块发现 → 优先级排序 → 依赖注册 → 异步初始化
```

## 🚀 快速开始

### 1. 启用模块系统
在你的项目启动类中调用模块加载器：

```csharp
using Game.Core;
using VContainer;
using VContainer.Unity;

public class ProjectBootstrap : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // 一行代码启用整个模块系统！
        ModuleLoader.RegisterAllModules(builder);
    }
}
```

### 2. 创建你的第一个模块
```csharp
using Game.Core;
using VContainer;

namespace MyGame.Modules
{
    public class MyFirstModule : IModule
    {
        public void Configure(IContainerBuilder builder)
        {
            // 在这里注册你的服务
            builder.Register<IMyService, MyService>(Lifetime.Singleton);
        }
    }
}
```

就这么简单！你的模块会被自动发现和加载。

## 🔧 核心接口详解

### IModule - 基础模块接口
所有模块的基础接口，定义了模块的配置方法。

```csharp
public interface IModule
{
    void Configure(IContainerBuilder builder);
}
```

**使用场景**: 简单的服务注册，无特殊加载要求的模块。

### IModuleWithOrder - 带优先级的模块
当你需要控制模块加载顺序时使用。

```csharp
public interface IModuleWithOrder : IModule
{
    int Order { get; }  // 数值越小，优先级越高
}
```

**优先级建议**:
- `-1000`: 核心基础服务 (如 ProjectModule)
- `-100`: 系统级模块 (如 FlowSystemModule, LogModule)  
- `-50`: 业务模块 (如 GameFlowModule)
- `0`: 默认优先级
- `100+`: 后加载的模块

### IAsyncModule - 异步模块
需要异步初始化的模块（如需要加载资源、连接网络等）。

```csharp
public interface IAsyncModule : IModule
{
    UniTask InitializeAsync(IObjectResolver resolver);
}
```

**使用场景**: 
- 资源系统初始化
- 网络连接建立
- 数据库连接
- 远程配置加载

## 📚 创建模块指南

### 基础模块示例
```csharp
using Game.Core;
using VContainer;

namespace MyGame.Audio
{
    /// <summary>
    /// 音频模块 - 注册音频相关服务
    /// </summary>
    public class AudioModule : IModule
    {
        public void Configure(IContainerBuilder builder)
        {
            builder.Register<IAudioManager, AudioManager>(Lifetime.Singleton);
            builder.Register<IAudioPlayer, AudioPlayer>(Lifetime.Singleton);
            builder.Register<IAudioConfig, AudioConfig>(Lifetime.Singleton);
        }
    }
}
```

### 带优先级的模块示例
```csharp
using Game.Core;
using VContainer;

namespace MyGame.Config
{
    /// <summary>
    /// 配置模块 - 需要优先加载的基础配置服务
    /// </summary>
    public class ConfigModule : IModuleWithOrder
    {
        // 高优先级，需要在其他模块前加载
        public int Order => -500;
        
        public void Configure(IContainerBuilder builder)
        {
            builder.Register<IConfigService, ConfigService>(Lifetime.Singleton);
            builder.Register<IGameSettings, GameSettings>(Lifetime.Singleton);
        }
    }
}
```

### 异步模块示例
```csharp
using Game.Core;
using VContainer;
using Cysharp.Threading.Tasks;

namespace MyGame.Network
{
    /// <summary>
    /// 网络模块 - 需要异步初始化网络连接
    /// </summary>
    public class NetworkModule : IModuleWithOrder, IAsyncModule
    {
        public int Order => -200;  // 在业务模块前初始化
        
        public void Configure(IContainerBuilder builder)
        {
            builder.Register<INetworkManager, NetworkManager>(Lifetime.Singleton);
            builder.Register<IApiClient, ApiClient>(Lifetime.Singleton);
        }
        
        public async UniTask InitializeAsync(IObjectResolver resolver)
        {
            var networkManager = resolver.Resolve<INetworkManager>();
            
            // 异步初始化网络连接
            await networkManager.ConnectAsync();
            
            // 加载远程配置
            var apiClient = resolver.Resolve<IApiClient>();
            await apiClient.LoadRemoteConfigAsync();
            
            UnityEngine.Debug.Log("Network module initialized successfully");
        }
    }
}
```

### 复杂业务模块示例
```csharp
using Game.Core;
using VContainer;

namespace MyGame.Player
{
    /// <summary>
    /// 玩家模块 - 注册玩家相关的所有服务
    /// </summary>
    public class PlayerModule : IModuleWithOrder
    {
        public int Order => 0;  // 默认优先级
        
        public void Configure(IContainerBuilder builder)
        {
            // 注册玩家数据服务
            RegisterPlayerServices(builder);
            
            // 注册玩家行为服务
            RegisterPlayerBehaviors(builder);
            
            // 注册玩家UI服务
            RegisterPlayerUI(builder);
        }
        
        private void RegisterPlayerServices(IContainerBuilder builder)
        {
            builder.Register<IPlayerDataService, PlayerDataService>(Lifetime.Singleton);
            builder.Register<IPlayerProgressService, PlayerProgressService>(Lifetime.Singleton);
            builder.Register<IPlayerInventoryService, PlayerInventoryService>(Lifetime.Singleton);
        }
        
        private void RegisterPlayerBehaviors(IContainerBuilder builder)
        {
            builder.Register<IPlayerController, PlayerController>(Lifetime.Singleton);
            builder.Register<IPlayerMovement, PlayerMovement>(Lifetime.Transient);
            builder.Register<IPlayerCombat, PlayerCombat>(Lifetime.Transient);
        }
        
        private void RegisterPlayerUI(IContainerBuilder builder)
        {
            builder.Register<IPlayerUIManager, PlayerUIManager>(Lifetime.Singleton);
            builder.Register<IPlayerHUD, PlayerHUD>(Lifetime.Singleton);
        }
    }
}
```

## ⚙️ 模块加载机制

### 1. 自动发现阶段
```csharp
// ModuleLoader.DiscoverModules() 实现原理
var assemblies = AppDomain.CurrentDomain.GetAssemblies();
foreach (var type in assemblies.SelectMany(a => a.GetTypes()))
{
    if (moduleType.IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
    {
        if (Activator.CreateInstance(type) is IModule module)
            result.Add(module);
    }
}
```

### 2. 优先级排序
```csharp
// 按 Order 属性排序，默认为 0
return result.OrderBy(m =>
{
    if (m is IModuleWithOrder ordered)
        return ordered.Order;
    return 0;
}).ToList();
```

### 3. 同步注册阶段
```csharp
foreach (var module in modules)
{
    module.Configure(builder);  // 调用模块的 Configure 方法
    if (module is IAsyncModule asyncModule)
        asyncModules.Add(asyncModule);  // 收集异步模块
}
```

### 4. 异步初始化阶段
```csharp
// ModuleInitializer 会在游戏启动时执行
public async UniTaskVoid Initialize()
{
    foreach (var module in modules)
    {
        await module.InitializeAsync(resolver);
    }
}
```

## 💡 最佳实践

### 1. 模块设计原则
- **单一职责**: 每个模块只负责一个特定的功能域
- **最小依赖**: 模块之间尽量减少依赖关系
- **接口隔离**: 通过接口暴露服务，隐藏实现细节
- **配置集中**: 所有注册逻辑都在 Configure 方法中

### 2. 命名规范
```csharp
// ✅ 推荐的命名方式
AudioModule          // 功能模块
PlayerModule         // 业务模块  
NetworkModule        // 系统模块
ProjectModule        // 全局模块

// ❌ 避免的命名方式
AudioInstaller       // 这是 VContainer 的旧方式
PlayerSetup         // 不够明确
GameStuff           // 太宽泛
```

### 3. 优先级设置指南
```csharp
// 推荐的优先级层次
-1000: ProjectModule     (全局基础服务)
-500:  ConfigModule      (配置服务)
-200:  NetworkModule     (网络服务)
-100:  FlowSystemModule  (流程系统)
-50:   GameFlowModule    (游戏流程)
0:     PlayerModule      (业务模块)
100:   UIModule          (界面模块)
```

### 4. 异步初始化的使用建议
```csharp
public async UniTask InitializeAsync(IObjectResolver resolver)
{
    try
    {
        // 1. 获取依赖的服务
        var service = resolver.Resolve<IMyService>();
        
        // 2. 执行异步初始化
        await service.InitializeAsync();
        
        // 3. 记录成功日志
        Debug.Log($"{GetType().Name} initialized successfully");
    }
    catch (Exception ex)
    {
        // 4. 错误处理和日志
        Debug.LogError($"Failed to initialize {GetType().Name}: {ex.Message}");
        throw; // 重新抛出异常，阻止后续模块初始化
    }
}
```

### 5. 模块内部组织
```csharp
public class MyComplexModule : IModuleWithOrder, IAsyncModule
{
    public int Order => -100;
    
    public void Configure(IContainerBuilder builder)
    {
        // 按功能分组注册
        RegisterCoreServices(builder);
        RegisterManagers(builder);
        RegisterUtilities(builder);
    }
    
    // 私有方法组织注册逻辑
    private void RegisterCoreServices(IContainerBuilder builder) { }
    private void RegisterManagers(IContainerBuilder builder) { }
    private void RegisterUtilities(IContainerBuilder builder) { }
    
    public async UniTask InitializeAsync(IObjectResolver resolver)
    {
        // 分步骤初始化
        await InitializeCoreServices(resolver);
        await InitializeManagers(resolver);
        await ValidateInitialization(resolver);
    }
}
```

## 📖 示例模块分析

### ProjectModule - 全局基础模块
```csharp
/// <summary>
/// 优先级: -1000 (最高优先级)
/// 职责: 注册全局单例服务，为其他模块提供基础设施
/// </summary>
public class ProjectModule : IModuleWithOrder
{
    public int Order => -1000;  // 最高优先级，确保基础服务最先可用
    
    public void Configure(IContainerBuilder builder)
    {
        // 核心基础服务
        builder.Register<INetworkService, NetworkService>(Lifetime.Singleton);
        builder.Register<IAccountService, AccountService>(Lifetime.Singleton);
        builder.Register<IMasterDataService, MasterDataService>(Lifetime.Singleton);
        builder.Register<IInventoryService, InventoryService>(Lifetime.Singleton);
        builder.Register<IGlobalEventBus, GlobalEventBus>(Lifetime.Singleton);
    }
}
```

**设计亮点**:
- 最高优先级确保基础服务最先注册
- 只注册真正全局需要的服务
- 服务都是单例，保证全局唯一性

### ResourceModule - 异步资源模块
```csharp
/// <summary>
/// 优先级: -99 (系统级)
/// 职责: 资源管理系统，支持异步初始化
/// </summary>
public class ResourceModule : IModuleWithOrder, IAsyncModule
{
    public int Order => -99;  // 系统级优先级
    
    public void Configure(IContainerBuilder builder)
    {
        // 资源服务注册
        builder.Register<IResourceService, YooAssetResourceProvider>(Lifetime.Singleton);
        builder.Register<ResourceService>(Lifetime.Singleton);
    }
    
    public async UniTask InitializeAsync(IObjectResolver resolver)
    {
        // 异步初始化资源系统
        await resolver.Resolve<ResourceService>().InitializeAsync();
    }
}
```

**设计亮点**:
- 同时实现 IModuleWithOrder 和 IAsyncModule
- 先注册服务，后异步初始化
- 资源系统的初始化通常需要时间，适合异步处理

### FlowSystemModule - 复杂系统模块
```csharp
/// <summary>
/// 优先级: -100 (系统级)
/// 职责: 流程系统的完整配置，包含多个子系统
/// </summary>
public class FlowSystemModule : IModuleWithOrder
{
    public int Order => -100;
    
    public void Configure(IContainerBuilder builder)
    {
        RegisterCoreComponents(builder);
        RegisterManagers(builder);
        RegisterEventSystem(builder);
        RegisterInitializer(builder);
    }
    
    private void RegisterCoreComponents(IContainerBuilder builder)
    {
        builder.Register<IFlowManager, FlowManager>(Lifetime.Singleton);
        builder.Register<ISubFlowManager, SubFlowManager>(Lifetime.Singleton);
        // ...
    }
    
    // 其他私有注册方法...
}
```

**设计亮点**:
- 复杂模块通过私有方法组织注册逻辑
- 按功能域分组注册相关服务
- 清晰的代码结构便于维护

### GameFlowModule - 业务流程模块
```csharp
/// <summary>
/// 优先级: -50 (业务级)
/// 职责: 注册具体的游戏流程类
/// </summary>
public class GameFlowModule : IModuleWithOrder
{
    public int Order => -50;  // 在系统模块之后，业务模块优先级
    
    public void Configure(IContainerBuilder builder)
    {
        RegisterMainFlows(builder);
        RegisterSubFlows(builder);
    }
    
    private void RegisterMainFlows(IContainerBuilder builder)
    {
        FlowSystemModule.RegisterFlow<LaunchFlow>(builder);
        FlowSystemModule.RegisterFlow<LoginFlow>(builder);
        // ...
    }
}
```

**设计亮点**:
- 依赖 FlowSystemModule 提供的静态注册方法
- 业务级优先级，在系统模块后加载
- 按流程类型分组注册

## 🔧 故障排除

### 常见问题及解决方案

#### 1. 模块未被发现
**症状**: 模块的 Configure 方法没有被调用

**可能原因**:
- 模块类不是 public
- 模块类是抽象类或接口
- 模块类没有无参构造函数
- 程序集没有被正确加载

**解决方案**:
```csharp
// ✅ 正确的模块定义
public class MyModule : IModule  // public 可见性
{
    // 无参构造函数（可省略）
    public MyModule() { }
    
    public void Configure(IContainerBuilder builder)
    {
        // 配置逻辑
    }
}

// ❌ 错误的模块定义
internal class MyModule : IModule { }     // 不是 public
public abstract class MyModule { }       // 抽象类
public class MyModule(string param) { }  // 没有无参构造函数
```

#### 2. 模块加载顺序问题
**症状**: 模块A依赖模块B，但B在A之后加载

**解决方案**:
```csharp
// 设置正确的优先级
public class ModuleB : IModuleWithOrder
{
    public int Order => -100;  // 更小的数值 = 更高的优先级
}

public class ModuleA : IModuleWithOrder  
{
    public int Order => -50;   // 在 ModuleB 之后加载
}
```

#### 3. 异步模块初始化失败
**症状**: 异步模块抛出异常，后续模块无法初始化

**解决方案**:
```csharp
public async UniTask InitializeAsync(IObjectResolver resolver)
{
    try
    {
        // 初始化逻辑
        await SomeAsyncOperation();
    }
    catch (Exception ex)
    {
        Debug.LogError($"Module {GetType().Name} initialization failed: {ex}");
        
        // 决定是否重新抛出异常
        // throw; // 阻止后续模块初始化
        // 或者继续执行，不影响其他模块
    }
}
```

#### 4. 依赖注入循环依赖
**症状**: VContainer 抛出循环依赖异常

**解决方案**:
```csharp
// ❌ 循环依赖
public class ServiceA 
{
    public ServiceA(IServiceB serviceB) { }
}

public class ServiceB 
{
    public ServiceB(IServiceA serviceA) { }  // 循环依赖
}

// ✅ 解决方案：引入中介者或重构依赖关系
public class ServiceA 
{
    public ServiceA(IEventBus eventBus) { }  // 通过事件总线通信
}

public class ServiceB 
{
    public ServiceB(IEventBus eventBus) { }
}
```

#### 5. 性能问题
**症状**: 游戏启动时间过长

**排查方法**:
```csharp
public void Configure(IContainerBuilder builder)
{
    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    
    // 你的注册代码
    RegisterServices(builder);
    
    stopwatch.Stop();
    Debug.Log($"{GetType().Name} configuration took {stopwatch.ElapsedMilliseconds}ms");
}
```

### 调试技巧

#### 1. 启用详细日志
```csharp
public class MyModule : IModule
{
    public void Configure(IContainerBuilder builder)
    {
        Debug.Log($"Configuring {GetType().Name}...");
        
        builder.Register<IMyService, MyService>(Lifetime.Singleton);
        Debug.Log("MyService registered");
        
        Debug.Log($"{GetType().Name} configuration completed");
    }
}
```

#### 2. 验证模块加载
```csharp
// 在 ProjectBootstrap 中添加验证
protected override void Configure(IContainerBuilder builder)
{
    var modules = ModuleLoader.DiscoverModules();
    Debug.Log($"Found {modules.Count} modules:");
    
    foreach (var module in modules)
    {
        var order = module is IModuleWithOrder ordered ? ordered.Order : 0;
        Debug.Log($"  - {module.GetType().Name} (Order: {order})");
    }
    
    ModuleLoader.RegisterAllModules(builder);
}
```

## 🔄 迁移指南

### 从传统 VContainer 迁移到模块系统

#### 1. 迁移 Installer 类
**原始代码 (VContainer Installer)**:
```csharp
public class PlayerInstaller : IInstaller
{
    public void Install(IContainerBuilder builder)
    {
        builder.Register<IPlayerService, PlayerService>(Lifetime.Singleton);
        builder.Register<IPlayerController, PlayerController>(Lifetime.Singleton);
    }
}

// 在 LifetimeScope 中手动注册
public class GameLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstaller<PlayerInstaller>();
    }
}
```

**迁移后 (Module System)**:
```csharp
public class PlayerModule : IModule
{
    public void Configure(IContainerBuilder builder)
    {
        builder.Register<IPlayerService, PlayerService>(Lifetime.Singleton);
        builder.Register<IPlayerController, PlayerController>(Lifetime.Singleton);
    }
}

// 在 LifetimeScope 中自动发现
public class GameLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // 自动发现和注册所有模块，包括 PlayerModule
        ModuleLoader.RegisterAllModules(builder);
    }
}
```

#### 2. 迁移优先级控制
**原始代码**:
```csharp
// 通过手动注册顺序控制
protected override void Configure(IContainerBuilder builder)
{
    builder.RegisterInstaller<CoreInstaller>();      // 先注册核心
    builder.RegisterInstaller<NetworkInstaller>();   // 再注册网络
    builder.RegisterInstaller<PlayerInstaller>();    // 最后注册玩家
}
```

**迁移后**:
```csharp
public class CoreModule : IModuleWithOrder
{
    public int Order => -100;  // 高优先级
}

public class NetworkModule : IModuleWithOrder  
{
    public int Order => -50;   // 中等优先级
}

public class PlayerModule : IModule
{
    // 默认优先级 0，最后加载
}
```

#### 3. 迁移异步初始化
**原始代码**:
```csharp
public class NetworkInstaller : IInstaller
{
    public void Install(IContainerBuilder builder)
    {
        builder.Register<INetworkManager, NetworkManager>(Lifetime.Singleton);
        
        // 需要手动处理异步初始化
        builder.RegisterEntryPoint<NetworkInitializer>(Lifetime.Singleton);
    }
}

public class NetworkInitializer : IStartable
{
    public void Start()
    {
        // 异步初始化逻辑
    }
}
```

**迁移后**:
```csharp
public class NetworkModule : IAsyncModule
{
    public void Configure(IContainerBuilder builder)
    {
        builder.Register<INetworkManager, NetworkManager>(Lifetime.Singleton);
    }
    
    public async UniTask InitializeAsync(IObjectResolver resolver)
    {
        var networkManager = resolver.Resolve<INetworkManager>();
        await networkManager.InitializeAsync();
    }
}
```

### 渐进式迁移策略

#### 阶段1: 并行运行
```csharp
public class GameLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // 保留原有的 Installer 注册
        builder.RegisterInstaller<LegacyInstaller>();
        
        // 添加新的模块系统
        ModuleLoader.RegisterAllModules(builder);
    }
}
```

#### 阶段2: 逐步迁移
每次迁移一个 Installer 到 Module，确保系统稳定运行。

#### 阶段3: 完全切换
移除所有 Installer 注册，只保留模块系统。

---

## 📝 总结

VCGameFramework 的模块系统通过以下特性大大简化了大型Unity项目的架构管理：

1. **零配置自动发现** - 新增模块无需手动注册
2. **优先级控制** - 灵活控制模块加载顺序  
3. **异步初始化支持** - 处理复杂的异步初始化场景
4. **强类型安全** - 基于接口的设计提供编译时检查
5. **易于测试** - 完全基于依赖注入，便于单元测试

通过遵循本文档的指导原则和最佳实践，你可以构建出结构清晰、易于维护的模块化Unity应用程序。

**记住**: 好的模块设计应该遵循单一职责原则，保持最小依赖，并通过清晰的接口暴露服务。模块系统是架构工具，最终目标是让代码更易于理解和维护。

---

💡 **提示**: 如果你在使用过程中遇到问题，请参考[故障排除](#故障排除)章节，或查看项目中的现有模块实现作为参考。