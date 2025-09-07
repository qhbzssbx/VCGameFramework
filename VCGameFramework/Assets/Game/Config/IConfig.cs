namespace Game.Config
{
    /// <summary>
    /// 配置接口基类
    /// 所有配置类都应实现此接口以便统一管理
    /// </summary>
    public interface IConfig
    {
        /// <summary>
        /// 配置名称
        /// </summary>
        string ConfigName { get; }
        
        /// <summary>
        /// 配置版本
        /// </summary>
        string Version { get; }
        
        /// <summary>
        /// 验证配置有效性
        /// </summary>
        /// <returns>配置是否有效</returns>
        bool Validate();
        
        /// <summary>
        /// 重置为默认配置
        /// </summary>
        void ResetToDefault();
    }
    
    /// <summary>
    /// 可序列化的配置接口
    /// </summary>
    public interface ISerializableConfig : IConfig
    {
        /// <summary>
        /// 序列化配置到字符串
        /// </summary>
        string Serialize();
        
        /// <summary>
        /// 从字符串反序列化配置
        /// </summary>
        /// <param name="data">序列化数据</param>
        void Deserialize(string data);
    }
    
    /// <summary>
    /// 可持久化的配置接口
    /// </summary>
    public interface IPersistentConfig : ISerializableConfig
    {
        /// <summary>
        /// 配置文件路径
        /// </summary>
        string FilePath { get; }
        
        /// <summary>
        /// 保存配置到文件
        /// </summary>
        void Save();
        
        /// <summary>
        /// 从文件加载配置
        /// </summary>
        void Load();
    }
}