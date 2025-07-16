namespace Game.Modules.Global.Domain
{
    using Cysharp.Threading.Tasks;
    using System.Collections.Generic;

    public interface IInventoryService
    {
        UniTask InitializeAsync();
        IReadOnlyList<string> Items { get; }
        void AddItem(string item);
    }
}
