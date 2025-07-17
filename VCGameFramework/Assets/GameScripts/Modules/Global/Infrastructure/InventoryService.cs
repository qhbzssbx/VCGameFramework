using Cysharp.Threading.Tasks;
using Game.Modules.Global.Domain;
using System.Collections.Generic;

namespace Game.Modules.Global.Infrastructure
{
    /// <summary>
    /// 背包数据服务的实现
    /// </summary>
    public class InventoryService : IInventoryService
    {
        private readonly List<string> _items = new();

        /// <inheritdoc />
        public IReadOnlyList<string> Items => _items;

        /// <inheritdoc />
        public UniTask InitializeAsync() => UniTask.CompletedTask;

        /// <inheritdoc />
        public void AddItem(string item) => _items.Add(item);
    }
}
