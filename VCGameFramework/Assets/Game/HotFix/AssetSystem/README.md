Game.HotFix.AssetSystem（YooAsset句柄+VContainer生命周期）

要点
- Registry（Singleton）：集中管理 address -> YooAsset 句柄与 RefCount；屏蔽业务对 YooAsset 的直接依赖。
- Scope（Scoped/按需创建）：绑定业务生命周期（UI/系统/流程）；Pin 资源随 Scope.Dispose 统一释放。
- API：IAssetRegistry.PinAsync/LeaseAsync/PreloadAsync，IAssetScope 同名转调。
- 迁移策略：删除旧 Resource 系统代码后，业务通过 Scope/Registry 获取资源；UI 通过 ScopedUIResourceLoader 适配，无需改动 UISystem。

未实现（留空扩展）
- TTL 淘汰与 TopN 统计
- 调试覆盖层 UI 与指标
- Address 映射与标签筛选
- 场景句柄的高级策略

使用
- 模块自动注册：AssetModule（Game.Infrastructure.AssetSystem.Module）会被 SmartModuleLoader 扫描并初始化 YooAsset。
- UI：`IUIResourceLoader` 已替换为 `ScopedUIResourceLoader`，内部使用 AssetScope 固定 UI Prefab。

