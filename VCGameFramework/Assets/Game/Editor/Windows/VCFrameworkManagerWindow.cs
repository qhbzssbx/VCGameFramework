using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

namespace Game.Editor.Windows
{
    /// <summary>
    /// VCGameFramework 项目管理窗口
    /// 提供项目概览、快速导航、代码生成等功能
    /// </summary>
    public class VCFrameworkManagerWindow : EditorWindow
    {
        private Vector2 _scrollPosition;
        private int _selectedTab = 0;
        private readonly string[] _tabs = { "项目概览", "模块管理", "资源工具", "代码生成" };
        
        [MenuItem("VCFramework/项目管理器", priority = 1)]
        public static void ShowWindow()
        {
            var window = GetWindow<VCFrameworkManagerWindow>("VCFramework 管理器");
            window.minSize = new Vector2(600, 400);
            window.Show();
        }
        
        private void OnGUI()
        {
            DrawHeader();
            
            _selectedTab = GUILayout.Toolbar(_selectedTab, _tabs);
            
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            
            switch (_selectedTab)
            {
                case 0:
                    DrawProjectOverview();
                    break;
                case 1:
                    DrawModuleManagement();
                    break;
                case 2:
                    DrawResourceTools();
                    break;
                case 3:
                    DrawCodeGeneration();
                    break;
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("VCGameFramework 项目管理器", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("刷新", EditorStyles.toolbarButton))
            {
                Repaint();
            }
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawProjectOverview()
        {
            EditorGUILayout.LabelField("项目概览", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField("框架信息", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("版本: 1.0.0");
                EditorGUILayout.LabelField("Unity版本: " + Application.unityVersion);
                EditorGUILayout.Space();
                
                var hotfixPath = "Assets/Game/HotFix";
                var aotPath = "Assets/Game/AOT";
                
                if (Directory.Exists(hotfixPath))
                {
                    var csFiles = Directory.GetFiles(hotfixPath, "*.cs", SearchOption.AllDirectories);
                    EditorGUILayout.LabelField($"HotFix代码文件: {csFiles.Length} 个");
                }
                
                if (Directory.Exists(aotPath))
                {
                    var csFiles = Directory.GetFiles(aotPath, "*.cs", SearchOption.AllDirectories);
                    EditorGUILayout.LabelField($"AOT代码文件: {csFiles.Length} 个");
                }
            }
            
            EditorGUILayout.Space();
            DrawQuickNavigation();
        }
        
        private void DrawQuickNavigation()
        {
            EditorGUILayout.LabelField("快速导航", EditorStyles.boldLabel);
            
            using (new EditorGUILayout.VerticalScope("box"))
            {
                if (GUILayout.Button("核心模块目录"))
                    OpenFolder("Assets/Game/HotFix/Core");
                    
                if (GUILayout.Button("资源系统目录"))
                    OpenFolder("Assets/Game/HotFix/AssetSystem");
                    
                if (GUILayout.Button("业务模块目录"))
                    OpenFolder("Assets/Game/HotFix/Modules");
                    
                if (GUILayout.Button("UI系统目录"))
                    OpenFolder("Assets/Game/HotFix/UI");
                    
                if (GUILayout.Button("配置目录"))
                    OpenFolder("Assets/AssetRaw/Configs");
            }
        }
        
        private void DrawModuleManagement()
        {
            EditorGUILayout.LabelField("模块管理", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField("已发现的模块", EditorStyles.boldLabel);
                
                var modulesPath = "Assets/Game/HotFix/Modules";
                if (Directory.Exists(modulesPath))
                {
                    var moduleDirectories = Directory.GetDirectories(modulesPath);
                    foreach (var moduleDir in moduleDirectories)
                    {
                        var moduleName = Path.GetFileName(moduleDir);
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField($"📁 {moduleName}");
                        if (GUILayout.Button("打开", GUILayout.Width(50)))
                        {
                            OpenFolder(moduleDir);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
                
                EditorGUILayout.Space();
                if (GUILayout.Button("创建新模块"))
                {
                    CreateNewModuleWizard();
                }
            }
        }
        
        private void DrawResourceTools()
        {
            EditorGUILayout.LabelField("资源工具", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField("资源管理", EditorStyles.boldLabel);
                
                if (GUILayout.Button("刷新资源索引"))
                {
                    AssetDatabase.Refresh();
                    Debug.Log("资源索引已刷新");
                }
                
                if (GUILayout.Button("清理空文件夹"))
                {
                    CleanEmptyFolders();
                }
                
                if (GUILayout.Button("生成资源地址映射"))
                {
                    Debug.Log("资源地址映射生成功能待实现");
                }
            }
            
            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField("AssetBundle工具", EditorStyles.boldLabel);
                
                if (GUILayout.Button("打开YooAsset设置"))
                {
                    // 这里可以打开YooAsset的设置窗口
                    Debug.Log("请手动打开 YooAsset -> AssetBundle Collector");
                }
            }
        }
        
        private void DrawCodeGeneration()
        {
            EditorGUILayout.LabelField("代码生成", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField("模块生成", EditorStyles.boldLabel);
                
                if (GUILayout.Button("生成空白模块"))
                {
                    CreateNewModuleWizard();
                }
                
                if (GUILayout.Button("生成服务接口"))
                {
                    Debug.Log("服务接口生成功能待实现");
                }
            }
            
            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField("UI代码生成", EditorStyles.boldLabel);
                
                if (GUILayout.Button("从预制体生成UI代码"))
                {
                    Debug.Log("UI代码生成功能待实现");
                }
            }
        }
        
        private void OpenFolder(string path)
        {
            if (Directory.Exists(path))
            {
                EditorUtility.RevealInFinder(path);
            }
            else
            {
                Debug.LogWarning($"目录不存在: {path}");
            }
        }
        
        private void CreateNewModuleWizard()
        {
            var moduleName = EditorInputDialog.Show("创建新模块", "请输入模块名称:");
            if (!string.IsNullOrEmpty(moduleName))
            {
                CreateModule(moduleName);
            }
        }
        
        private void CreateModule(string moduleName)
        {
            var modulePath = $"Assets/Game/HotFix/Modules/{moduleName}";
            
            Directory.CreateDirectory($"{modulePath}/Application");
            Directory.CreateDirectory($"{modulePath}/Domain");
            Directory.CreateDirectory($"{modulePath}/Infrastructure");
            
            // 创建模块类
            var moduleContent = $@"using Game.Core;
using VContainer;

namespace Game.HotFix.Modules.{moduleName}.Application
{{
    /// <summary>
    /// {moduleName}模块
    /// </summary>
    public class {moduleName}Module : IModule
    {{
        public void Configure(IContainerBuilder builder)
        {{
            // 在这里注册{moduleName}模块的服务
            // builder.Register<I{moduleName}Service, {moduleName}Service>(Lifetime.Singleton);
        }}
    }}
}}";
            
            File.WriteAllText($"{modulePath}/Application/{moduleName}Module.cs", moduleContent);
            
            AssetDatabase.Refresh();
            Debug.Log($"模块 {moduleName} 创建成功！路径: {modulePath}");
        }
        
        private void CleanEmptyFolders()
        {
            var gamePath = "Assets/Game";
            var emptyFolders = Directory.GetDirectories(gamePath, "*", SearchOption.AllDirectories)
                .Where(dir => Directory.GetFiles(dir, "*", SearchOption.AllDirectories).Length == 0)
                .OrderByDescending(dir => dir.Length)
                .ToArray();
                
            foreach (var folder in emptyFolders)
            {
                if (Directory.Exists(folder) && Directory.GetFileSystemEntries(folder).Length == 0)
                {
                    Directory.Delete(folder);
                    File.Delete(folder + ".meta");
                }
            }
            
            if (emptyFolders.Length > 0)
            {
                AssetDatabase.Refresh();
                Debug.Log($"清理了 {emptyFolders.Length} 个空文件夹");
            }
            else
            {
                Debug.Log("没有发现空文件夹");
            }
        }
    }
    
    /// <summary>
    /// 编辑器输入对话框
    /// </summary>
    public class EditorInputDialog : EditorWindow
    {
        private string _inputText = "";
        private string _description = "";
        private System.Action<string> _onConfirm;
        
        public static string Show(string title, string description)
        {
            var window = CreateInstance<EditorInputDialog>();
            window.titleContent = new GUIContent(title);
            window._description = description;
            window.minSize = new Vector2(300, 120);
            window.maxSize = new Vector2(300, 120);
            window.ShowModal();
            
            return window._inputText;
        }
        
        private void OnGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(_description);
            EditorGUILayout.Space();
            
            _inputText = EditorGUILayout.TextField(_inputText);
            
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("确定"))
            {
                Close();
            }
            
            if (GUILayout.Button("取消"))
            {
                _inputText = "";
                Close();
            }
            
            EditorGUILayout.EndHorizontal();
        }
    }
}