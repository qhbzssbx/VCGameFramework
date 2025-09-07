# VCGameFramework - Unity游戏框架

🎮 一个功能完整、结构清晰的Unity游戏开发框架，特别适用于MMORPG等大型项目。

## ✨ 特性亮点

- 🏗️ **模块化架构**: 基于VContainer的依赖注入系统，自动发现和加载模块
- 📦 **资源管理**: 完整的资源生命周期管理，支持对象池、地址映射、TTL缓存
- 🌊 **流程系统**: 灵活的游戏流程管理，支持主流程和子流程
- 🎨 **UI系统**: 高效的UI资源管理和生命周期控制
- 📱 **热更新**: AOT/HotFix分离架构，支持代码热更新
- 🛠️ **开发工具**: 丰富的编辑器工具，提升开发效率

## 📁 项目结构

```
Assets/Game/
├── AOT/                  # AOT代码目录（不可热更新）
├── HotFix/               # 热更新代码目录
│   ├── Core/             # 核心系统
│   │   ├── Module/       # 模块系统
│   │   ├── FlowSystem/   # 流程系统
│   │   ├── DI/           # 依赖注入
│   │   └── UI/           # UI核心
│   ├── AssetSystem/      # 资源管理系统
│   ├── Infrastructure/   # 基础设施层
│   │   ├── Bootstrap/    # 引导启动
│   │   ├── Camera/       # 相机系统
│   │   └── Resource/     # 资源提供者
│   ├── Modules/          # 业务模块
│   │   ├── Global/       # 全局服务
│   │   ├── Player/       # 玩家模块
│   │   ├── Scene/        # 场景模块
│   │   └── Log/          # 日志模块
│   ├── UI/               # UI系统
│   └── Scenes/           # 场景管理
├── Config/               # 配置文件
├── Examples/             # 示例代码
├── Editor/               # 编辑器工具
│   ├── Windows/          # 编辑器窗口
│   ├── Tools/            # 工具类
│   └── Utilities/        # 实用工具
└── Scenes/               # 游戏场景
```

## 🚀 快速开始

### 1. 环境要求

- Unity 2022.3 LTS 或更高版本
- .NET Standard 2.1
- VContainer (已包含)
- UniTask (已包含)
- YooAsset (已包含)

### 2. 项目初始化

1. **打开项目管理器**
   ```
   菜单 → VCFramework → 项目管理器
   ```

2. **查看项目概览**
   - 项目统计信息
   - 快速导航链接
   - 模块管理界面

### 3. 创建第一个模块

```csharp
using Game.Core;
using VContainer;

public class MyFirstModule : IModule
{
    public void Configure(IContainerBuilder builder)
    {
        builder.Register<IMyService, MyService>(Lifetime.Singleton);
    }
}
```

### 4. 资源使用示例

```csharp
public class ExampleScript : MonoBehaviour
{
    [Inject] private IAssetScope _assetScope;
    [Inject] private CancellationToken _cancellationToken;
    
    private async UniTask LoadAssetExample()
    {
        // Pin资源 - 长期持有
        var prefab = await _assetScope.PinAsync<GameObject>(
            "ui", "MyPrefab", _cancellationToken);
        
        // Lease资源 - 短期使用
        await using var config = await _assetScope.LeaseAsync<TextAsset>(
            "config", "GameConfig", _cancellationToken);
    }
}
```

## 📚 详细文档

### 核心系统文档
- [模块系统](HotFix/Core/README.md) - 依赖注入和模块管理
- [资源系统](HotFix/AssetSystem/使用说明.md) - 资源管理完整指南
- [流程系统](HotFix/Core/FlowSystem/README.md) - 游戏流程控制

### 扩展系统文档
- [UI系统](HotFix/UI/README.md) - UI框架使用指南
- [相机系统](HotFix/Infrastructure/Camera/README.md) - 相机控制文档

### 开发指南
- [示例代码](Examples/README.md) - 各种使用示例
- [配置管理](Config/README.md) - 配置文件管理
- [编辑器工具](Editor/README.md) - 开发工具使用

## 🛠️ 开发工具

### VCFramework 管理器
通过 `VCFramework → 项目管理器` 菜单访问：

- **项目概览**: 查看项目统计和快速导航
- **模块管理**: 管理和创建新模块
- **资源工具**: 资源管理和优化工具
- **代码生成**: 自动生成模板代码

### 常用功能
- **清理空文件夹**: 自动清理项目中的空文件夹
- **模块创建向导**: 快速创建标准模块结构
- **资源索引刷新**: 更新资源数据库

## 🏗️ 架构设计

### 分层架构
```
表现层 (Presentation)     → UI、场景、表现逻辑
应用层 (Application)      → 业务流程、模块协调
领域层 (Domain)          → 业务逻辑、领域模型  
基础设施层 (Infrastructure) → 技术实现、外部集成
```

### 依赖流向
- 上层依赖下层，下层不依赖上层
- 通过接口和事件实现松耦合
- DI容器管理对象生命周期

## 📈 性能优化

- **程序集分离**: 独立的Assembly Definition提升编译速度
- **对象池**: 减少GC压力，提升运行时性能
- **资源缓存**: TTL缓存机制，平衡内存和加载速度
- **异步加载**: 基于UniTask的高性能异步操作

## 🤝 贡献指南

1. **Fork** 本项目
2. **创建** 特性分支 (`git checkout -b feature/AmazingFeature`)
3. **提交** 你的更改 (`git commit -m 'Add some AmazingFeature'`)
4. **推送** 到分支 (`git push origin feature/AmazingFeature`)
5. **打开** Pull Request

## 📄 许可证

本项目采用 MIT 许可证 - 详见 [LICENSE](LICENSE) 文件

## 🙋 问题与支持

- 📖 查看 [文档](Docs/README.md) 解决常见问题
- 🐛 提交 [Issue](https://github.com/your-repo/issues) 报告问题
- 💬 加入社区群聊讨论技术问题

---

⭐ 如果这个框架对你有帮助，请给个 Star 支持一下！