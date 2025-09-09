using Cysharp.Threading.Tasks;
using Game.Infrastructure.Managers;
using Game.Modules.Global.Domain;
using Game.Modules.Log.Domain;
using Game.HotFix.FlowSystem;
using Game.HotFix.FlowSystem.BaseClass;
using Game.HotFix.FlowSystem.Interface;
using Game.HotFix.FlowSystem.Manager;
using UnityEngine;

namespace Game.Flows.Main
{
    /// <summary>
    /// 登录状态枚举
    /// </summary>
    public enum LoginState
    {
        ShowingLoginUI,     // 显示登录界面
        AutoLogin,          // 自动登录
        ManualLogin,        // 手动登录
        Authenticating,     // 认证中
        SelectingServer,    // 选择服务器
        ConnectingServer,   // 连接服务器
        LoginSuccess,       // 登录成功
        LoginFailed         // 登录失败
    }
    
    /// <summary>
    /// 登录类型枚举
    /// </summary>
    public enum LoginType
    {
        Guest,          // 游客登录
        Account,        // 账号密码登录
        ThirdParty      // 第三方登录（微信、QQ等）
    }
    
    /// <summary>
    /// 登录流程 - 负责用户认证和服务器连接
    /// </summary>
    public class LoginFlow : BaseMainFlow
    {
        private readonly ILogService _logService;
        private readonly IAccountService _accountService;
        private readonly INetworkService _networkService;
        private readonly IInputManager _inputManager;
        private readonly IAudioManager _audioManager;
        private readonly IFlowManager _flowManager;
        
        private LoginState _currentState = LoginState.ShowingLoginUI;
        private LoginType _selectedLoginType = LoginType.Guest;
        private string _username = "";
        private string _password = "";
        private bool _rememberPassword = false;
        private bool _autoLogin = false;
        private string _selectedServer = "";
        private LoginData _loginData;
        
        /// <summary>
        /// 登录流程优先级
        /// </summary>
        public override int Priority => 20;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public LoginFlow(
            ILogService logService,
            IAccountService accountService,
            INetworkService networkService,
            IInputManager inputManager,
            IAudioManager audioManager,
            IFlowManager flowManager)
        {
            _logService = logService;
            _accountService = accountService;
            _networkService = networkService;
            _inputManager = inputManager;
            _audioManager = audioManager;
            _flowManager = flowManager;
        }
        
        /// <summary>
        /// 登录流程通常只能切换到游戏主流程
        /// </summary>
        public override bool CanSwitchTo(System.Type targetFlowType)
        {
            // 登录成功后通常切换到游戏主流程
            return targetFlowType == typeof(GameMainFlow) || base.CanSwitchTo(targetFlowType);
        }
        
        /// <summary>
        /// 进入登录流程
        /// </summary>
        protected override async UniTask OnEnterInternal(FlowContext context)
        {
            _logService.Info("=== 登录流程开始 ===");
            
            try
            {
                // 获取来源信息
                var fromFlow = context?.Get<string>("FromFlow") ?? "Unknown";
                var updateRequired = context?.Get<bool>("UpdateRequired") ?? false;
                _logService.Info($"从 {fromFlow} 流程进入登录，更新状态: {updateRequired}");
                
                // 初始化登录数据
                InitializeLoginData(context);
                
                // 播放登录背景音乐
                await PlayLoginMusic();
                
                // 显示登录界面
                await ShowLoginUI();
                
                // 设置输入处理
                SetupInputHandlers();
                
                // 检查是否需要自动登录
                if (ShouldAutoLogin())
                {
                    await StartAutoLogin();
                }
                else
                {
                    await WaitForUserLogin();
                }
                
                _logService.Info("=== 登录流程处理完成 ===");
            }
            catch (System.Exception ex)
            {
                _logService.Error($"登录流程发生错误: {ex.Message}");
                await HandleLoginError(ex);
                throw;
            }
        }
        
        /// <summary>
        /// 初始化登录数据
        /// </summary>
        private void InitializeLoginData(FlowContext context)
        {
            // 从PlayerPrefs或上下文中读取登录信息
            _username = PlayerPrefs.GetString("SavedUsername", "");
            _rememberPassword = PlayerPrefs.GetInt("RememberPassword", 0) == 1;
            _autoLogin = PlayerPrefs.GetInt("AutoLogin", 0) == 1;
            _selectedServer = PlayerPrefs.GetString("SelectedServer", "");
            
            if (_rememberPassword)
            {
                // 注意：实际项目中密码应该加密存储
                _password = PlayerPrefs.GetString("SavedPassword", "");
            }
            
            // 从上下文获取特殊登录数据
            _loginData = context?.GetTyped<LoginData>() ?? new LoginData();
            
            _logService.Info($"登录数据初始化完成 - 用户名: {_username}, 自动登录: {_autoLogin}");
        }
        
        /// <summary>
        /// 播放登录背景音乐
        /// </summary>
        private async UniTask PlayLoginMusic()
        {
            // 这里可以播放登录界面的背景音乐
            // var loginMusic = await ResourceService.LoadAssetAsync<AudioClip>("LoginBGM");
            // _audioManager.PlayMusic(loginMusic, loop: true, fadeInDuration: 2f);
            
            _logService.Info("播放登录背景音乐");
            await UniTask.Delay(500); // 模拟音乐加载时间
        }
        
        /// <summary>
        /// 显示登录UI界面
        /// </summary>
        private async UniTask ShowLoginUI()
        {
            _logService.Info("显示登录界面");
            _currentState = LoginState.ShowingLoginUI;
            
            // 这里应该显示登录UI界面
            // 包括用户名输入框、密码输入框、登录按钮等
            
            await UniTask.Delay(800); // 模拟UI显示时间
        }
        
        /// <summary>
        /// 设置输入处理
        /// </summary>
        private void SetupInputHandlers()
        {
            // 在登录界面，主要是UI输入
            _inputManager.SetUIOnlyMode();
            
            // 监听特殊按键
            _inputManager.OnConfirmPressed += OnLoginConfirmed;
            _inputManager.OnBackPressed += OnBackPressed;
        }
        
        /// <summary>
        /// 检查是否应该自动登录
        /// </summary>
        private bool ShouldAutoLogin()
        {
            // 满足以下条件才自动登录：
            // 1. 启用了自动登录
            // 2. 有保存的用户名
            // 3. 有保存的密码（如果需要）
            // 4. 不是调试模式下的特殊情况
            
            bool canAutoLogin = _autoLogin && 
                               !string.IsNullOrEmpty(_username) && 
                               (_selectedLoginType == LoginType.Guest || !string.IsNullOrEmpty(_password));
            
            _logService.Info($"自动登录检查: {canAutoLogin}");
            return canAutoLogin;
        }
        
        /// <summary>
        /// 开始自动登录
        /// </summary>
        private async UniTask StartAutoLogin()
        {
            _logService.Info("开始自动登录...");
            _currentState = LoginState.AutoLogin;
            
            // 显示自动登录提示
            await ShowAutoLoginTip();
            
            // 执行登录
            await PerformLogin();
        }
        
        /// <summary>
        /// 显示自动登录提示
        /// </summary>
        private async UniTask ShowAutoLoginTip()
        {
            // 显示"正在自动登录..."的提示
            await UniTask.Delay(1000);
        }
        
        /// <summary>
        /// 等待用户手动登录
        /// </summary>
        private async UniTask WaitForUserLogin()
        {
            _logService.Info("等待用户手动登录...");
            _currentState = LoginState.ManualLogin;
            
            // 等待用户填写登录信息并点击登录按钮
            // 这个方法会在用户点击登录按钮时通过事件触发结束
            while (_currentState == LoginState.ManualLogin)
            {
                await UniTask.Delay(100);
            }
        }
        
        /// <summary>
        /// 执行登录操作
        /// </summary>
        private async UniTask PerformLogin()
        {
            _logService.Info($"开始执行登录 - 类型: {_selectedLoginType}, 用户: {_username}");
            _currentState = LoginState.Authenticating;
            
            try
            {
                // 根据登录类型执行不同的登录逻辑
                bool loginSuccess = false;
                
                switch (_selectedLoginType)
                {
                    case LoginType.Guest:
                        loginSuccess = await PerformGuestLogin();
                        break;
                        
                    case LoginType.Account:
                        loginSuccess = await PerformAccountLogin();
                        break;
                        
                    case LoginType.ThirdParty:
                        loginSuccess = await PerformThirdPartyLogin();
                        break;
                }
                
                if (loginSuccess)
                {
                    // 选择服务器
                    await SelectServer();
                    
                    // 连接服务器
                    await ConnectToServer();
                    
                    // 登录成功
                    await OnLoginSuccess();
                }
                else
                {
                    await OnLoginFailed("登录认证失败");
                }
            }
            catch (System.Exception ex)
            {
                await OnLoginFailed($"登录过程出错: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 执行游客登录
        /// </summary>
        private async UniTask<bool> PerformGuestLogin()
        {
            _logService.Info("执行游客登录...");
            
            // 生成游客ID
            string guestId = GenerateGuestId();
            _username = $"Guest_{guestId}";
            
            // 模拟登录验证
            await UniTask.Delay(1500);
            
            _logService.Info($"游客登录成功: {_username}");
            return true;
        }
        
        /// <summary>
        /// 执行账号登录
        /// </summary>
        private async UniTask<bool> PerformAccountLogin()
        {
            _logService.Info("执行账号登录...");
            
            if (string.IsNullOrEmpty(_username) || string.IsNullOrEmpty(_password))
            {
                _logService.Warning("用户名或密码为空");
                return false;
            }
            
            // 调用账号服务进行验证
            var loginSuccess = await _accountService.LoginAsync(_username, _password);
            
            if (loginSuccess)
            {
                _logService.Info($"账号登录成功: {_username}");
                
                // 保存登录信息
                if (_rememberPassword)
                {
                    SaveLoginInfo();
                }
                
                return true;
            }
            else
            {
                _logService.Warning("账号登录失败");
                return false;
            }
        }
        
        /// <summary>
        /// 执行第三方登录
        /// </summary>
        private async UniTask<bool> PerformThirdPartyLogin()
        {
            _logService.Info("执行第三方登录...");
            
            // 这里应该调用第三方SDK进行登录
            // 比如微信、QQ、Steam等
            
            await UniTask.Delay(2000); // 模拟第三方登录时间
            
            _logService.Info("第三方登录成功");
            return true;
        }
        
        /// <summary>
        /// 选择服务器
        /// </summary>
        private async UniTask SelectServer()
        {
            _logService.Info("选择服务器...");
            _currentState = LoginState.SelectingServer;
            
            if (!string.IsNullOrEmpty(_selectedServer))
            {
                _logService.Info($"使用已保存的服务器: {_selectedServer}");
                return;
            }
            
            // 获取服务器列表
            var serverList = await GetServerList();
            
            if (serverList != null && serverList.Length > 0)
            {
                // 选择推荐服务器或让用户选择
                _selectedServer = serverList[0].Name;
                _logService.Info($"自动选择服务器: {_selectedServer}");
                
                // 保存服务器选择
                PlayerPrefs.SetString("SelectedServer", _selectedServer);
            }
            else
            {
                throw new System.Exception("无法获取服务器列表");
            }
        }
        
        /// <summary>
        /// 获取服务器列表
        /// </summary>
        private async UniTask<ServerInfo[]> GetServerList()
        {
            // 模拟从服务器获取服务器列表
            await UniTask.Delay(1000);
            
            return new ServerInfo[]
            {
                new ServerInfo { Name = "推荐服务器", Address = "game1.example.com", Port = 8080, Status = "正常" },
                new ServerInfo { Name = "备用服务器", Address = "game2.example.com", Port = 8080, Status = "正常" }
            };
        }
        
        /// <summary>
        /// 连接服务器
        /// </summary>
        private async UniTask ConnectToServer()
        {
            _logService.Info($"连接服务器: {_selectedServer}");
            _currentState = LoginState.ConnectingServer;
            
            try
            {
                // 这里应该调用网络服务连接游戏服务器
                // await _networkService.ConnectAsync(serverAddress, port);
                
                // 模拟连接过程
                await UniTask.Delay(2000);
                
                _logService.Info("服务器连接成功");
            }
            catch (System.Exception ex)
            {
                _logService.Error($"服务器连接失败: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// 登录成功处理
        /// </summary>
        private async UniTask OnLoginSuccess()
        {
            _logService.Info("登录成功！");
            _currentState = LoginState.LoginSuccess;
            
            // 播放登录成功音效
            // _audioManager.PlaySFX(loginSuccessSound);
            
            // 显示登录成功提示
            await ShowLoginSuccessMessage();
            
            // 准备进入游戏
            await EnterGame();
        }
        
        /// <summary>
        /// 登录失败处理
        /// </summary>
        private async UniTask OnLoginFailed(string reason)
        {
            _logService.Warning($"登录失败: {reason}");
            _currentState = LoginState.LoginFailed;
            
            // 播放登录失败音效
            // _audioManager.PlaySFX(loginFailedSound);
            
            // 显示错误消息
            await ShowLoginErrorMessage(reason);
            
            // 返回到手动登录状态
            _currentState = LoginState.ManualLogin;
        }
        
        /// <summary>
        /// 显示登录成功消息
        /// </summary>
        private async UniTask ShowLoginSuccessMessage()
        {
            // 显示"登录成功，正在进入游戏..."
            await UniTask.Delay(1500);
        }
        
        /// <summary>
        /// 显示登录错误消息
        /// </summary>
        private async UniTask ShowLoginErrorMessage(string message)
        {
            // 显示错误对话框
            await UniTask.Delay(2000);
        }
        
        /// <summary>
        /// 进入游戏
        /// </summary>
        private async UniTask EnterGame()
        {
            _logService.Info("准备进入游戏主流程...");
            
            // 创建游戏数据上下文
            var gameContext = FlowContextBuilder.Create()
                .WithData("FromFlow", "Login")
                .WithData("Username", _username)
                .WithData("LoginType", _selectedLoginType.ToString())
                .WithData("ServerName", _selectedServer)
                .WithTypedData(new PlayerLoginInfo 
                { 
                    Username = _username,
                    LoginType = _selectedLoginType,
                    ServerName = _selectedServer,
                    LoginTime = System.DateTime.Now
                })
                .WithTypedData(_flowManager)
                .Build();
            
            // 延迟一下让用户看到成功提示
            await UniTask.Delay(1000);
            
            // 切换到游戏主流程
            await _flowManager.SwitchToFlow<GameMainFlow>(gameContext);
        }
        
        /// <summary>
        /// 处理登录错误
        /// </summary>
        private async UniTask HandleLoginError(System.Exception ex)
        {
            _logService.Error($"登录流程发生严重错误: {ex}");
            
            // 显示错误对话框，允许用户重试或退出
            await ShowCriticalErrorDialog(ex.Message);
        }
        
        /// <summary>
        /// 显示严重错误对话框
        /// </summary>
        private async UniTask ShowCriticalErrorDialog(string message)
        {
            // 显示错误对话框
            await UniTask.Delay(3000);
            
            // 返回到正常登录状态
            _currentState = LoginState.ManualLogin;
        }
        
        #region 辅助方法
        
        /// <summary>
        /// 生成游客ID
        /// </summary>
        private string GenerateGuestId()
        {
            return System.DateTime.Now.Ticks.ToString().Substring(8);
        }
        
        /// <summary>
        /// 保存登录信息
        /// </summary>
        private void SaveLoginInfo()
        {
            PlayerPrefs.SetString("SavedUsername", _username);
            PlayerPrefs.SetInt("RememberPassword", _rememberPassword ? 1 : 0);
            PlayerPrefs.SetInt("AutoLogin", _autoLogin ? 1 : 0);
            
            if (_rememberPassword)
            {
                // 注意：实际项目中应该加密存储密码
                PlayerPrefs.SetString("SavedPassword", _password);
            }
            
            PlayerPrefs.Save();
        }
        
        #endregion
        
        #region 事件处理
        
        /// <summary>
        /// 用户确认登录
        /// </summary>
        private void OnLoginConfirmed()
        {
            if (_currentState == LoginState.ManualLogin)
            {
                _logService.Info("用户确认登录");
                PerformLogin().Forget();
            }
        }
        
        /// <summary>
        /// 用户按下返回键
        /// </summary>
        private void OnBackPressed()
        {
            _logService.Info("用户按下返回键");
            
            switch (_currentState)
            {
                case LoginState.ManualLogin:
                case LoginState.ShowingLoginUI:
                    // 退出游戏或返回上一个流程
                    ExitGame();
                    break;
                    
                case LoginState.Authenticating:
                    // 取消登录
                    _currentState = LoginState.ManualLogin;
                    break;
            }
        }
        
        /// <summary>
        /// 退出游戏
        /// </summary>
        private void ExitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        
        #endregion
        
        /// <summary>
        /// 退出登录流程
        /// </summary>
        protected override async UniTask OnExitInternal()
        {
            _logService.Info("退出登录流程");
            
            // 清理事件监听
            _inputManager.OnConfirmPressed -= OnLoginConfirmed;
            _inputManager.OnBackPressed -= OnBackPressed;
            
            // 停止登录背景音乐
            _audioManager.StopMusic();
            
            // 恢复正常输入模式
            _inputManager.EnableInput();
            
            // 隐藏登录UI
            await HideLoginUI();
        }
        
        /// <summary>
        /// 隐藏登录UI
        /// </summary>
        private async UniTask HideLoginUI()
        {
            // 隐藏登录界面
            await UniTask.Delay(300);
        }
        
        #region 数据类
        
        /// <summary>
        /// 登录数据
        /// </summary>
        public class LoginData
        {
            public bool ForceManualLogin { get; set; } = false;
            public string PreselectedServer { get; set; } = "";
            public LoginType DefaultLoginType { get; set; } = LoginType.Guest;
        }
        
        /// <summary>
        /// 服务器信息
        /// </summary>
        private class ServerInfo
        {
            public string Name { get; set; }
            public string Address { get; set; }
            public int Port { get; set; }
            public string Status { get; set; }
        }
        
        /// <summary>
        /// 玩家登录信息
        /// </summary>
        public class PlayerLoginInfo
        {
            public string Username { get; set; }
            public LoginType LoginType { get; set; }
            public string ServerName { get; set; }
            public System.DateTime LoginTime { get; set; }
        }
        
        #endregion
    }
}