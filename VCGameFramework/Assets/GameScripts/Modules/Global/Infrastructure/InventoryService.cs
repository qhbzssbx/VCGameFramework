using Cysharp.Threading.Tasks;
using Game.Modules.Global.Domain;
using System.Collections.Generic;

namespace Game.Modules.Global.Infrastructure
{
    public class InventoryService : IInventoryService
    {
        private readonly List<string> _items = new();
        public IReadOnlyList<string> Items => _items;

        public UniTask InitializeAsync() => UniTask.CompletedTask;

        public void AddItem(string item) => _items.Add(item);
    }
}
