using Game.Core;
using Game.HotFix.FlowSystem.Event;
using Game.HotFix.FlowSystem.Interface;
using Game.HotFix.FlowSystem.Manager;
using Game.Infrastructure.Managers;
using MessagePipe;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.HotFix.FlowSystem
{
    /// <summary>
    /// 流程系统模块，负责注册流程系统相关的所有组件
    /// </summary>
    public class FlowSystemModule : IModuleWithOrder
    {
        /// <summary>
        /// 模块加载优先级，数值越小优先级越高
        /// 流程系统需要在其他模块之前加载
        /// </summary>
        public int Order => -100;
        
        /// <summary>
        /// 配置容器注册
        /// </summary>
        /// <param name="builder">容器构建器</param>
        public void Configure(IContainerBuilder builder)
        {
            // 注册核心流程系统组件
            RegisterCoreComponents(builder);
            
            // 注册管理器组件
            RegisterManagers(builder);
            
            // 注册事件系统, MessagePipe与VContainer的合作会创建一个发布器和订阅器的工厂
            // 这个工厂会在使用时产生对应的发布器和订阅器不需要手动注册
            // RegisterEventSystem(builder);
            
            // 注册流程系统初始化器
            RegisterInitializer(builder);
        }
        
        /// <summary>
        /// 注册核心流程系统组件
        /// </summary>
        private void RegisterCoreComponents(IContainerBuilder builder)
        {
            // 注册流程管理器
            builder.Register<IFlowManager, FlowManager>(Lifetime.Singleton);
            builder.Register<ISubFlowManager, SubFlowManager>(Lifetime.Singleton);
            
            // 注册事件发布器
            builder.Register<IFlowEventPublisher, FlowEventPublisher>(Lifetime.Singleton);
            
            // 注册FlowContext为瞬态，每次使用时创建新实例
            builder.Register<FlowContext>(Lifetime.Transient);
        }
        
        /// <summary>
        /// 注册管理器组件
        /// </summary>
        private void RegisterManagers(IContainerBuilder builder)
        {
            // 注册时间管理器
            builder.Register<ITimeManager, TimeManager>(Lifetime.Singleton);
            
            // 注册音频管理器（作为MonoBehaviour组件）
            builder.RegisterComponentOnNewGameObject<AudioManager>(
                Lifetime.Singleton, "AudioManager"
            ).DontDestroyOnLoad().AsImplementedInterfaces();
            
            // 注册输入管理器（作为MonoBehaviour组件）
            builder.RegisterComponentOnNewGameObject<InputManager>(
                Lifetime.Singleton, "InputManager"
            ).DontDestroyOnLoad().AsImplementedInterfaces();
        }
        
        /// <summary>
        /// 注册事件系统
        /// </summary>
        private void RegisterEventSystem(IContainerBuilder builder)
        {
            // 注册MessagePipe的FlowEvent发布器和订阅器
            builder.Register<IPublisher<FlowEvent>>(resolver =>
            {
                // 直接使用GlobalMessagePipe静态方法，无需通过容器解析
                return GlobalMessagePipe.GetPublisher<FlowEvent>();
            }, Lifetime.Singleton);
            
            builder.Register<ISubscriber<FlowEvent>>(resolver =>
            {
                // 直接使用GlobalMessagePipe静态方法，无需通过容器解析
                return GlobalMessagePipe.GetSubscriber<FlowEvent>();
            }, Lifetime.Singleton);
            
            Debug.Log("Event system registered");
        }
        
        /// <summary>
        /// 注册流程系统初始化器
        /// </summary>
        private void RegisterInitializer(IContainerBuilder builder)
        {
            // 注册初始化器作为入口点，确保流程系统在游戏启动时正确初始化
            builder.RegisterEntryPoint<FlowSystemInitializer>(Lifetime.Singleton);
        }
        
        /// <summary>
        /// 创建音频管理器预制体（已弃用，使用RegisterComponentOnNewGameObject代替）
        /// </summary>
        private GameObject CreateAudioManagerPrefab()
        {
            var audioManagerGO = new GameObject("AudioManager");
            
            // 确保音频管理器在场景切换时不被销毁
            Object.DontDestroyOnLoad(audioManagerGO);
            
            // 添加AudioManager组件
            var audioManager = audioManagerGO.AddComponent<AudioManager>();
            
            // 设置默认音量值
            audioManager.MasterVolume = 1.0f;
            audioManager.SFXVolume = 0.8f;
            audioManager.MusicVolume = 0.6f;
            
            return audioManagerGO;
        }
        
        /// <summary>
        /// 创建输入管理器预制体（已弃用，使用RegisterComponentOnNewGameObject代替）
        /// </summary>
        private GameObject CreateInputManagerPrefab()
        {
            var inputManagerGO = new GameObject("InputManager");
            
            // 确保输入管理器在场景切换时不被销毁
            Object.DontDestroyOnLoad(inputManagerGO);
            
            // 添加InputManager组件
            inputManagerGO.AddComponent<InputManager>();
            
            return inputManagerGO;
        }
        
        /// <summary>
        /// 注册具体流程类型（静态方法，用于在其他模块中注册流程）
        /// </summary>
        /// <param name="builder">容器构建器</param>
        public static void RegisterFlow<TFlow>(IContainerBuilder builder) where TFlow : class, IMainFlow
        {
            builder.Register<TFlow>(Lifetime.Singleton);
        }
        
        /// <summary>
        /// 注册子流程类型（静态方法，用于在其他模块中注册子流程）
        /// </summary>
        /// <param name="builder">容器构建器</param>
        public static void RegisterSubFlow<TSubFlow>(IContainerBuilder builder) where TSubFlow : class, ISubFlow
        {
            builder.Register<TSubFlow>(Lifetime.Singleton);
        }
    }
}