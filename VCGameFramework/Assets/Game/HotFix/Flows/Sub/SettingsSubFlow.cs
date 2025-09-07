using Cysharp.Threading.Tasks;
using Game.Infrastructure.Managers;
using Game.Core.FlowSystem;
using Game.Modules.Log.Domain;
using UnityEngine;

namespace Game.Flows.Sub
{
    /// <summary>
    /// 设置类别枚举
    /// </summary>
    public enum SettingsCategory
    {
        Audio,      // 音频设置
        Graphics,   // 图形设置
        Controls,   // 控制设置
        Gameplay,   // 游戏性设置
        General     // 通用设置
    }
    
    /// <summary>
    /// 设置菜单子流程 - 游戏设置界面
    /// </summary>
    public class SettingsSubFlow : BaseSubFlow
    {
        private readonly ILogService _logService;
        private readonly IAudioManager _audioManager;
        private readonly IInputManager _inputManager;
        private readonly ITimeManager _timeManager;
        private readonly ISubFlowManager _subFlowManager;
        
        private SettingsCategory _currentCategory = SettingsCategory.General;
        private bool _hasUnsavedChanges = false;
        private SettingsData _originalSettings;
        private SettingsData _currentSettings;
        
        /// <summary>
        /// 设置菜单需要暂停父流程
        /// </summary>
        public override bool ShouldPauseParent => true;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public SettingsSubFlow(
            ILogService logService,
            IAudioManager audioManager,
            IInputManager inputManager,
            ITimeManager timeManager,
            ISubFlowManager subFlowManager)
        {
            _logService = logService;
            _audioManager = audioManager;
            _inputManager = inputManager;
            _timeManager = timeManager;
            _subFlowManager = subFlowManager;
        }
        
        /// <summary>
        /// 进入设置菜单
        /// </summary>
        protected override async UniTask OnEnterInternal(FlowContext context)
        {
            _logService.Info("进入设置菜单");
            
            // 获取来源信息
            var fromFlow = context?.Get<string>("FromFlow") ?? "Unknown";
            var initialCategory = context?.Get<string>("Category");
            
            _logService.Info($"从 {fromFlow} 进入设置菜单");
            
            // 设置初始类别
            if (!string.IsNullOrEmpty(initialCategory) && 
                System.Enum.TryParse<SettingsCategory>(initialCategory, out var category))
            {
                _currentCategory = category;
            }
            
            // 设置UI输入模式
            _inputManager.SetUIOnlyMode();
            
            // 加载当前设置
            await LoadCurrentSettings();
            
            // 显示设置UI
            await ShowSettingsUI();
            
            // 设置输入处理
            SetupSettingsInput();
            
            _logService.Info("设置菜单显示完成");
        }
        
        /// <summary>
        /// 加载当前设置
        /// </summary>
        private async UniTask LoadCurrentSettings()
        {
            _logService.Info("加载当前设置...");
            
            // 从PlayerPrefs或配置文件加载设置
            _currentSettings = new SettingsData
            {
                // 音频设置
                MasterVolume = _audioManager.MasterVolume,
                SFXVolume = _audioManager.SFXVolume,
                MusicVolume = _audioManager.MusicVolume,
                SFXMuted = _audioManager.IsSFXMuted,
                MusicMuted = _audioManager.IsMusicMuted,
                
                // 图形设置
                QualityLevel = QualitySettings.GetQualityLevel(),
                FullScreen = Screen.fullScreen,
                Resolution = new Vector2Int(Screen.width, Screen.height),
                VSync = QualitySettings.vSyncCount > 0,
                
                // 控制设置
                MouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 1.0f),
                InvertMouseY = PlayerPrefs.GetInt("InvertMouseY", 0) == 1,
                
                // 游戏性设置
                AutoSave = PlayerPrefs.GetInt("AutoSave", 1) == 1,
                ShowHints = PlayerPrefs.GetInt("ShowHints", 1) == 1,
                
                // 通用设置
                Language = PlayerPrefs.GetString("Language", "Chinese"),
                ShowFPS = PlayerPrefs.GetInt("ShowFPS", 0) == 1
            };
            
            // 备份原始设置，用于取消时恢复
            _originalSettings = _currentSettings.Clone();
            _hasUnsavedChanges = false;
            
            await UniTask.Delay(300); // 模拟加载时间
            _logService.Info("✓ 设置加载完成");
        }
        
        /// <summary>
        /// 显示设置UI
        /// </summary>
        private async UniTask ShowSettingsUI()
        {
            _logService.Info($"显示设置UI - 当前类别: {_currentCategory}");
            
            // 这里应该显示设置界面的UI
            // 包括不同的设置类别标签和对应的设置选项
            
            await UniTask.Delay(500); // 模拟UI显示时间
            _logService.Info("✓ 设置UI显示完成");
        }
        
        /// <summary>
        /// 设置输入处理
        /// </summary>
        private void SetupSettingsInput()
        {
            _inputManager.OnBackPressed += OnBackPressed;
            _inputManager.OnConfirmPressed += OnConfirmPressed;
        }
        
        /// <summary>
        /// 切换设置类别
        /// </summary>
        public async UniTask SwitchCategory(SettingsCategory category)
        {
            if (_currentCategory == category) return;
            
            _logService.Info($"切换设置类别: {_currentCategory} -> {category}");
            _currentCategory = category;
            
            // 播放切换音效
            PlayCategorySwitchSFX();
            
            // 更新UI显示
            await UpdateCategoryUI();
        }
        
        /// <summary>
        /// 更新类别UI
        /// </summary>
        private async UniTask UpdateCategoryUI()
        {
            // 更新UI以显示新类别的设置项
            await UniTask.Delay(200); // 模拟UI更新时间
        }
        
        /// <summary>
        /// 播放类别切换音效
        /// </summary>
        private void PlayCategorySwitchSFX()
        {
            // 播放切换音效
            // var switchSound = ResourceService.LoadAsset<AudioClip>("CategorySwitch");
            // _audioManager.PlaySFX(switchSound);
        }
        
        /// <summary>
        /// 更改设置值
        /// </summary>
        public void ChangeSettingValue(string settingName, object value)
        {
            _logService.Info($"更改设置: {settingName} = {value}");
            
            // 根据设置名称更新对应的值
            switch (settingName)
            {
                case "MasterVolume":
                    _currentSettings.MasterVolume = (float)value;
                    _audioManager.MasterVolume = (float)value;
                    break;
                    
                case "SFXVolume":
                    _currentSettings.SFXVolume = (float)value;
                    _audioManager.SFXVolume = (float)value;
                    break;
                    
                case "MusicVolume":
                    _currentSettings.MusicVolume = (float)value;
                    _audioManager.MusicVolume = (float)value;
                    break;
                    
                case "SFXMuted":
                    _currentSettings.SFXMuted = (bool)value;
                    _audioManager.SetSFXMuted((bool)value);
                    break;
                    
                case "MusicMuted":
                    _currentSettings.MusicMuted = (bool)value;
                    _audioManager.SetMusicMuted((bool)value);
                    break;
                    
                case "QualityLevel":
                    _currentSettings.QualityLevel = (int)value;
                    QualitySettings.SetQualityLevel((int)value);
                    break;
                    
                case "FullScreen":
                    _currentSettings.FullScreen = (bool)value;
                    Screen.fullScreen = (bool)value;
                    break;
                    
                case "VSync":
                    _currentSettings.VSync = (bool)value;
                    QualitySettings.vSyncCount = (bool)value ? 1 : 0;
                    break;
                    
                case "MouseSensitivity":
                    _currentSettings.MouseSensitivity = (float)value;
                    break;
                    
                case "InvertMouseY":
                    _currentSettings.InvertMouseY = (bool)value;
                    break;
                    
                case "AutoSave":
                    _currentSettings.AutoSave = (bool)value;
                    break;
                    
                case "ShowHints":
                    _currentSettings.ShowHints = (bool)value;
                    break;
                    
                case "Language":
                    _currentSettings.Language = (string)value;
                    break;
                    
                case "ShowFPS":
                    _currentSettings.ShowFPS = (bool)value;
                    break;
            }
            
            _hasUnsavedChanges = true;
        }
        
        /// <summary>
        /// 保存设置
        /// </summary>
        public async UniTask SaveSettings()
        {
            _logService.Info("保存设置...");
            
            if (!_hasUnsavedChanges)
            {
                _logService.Info("没有未保存的更改");
                return;
            }
            
            try
            {
                // 保存到PlayerPrefs
                PlayerPrefs.SetFloat("MasterVolume", _currentSettings.MasterVolume);
                PlayerPrefs.SetFloat("SFXVolume", _currentSettings.SFXVolume);
                PlayerPrefs.SetFloat("MusicVolume", _currentSettings.MusicVolume);
                PlayerPrefs.SetInt("SFXMuted", _currentSettings.SFXMuted ? 1 : 0);
                PlayerPrefs.SetInt("MusicMuted", _currentSettings.MusicMuted ? 1 : 0);
                
                PlayerPrefs.SetInt("QualityLevel", _currentSettings.QualityLevel);
                PlayerPrefs.SetInt("FullScreen", _currentSettings.FullScreen ? 1 : 0);
                PlayerPrefs.SetInt("VSync", _currentSettings.VSync ? 1 : 0);
                
                PlayerPrefs.SetFloat("MouseSensitivity", _currentSettings.MouseSensitivity);
                PlayerPrefs.SetInt("InvertMouseY", _currentSettings.InvertMouseY ? 1 : 0);
                
                PlayerPrefs.SetInt("AutoSave", _currentSettings.AutoSave ? 1 : 0);
                PlayerPrefs.SetInt("ShowHints", _currentSettings.ShowHints ? 1 : 0);
                
                PlayerPrefs.SetString("Language", _currentSettings.Language);
                PlayerPrefs.SetInt("ShowFPS", _currentSettings.ShowFPS ? 1 : 0);
                
                PlayerPrefs.Save();
                
                // 更新原始设置备份
                _originalSettings = _currentSettings.Clone();
                _hasUnsavedChanges = false;
                
                await UniTask.Delay(500); // 模拟保存时间
                
                // 播放保存成功音效
                PlaySaveSuccessSFX();
                
                // 显示保存成功提示
                await ShowSaveSuccessMessage();
                
                _logService.Info("✓ 设置保存成功");
            }
            catch (System.Exception ex)
            {
                _logService.Error($"设置保存失败: {ex.Message}");
                await ShowSaveErrorMessage(ex.Message);
            }
        }
        
        /// <summary>
        /// 重置设置到默认值
        /// </summary>
        public async UniTask ResetToDefaults()
        {
            _logService.Info("重置设置到默认值");
            
            // 显示确认对话框
            bool confirmed = await ShowResetConfirmDialog();
            
            if (confirmed)
            {
                // 重置所有设置到默认值
                _currentSettings = SettingsData.CreateDefault();
                
                // 应用默认设置
                ApplyCurrentSettings();
                
                _hasUnsavedChanges = true;
                
                // 更新UI显示
                await UpdateSettingsUI();
                
                _logService.Info("设置已重置到默认值");
            }
        }
        
        /// <summary>
        /// 应用当前设置
        /// </summary>
        private void ApplyCurrentSettings()
        {
            // 应用音频设置
            _audioManager.MasterVolume = _currentSettings.MasterVolume;
            _audioManager.SFXVolume = _currentSettings.SFXVolume;
            _audioManager.MusicVolume = _currentSettings.MusicVolume;
            _audioManager.SetSFXMuted(_currentSettings.SFXMuted);
            _audioManager.SetMusicMuted(_currentSettings.MusicMuted);
            
            // 应用图形设置
            QualitySettings.SetQualityLevel(_currentSettings.QualityLevel);
            Screen.fullScreen = _currentSettings.FullScreen;
            QualitySettings.vSyncCount = _currentSettings.VSync ? 1 : 0;
        }
        
        /// <summary>
        /// 取消更改
        /// </summary>
        public async UniTask CancelChanges()
        {
            _logService.Info("取消设置更改");
            
            if (_hasUnsavedChanges)
            {
                // 显示确认对话框
                bool confirmed = await ShowCancelConfirmDialog();
                
                if (confirmed)
                {
                    // 恢复到原始设置
                    _currentSettings = _originalSettings.Clone();
                    ApplyCurrentSettings();
                    _hasUnsavedChanges = false;
                    
                    // 更新UI显示
                    await UpdateSettingsUI();
                    
                    _logService.Info("设置更改已取消");
                }
            }
            
            // 关闭设置菜单
            await _subFlowManager.PopSubFlow();
        }
        
        /// <summary>
        /// 更新设置UI显示
        /// </summary>
        private async UniTask UpdateSettingsUI()
        {
            // 更新UI以反映当前设置值
            await UniTask.Delay(300);
        }
        
        /// <summary>
        /// 显示保存成功消息
        /// </summary>
        private async UniTask ShowSaveSuccessMessage()
        {
            // 显示"设置已保存"消息
            await UniTask.Delay(1000);
        }
        
        /// <summary>
        /// 显示保存错误消息
        /// </summary>
        private async UniTask ShowSaveErrorMessage(string error)
        {
            // 显示保存错误消息
            await UniTask.Delay(2000);
        }
        
        /// <summary>
        /// 显示重置确认对话框
        /// </summary>
        private async UniTask<bool> ShowResetConfirmDialog()
        {
            // 显示"确定要重置所有设置吗？"对话框
            await UniTask.Delay(1500);
            return false; // 模拟用户取消
        }
        
        /// <summary>
        /// 显示取消确认对话框
        /// </summary>
        private async UniTask<bool> ShowCancelConfirmDialog()
        {
            // 显示"有未保存的更改，确定要取消吗？"对话框
            await UniTask.Delay(1500);
            return true; // 模拟用户确认
        }
        
        /// <summary>
        /// 播放保存成功音效
        /// </summary>
        private void PlaySaveSuccessSFX()
        {
            // 播放保存成功音效
            // var saveSound = ResourceService.LoadAsset<AudioClip>("SaveSuccess");
            // _audioManager.PlaySFX(saveSound);
        }
        
        #region 事件处理
        
        /// <summary>
        /// 返回按键按下
        /// </summary>
        private async void OnBackPressed()
        {
            await CancelChanges();
        }
        
        /// <summary>
        /// 确认按键按下
        /// </summary>
        private async void OnConfirmPressed()
        {
            await SaveSettings();
        }
        
        #endregion
        
        /// <summary>
        /// 退出设置菜单
        /// </summary>
        protected override async UniTask OnExitInternal()
        {
            _logService.Info("退出设置菜单");
            
            // 清理输入事件监听
            _inputManager.OnBackPressed -= OnBackPressed;
            _inputManager.OnConfirmPressed -= OnConfirmPressed;
            
            // 如果有未保存的更改，询问是否保存
            if (_hasUnsavedChanges)
            {
                bool saveChanges = await ShowSaveChangesDialog();
                if (saveChanges)
                {
                    await SaveSettings();
                }
                else
                {
                    // 恢复原始设置
                    _currentSettings = _originalSettings.Clone();
                    ApplyCurrentSettings();
                }
            }
            
            // 隐藏设置UI
            await HideSettingsUI();
            
            _logService.Info("设置菜单退出完成");
        }
        
        /// <summary>
        /// 显示保存更改对话框
        /// </summary>
        private async UniTask<bool> ShowSaveChangesDialog()
        {
            // 显示"是否保存更改？"对话框
            await UniTask.Delay(1500);
            return true; // 模拟用户选择保存
        }
        
        /// <summary>
        /// 隐藏设置UI
        /// </summary>
        private async UniTask HideSettingsUI()
        {
            // 隐藏设置界面
            await UniTask.Delay(300);
        }
        
        #region 数据类
        
        /// <summary>
        /// 设置数据类
        /// </summary>
        private class SettingsData
        {
            // 音频设置
            public float MasterVolume { get; set; }
            public float SFXVolume { get; set; }
            public float MusicVolume { get; set; }
            public bool SFXMuted { get; set; }
            public bool MusicMuted { get; set; }
            
            // 图形设置
            public int QualityLevel { get; set; }
            public bool FullScreen { get; set; }
            public Vector2Int Resolution { get; set; }
            public bool VSync { get; set; }
            
            // 控制设置
            public float MouseSensitivity { get; set; }
            public bool InvertMouseY { get; set; }
            
            // 游戏性设置
            public bool AutoSave { get; set; }
            public bool ShowHints { get; set; }
            
            // 通用设置
            public string Language { get; set; }
            public bool ShowFPS { get; set; }
            
            /// <summary>
            /// 克隆设置数据
            /// </summary>
            public SettingsData Clone()
            {
                return new SettingsData
                {
                    MasterVolume = this.MasterVolume,
                    SFXVolume = this.SFXVolume,
                    MusicVolume = this.MusicVolume,
                    SFXMuted = this.SFXMuted,
                    MusicMuted = this.MusicMuted,
                    
                    QualityLevel = this.QualityLevel,
                    FullScreen = this.FullScreen,
                    Resolution = this.Resolution,
                    VSync = this.VSync,
                    
                    MouseSensitivity = this.MouseSensitivity,
                    InvertMouseY = this.InvertMouseY,
                    
                    AutoSave = this.AutoSave,
                    ShowHints = this.ShowHints,
                    
                    Language = this.Language,
                    ShowFPS = this.ShowFPS
                };
            }
            
            /// <summary>
            /// 创建默认设置
            /// </summary>
            public static SettingsData CreateDefault()
            {
                return new SettingsData
                {
                    MasterVolume = 1.0f,
                    SFXVolume = 0.8f,
                    MusicVolume = 0.6f,
                    SFXMuted = false,
                    MusicMuted = false,
                    
                    QualityLevel = 2,
                    FullScreen = true,
                    Resolution = new Vector2Int(1920, 1080),
                    VSync = true,
                    
                    MouseSensitivity = 1.0f,
                    InvertMouseY = false,
                    
                    AutoSave = true,
                    ShowHints = true,
                    
                    Language = "Chinese",
                    ShowFPS = false
                };
            }
        }
        
        #endregion
    }
}