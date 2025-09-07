using System.Collections.Generic;

namespace Game.HotFix.AssetSystem.Core
{
    /// <summary>
    /// 地址映射器接口：支持根据设备、画质、地区等条件进行地址变体选择
    /// </summary>
    public interface IAddressMapper
    {
        /// <summary>
        /// 根据原始地址和上下文信息，返回映射后的地址
        /// </summary>
        /// <param name="package">包名</param>
        /// <param name="originalAddress">原始地址</param>
        /// <param name="context">映射上下文</param>
        /// <returns>映射后的地址</returns>
        string MapAddress(string package, string originalAddress, AddressMappingContext? context = null);

        /// <summary>
        /// 批量地址映射
        /// </summary>
        /// <param name="package">包名</param>
        /// <param name="addresses">原始地址列表</param>
        /// <param name="context">映射上下文</param>
        /// <returns>映射后的地址列表</returns>
        IReadOnlyList<string> MapAddresses(string package, IReadOnlyList<string> addresses, 
            AddressMappingContext? context = null);

        /// <summary>
        /// 注册地址映射规则
        /// </summary>
        /// <param name="rule">映射规则</param>
        void RegisterMappingRule(IAddressMappingRule rule);

        /// <summary>
        /// 移除地址映射规则
        /// </summary>
        /// <param name="rule">映射规则</param>
        void UnregisterMappingRule(IAddressMappingRule rule);

        /// <summary>
        /// 清除所有映射规则
        /// </summary>
        void ClearMappingRules();
    }

    /// <summary>
    /// 地址映射上下文
    /// </summary>
    public sealed class AddressMappingContext
    {
        /// <summary>
        /// 画质级别 (Low, Medium, High, Ultra)
        /// </summary>
        public string QualityLevel { get; set; } = "Medium";

        /// <summary>
        /// 设备类型 (Mobile, PC, Console)
        /// </summary>
        public string DeviceType { get; set; } = "Mobile";

        /// <summary>
        /// 地区代码 (CN, EN, JP, etc.)
        /// </summary>
        public string RegionCode { get; set; } = "CN";

        /// <summary>
        /// 平台标识 (Android, iOS, Windows, etc.)
        /// </summary>
        public string Platform { get; set; } = "Android";

        /// <summary>
        /// 网络状态 (WiFi, Cellular, Offline)
        /// </summary>
        public string NetworkType { get; set; } = "WiFi";

        /// <summary>
        /// 自定义属性
        /// </summary>
        public Dictionary<string, string> CustomProperties { get; set; } = new();

        /// <summary>
        /// 创建默认上下文
        /// </summary>
        /// <returns>默认配置的地址映射上下文</returns>
        public static AddressMappingContext CreateDefault()
        {
            return new AddressMappingContext
            {
                QualityLevel = UnityEngine.QualitySettings.names[UnityEngine.QualitySettings.GetQualityLevel()],
                DeviceType = UnityEngine.SystemInfo.deviceType == UnityEngine.DeviceType.Handheld ? "Mobile" : "PC",
                Platform = UnityEngine.Application.platform.ToString(),
                RegionCode = System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName.ToUpper()
            };
        }
    }

    /// <summary>
    /// 地址映射规则接口
    /// </summary>
    public interface IAddressMappingRule
    {
        /// <summary>
        /// 规则优先级，数值越大优先级越高
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// 检查规则是否适用于指定的地址和上下文
        /// </summary>
        /// <param name="package">包名</param>
        /// <param name="address">地址</param>
        /// <param name="context">上下文</param>
        /// <returns>是否适用</returns>
        bool IsApplicable(string package, string address, AddressMappingContext context);

        /// <summary>
        /// 应用映射规则
        /// </summary>
        /// <param name="package">包名</param>
        /// <param name="address">原始地址</param>
        /// <param name="context">上下文</param>
        /// <returns>映射后的地址</returns>
        string ApplyMapping(string package, string address, AddressMappingContext context);
    }
}