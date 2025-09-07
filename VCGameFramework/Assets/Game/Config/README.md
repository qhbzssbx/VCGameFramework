# 配置文件目录

本目录统一管理VCGameFramework的各种配置文件和配置相关代码。

## 📁 目录结构

```
Config/
├── AssetSystem/          # 资源系统配置
├── YooAsset/            # YooAsset相关配置
│   └── YooAssetConfig.cs
├── Settings/            # 游戏设置配置
├── Data/                # 静态数据配置
└── Runtime/             # 运行时配置
```

## 🔧 配置说明

### AssetSystem 配置
- 资源地址映射规则
- 资源加载配置
- 对象池配置

### YooAsset 配置
- 资源包配置
- 资源服务器配置
- 资源更新策略

### Settings 配置
- 游戏基础设置
- 画质设置
- 音频设置

### Data 配置
- 静态数据表
- 本地化配置
- 常量定义

## 📝 使用指南

1. **添加新配置**: 在相应的子目录下创建配置文件
2. **配置命名**: 使用 `*Config.cs` 或 `*Settings.cs` 命名规则
3. **接口规范**: 实现 `IConfig` 接口统一管理
4. **文档更新**: 添加配置后及时更新此文档

## 🔗 相关文档

- [框架配置指南](../HotFix/README.md)
- [资源配置文档](../HotFix/AssetSystem/README.md)