namespace Game.HotFix.FlowSystem.Manager
{
    /// <summary>
    /// 流程上下文构建器，提供流畅的API来构建FlowContext
    /// </summary>
    public class FlowContextBuilder
    {
        private readonly FlowContext _context;
        
        /// <summary>
        /// 私有构造函数，通过静态方法创建实例
        /// </summary>
        private FlowContextBuilder()
        {
            _context = new FlowContext();
        }
        
        /// <summary>
        /// 从现有上下文创建构建器
        /// </summary>
        /// <param name="existingContext">现有上下文</param>
        private FlowContextBuilder(FlowContext existingContext)
        {
            _context = existingContext ?? new FlowContext();
        }
        
        /// <summary>
        /// 创建新的流程上下文构建器
        /// </summary>
        /// <returns>构建器实例</returns>
        public static FlowContextBuilder Create()
        {
            return new FlowContextBuilder();
        }
        
        /// <summary>
        /// 从现有上下文创建构建器
        /// </summary>
        /// <param name="existingContext">现有上下文</param>
        /// <returns>构建器实例</returns>
        public static FlowContextBuilder From(FlowContext existingContext)
        {
            return new FlowContextBuilder(existingContext);
        }
        
        /// <summary>
        /// 添加字符串键值对数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <returns>构建器实例，支持链式调用</returns>
        public FlowContextBuilder WithData<T>(string key, T value)
        {
            _context.Set(key, value);
            return this;
        }
        
        /// <summary>
        /// 添加类型化数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="value">值</param>
        /// <returns>构建器实例，支持链式调用</returns>
        public FlowContextBuilder WithTypedData<T>(T value)
        {
            _context.SetTyped(value);
            return this;
        }
        
        /// <summary>
        /// 构建最终的FlowContext实例
        /// </summary>
        /// <returns>构建完成的FlowContext</returns>
        public FlowContext Build()
        {
            return _context;
        }
        
        /// <summary>
        /// 隐式转换操作符，允许直接将构建器转换为FlowContext
        /// </summary>
        /// <param name="builder">构建器实例</param>
        public static implicit operator FlowContext(FlowContextBuilder builder)
        {
            return builder.Build();
        }
    }
}