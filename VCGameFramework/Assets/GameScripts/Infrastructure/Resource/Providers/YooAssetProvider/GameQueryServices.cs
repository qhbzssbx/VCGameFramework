using YooAsset;

namespace Game.Infrastructure.Resource.Providers.YooAssetProvider
{
    /// <summary>
    /// 游戏查询服务，用于YooAsset的内置文件查询
    /// </summary>
    // public class GameQueryServices : IBuildinQueryServices
    public class GameQueryServices
    {
        /// <summary>
        /// 查询内置文件是否存在
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <returns>是否存在</returns>
        public bool Query(string packageName, string fileName)
        {
            // 在编辑器模式下，内置文件查询通常返回false
            // 因为我们使用的是EditorSimulateMode
#if UNITY_EDITOR
            return false;
#else
            // 在运行时，可以根据实际需求实现内置文件检查逻辑
            // 这里简单返回false，表示没有内置文件
            return false;
#endif
        }
    }
}