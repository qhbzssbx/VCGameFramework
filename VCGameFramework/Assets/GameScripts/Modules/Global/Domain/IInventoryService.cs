namespace Game.Modules.Global.Domain
{
    using Cysharp.Threading.Tasks;
    using System.Collections.Generic;

    /// <summary>
    /// 背包数据服务接口
    /// </summary>
    public interface IInventoryService
    {
        /// <summary>
        /// 初始化背包数据
        /// </summary>
        UniTask InitializeAsync();

        /// <summary>
        /// 当前背包中的物品列表
        /// </summary>
        IReadOnlyList<string> Items { get; }

        /// <summary>
        /// 添加物品
        /// </summary>
        void AddItem(string item);
    }
}
