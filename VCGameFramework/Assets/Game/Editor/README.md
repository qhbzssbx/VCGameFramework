# 编辑器工具文档

本目录包含VCGameFramework的所有编辑器扩展工具，用于提升开发效率。

## 📁 目录结构

```
Editor/
├── Windows/              # 编辑器窗口
│   └── VCFrameworkManagerWindow.cs
├── Tools/                # 工具类
│   └── AssetDatabaseHelper.cs
├── Inspectors/           # 自定义Inspector
├── Utilities/            # 实用工具
└── Game.Editor.asmdef    # 编辑器程序集定义
```

## 🛠️ 主要工具

### VCFramework 管理器 (`VCFrameworkManagerWindow.cs`)

**访问方式**: `菜单 → VCFramework → 项目管理器`

**功能特性**:
- **项目概览**: 显示项目统计信息和快速导航
- **模块管理**: 查看和创建业务模块
- **资源工具**: 资源管理和优化功能
- **代码生成**: 自动生成模板代码

**主要功能**:
1. **项目统计**
   - 显示代码文件数量
   - 显示Unity版本信息
   - 显示框架版本

2. **快速导航**
   - 直接打开核心目录
   - 快速访问配置文件夹
   - 一键跳转到业务模块

3. **模块创建**
   - 向导式创建新模块
   - 自动生成标准目录结构
   - 生成模板代码文件

4. **资源管理**
   - 刷新资源索引
   - 清理空文件夹
   - 生成资源地址映射

### 资源数据库助手 (`AssetDatabaseHelper.cs`)

**功能特性**:
- **资源查询**: 按类型、路径查找资源
- **依赖分析**: 查看资源依赖关系
- **批量操作**: 批量重命名、删除资源
- **项目统计**: 统计各类资源数量

**常用方法**:

```csharp
// 获取指定路径下的所有材质
var materials = AssetDatabaseHelper.GetAssetsAtPath<Material>("Assets/Materials");

// 获取项目统计信息
var stats = AssetDatabaseHelper.GetProjectStats();
Debug.Log($"项目包含: {stats}");

// 查找资源引用
var references = AssetDatabaseHelper.FindAssetReferences("Assets/Prefabs/Player.prefab");

// 安全删除资源（带确认对话框）
AssetDatabaseHelper.SafeDeleteAsset("Assets/OldAsset.asset");
```

## 🔧 自定义工具开发

### 创建新的编辑器窗口

```csharp
using UnityEngine;
using UnityEditor;

namespace Game.Editor.Windows
{
    public class MyCustomWindow : EditorWindow
    {
        [MenuItem("VCFramework/我的工具")]
        public static void ShowWindow()
        {
            var window = GetWindow<MyCustomWindow>("我的工具");
            window.Show();
        }
        
        private void OnGUI()
        {
            // 绘制UI界面
            GUILayout.Label("我的自定义工具", EditorStyles.boldLabel);
            
            if (GUILayout.Button("执行操作"))
            {
                Debug.Log("执行自定义操作");
            }
        }
    }
}
```

### 创建自定义Inspector

```csharp
using UnityEngine;
using UnityEditor;

namespace Game.Editor.Inspectors
{
    [CustomEditor(typeof(MyComponent))]
    public class MyComponentEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            var component = target as MyComponent;
            
            EditorGUILayout.Space();
            if (GUILayout.Button("自定义操作"))
            {
                component.DoSomething();
            }
        }
    }
}
```

## 📝 使用指南

### 1. 访问工具

所有VCFramework工具都在Unity菜单的 **VCFramework** 分类下：

- `VCFramework → 项目管理器`: 打开主管理界面
- `VCFramework → 我的工具`: 自定义工具（如果有）

### 2. 常见操作

**创建新模块**:
1. 打开项目管理器
2. 切换到"模块管理"标签
3. 点击"创建新模块"
4. 输入模块名称
5. 自动生成模块结构

**清理项目**:
1. 打开项目管理器
2. 切换到"资源工具"标签
3. 点击"清理空文件夹"
4. 自动删除空目录

**查看项目统计**:
1. 打开项目管理器
2. 在"项目概览"标签查看
3. 显示各类文件数量统计

### 3. 扩展开发

如需添加新的编辑器工具：

1. **在相应目录创建脚本**
   - `Windows/`: 编辑器窗口
   - `Tools/`: 工具类
   - `Inspectors/`: 自定义Inspector

2. **使用正确的命名空间**
   ```csharp
   namespace Game.Editor.Windows { }
   namespace Game.Editor.Tools { }
   namespace Game.Editor.Inspectors { }
   ```

3. **添加菜单项**
   ```csharp
   [MenuItem("VCFramework/我的工具", priority = 100)]
   ```

## ⚠️ 注意事项

- 编辑器工具只在Editor模式下可用
- 确保使用 `#if UNITY_EDITOR` 预编译指令
- 工具类应为 `static` 以便直接调用
- 窗口类继承自 `EditorWindow`
- Inspector类继承自 `Editor`

## 🔗 相关文档

- [Unity编辑器扩展官方文档](https://docs.unity3d.com/Manual/ExtendingTheEditor.html)
- [框架核心文档](../HotFix/README.md)
- [示例代码](../Examples/README.md)

---

💡 **提示**: 善用编辑器工具可以大大提升开发效率，建议根据项目需要定制专属工具！