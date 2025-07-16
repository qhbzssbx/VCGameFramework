using Game.Modules.Global.Domain;
using System;
using VContainer.Unity;

namespace Game.UI.Inventory
{
    public class InventoryPresenter : IStartable, IDisposable
    {
        private readonly IInventoryService _service;
        private readonly InventoryView _view;

        public InventoryPresenter(IInventoryService service, InventoryView view)
        {
            _service = service;
            _view = view;
        }

        public void Start()
        {
            UnityEngine.Debug.Log($"Inventory items: {_service.Items.Count}");
        }

        public void Dispose()
        {
        }
    }
}
