using Cysharp.Threading.Tasks;
using Game.Infrastructure.Managers;
using Game.Core.FlowSystem;
using Game.Modules.Global.Domain;
using Game.Modules.Log.Domain;
using System.Collections.Generic;
using System.Linq;

namespace Game.Flows.Sub
{
    /// <summary>
    /// 背包标签页枚举
    /// </summary>
    public enum InventoryTab
    {
        All,        // 全部
        Equipment,  // 装备
        Consumable, // 消耗品
        Material,   // 材料
        Quest,      // 任务物品
        Other       // 其他
    }
    
    /// <summary>
    /// 物品类型枚举
    /// </summary>
    public enum ItemType
    {
        Equipment,
        Consumable,
        Material,
        Quest,
        Other
    }
    
    /// <summary>
    /// 背包界面子流程 - 物品管理界面
    /// </summary>
    public class InventorySubFlow : BaseSubFlow
    {
        private readonly ILogService _logService;
        private readonly IInputManager _inputManager;
        private readonly IAudioManager _audioManager;
        private readonly IInventoryService _inventoryService;
        private readonly ISubFlowManager _subFlowManager;
        
        private InventoryTab _currentTab = InventoryTab.All;
        private List<InventoryItem> _allItems = new();
        private List<InventoryItem> _filteredItems = new();
        private int _selectedItemIndex = 0;
        private bool _isInventoryLoaded = false;
        
        /// <summary>
        /// 背包界面不需要暂停父流程（玩家可以边游戏边管理物品）
        /// </summary>
        public override bool ShouldPauseParent => false;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public InventorySubFlow(
            ILogService logService,
            IInputManager inputManager,
            IAudioManager audioManager,
            IInventoryService inventoryService,
            ISubFlowManager subFlowManager)
        {
            _logService = logService;
            _inputManager = inputManager;
            _audioManager = audioManager;
            _inventoryService = inventoryService;
            _subFlowManager = subFlowManager;
        }
        
        /// <summary>
        /// 进入背包界面
        /// </summary>
        protected override async UniTask OnEnterInternal(FlowContext context)
        {
            _logService.Info("进入背包界面");
            
            // 获取来源信息和初始标签页
            var fromFlow = context?.Get<string>("FromFlow") ?? "Unknown";
            var initialTab = context?.Get<string>("InitialTab");
            
            _logService.Info($"从 {fromFlow} 进入背包界面");
            
            // 设置初始标签页
            if (!string.IsNullOrEmpty(initialTab) && 
                System.Enum.TryParse<InventoryTab>(initialTab, out var tab))
            {
                _currentTab = tab;
            }
            
            // 设置UI输入模式
            _inputManager.SetUIOnlyMode();
            
            // 加载背包数据
            await LoadInventoryData();
            
            // 显示背包UI
            await ShowInventoryUI();
            
            // 设置输入处理
            SetupInventoryInput();
            
            // 播放背包打开音效
            PlayInventoryOpenSFX();
            
            _logService.Info("背包界面显示完成");
        }
        
        /// <summary>
        /// 加载背包数据
        /// </summary>
        private async UniTask LoadInventoryData()
        {
            _logService.Info("加载背包数据...");
            
            try
            {
                // 从背包服务获取物品数据
                _allItems = await LoadPlayerInventory();
                
                // 根据当前标签页过滤物品
                FilterItemsByTab(_currentTab);
                
                _isInventoryLoaded = true;
                _logService.Info($"✓ 背包数据加载完成，共 {_allItems.Count} 个物品");
            }
            catch (System.Exception ex)
            {
                _logService.Error($"背包数据加载失败: {ex.Message}");
                _allItems = new List<InventoryItem>();
                _filteredItems = new List<InventoryItem>();
            }
        }
        
        /// <summary>
        /// 加载玩家背包
        /// </summary>
        private async UniTask<List<InventoryItem>> LoadPlayerInventory()
        {
            // 模拟从服务器或本地加载背包数据
            await UniTask.Delay(800);
            
            // 创建一些示例物品
            return new List<InventoryItem>
            {
                new InventoryItem { Id = 1, Name = "铁剑", Type = ItemType.Equipment, Count = 1, Description = "普通的铁制剑", Rarity = ItemRarity.Common },
                new InventoryItem { Id = 2, Name = "生命药水", Type = ItemType.Consumable, Count = 5, Description = "恢复50点生命值", Rarity = ItemRarity.Common },
                new InventoryItem { Id = 3, Name = "铁矿石", Type = ItemType.Material, Count = 10, Description = "制作装备的材料", Rarity = ItemRarity.Common },
                new InventoryItem { Id = 4, Name = "神秘钥匙", Type = ItemType.Quest, Count = 1, Description = "神秘房间的钥匙", Rarity = ItemRarity.Rare },
                new InventoryItem { Id = 5, Name = "钢甲", Type = ItemType.Equipment, Count = 1, Description = "坚固的钢制盔甲", Rarity = ItemRarity.Uncommon },
                new InventoryItem { Id = 6, Name = "魔法药水", Type = ItemType.Consumable, Count = 3, Description = "恢复30点魔法值", Rarity = ItemRarity.Uncommon },
            };
        }
        
        /// <summary>
        /// 显示背包UI
        /// </summary>
        private async UniTask ShowInventoryUI()
        {
            _logService.Info($"显示背包UI - 当前标签: {_currentTab}, 物品数量: {_filteredItems.Count}");
            
            // 这里应该显示背包界面的UI
            // 包括标签页、物品格子、物品详情等
            
            await UniTask.Delay(600); // 模拟UI显示时间
            _logService.Info("✓ 背包UI显示完成");
        }
        
        /// <summary>
        /// 设置背包输入处理
        /// </summary>
        private void SetupInventoryInput()
        {
            _inputManager.OnBackPressed += OnCloseInventory;
            _inputManager.OnConfirmPressed += OnUseItem;
            _inputManager.OnMenuPressed += OnShowItemDetail;
        }
        
        /// <summary>
        /// 根据标签页过滤物品
        /// </summary>
        private void FilterItemsByTab(InventoryTab tab)
        {
            _logService.Info($"过滤物品 - 标签: {tab}");
            
            switch (tab)
            {
                case InventoryTab.All:
                    _filteredItems = new List<InventoryItem>(_allItems);
                    break;
                    
                case InventoryTab.Equipment:
                    _filteredItems = _allItems.Where(item => item.Type == ItemType.Equipment).ToList();
                    break;
                    
                case InventoryTab.Consumable:
                    _filteredItems = _allItems.Where(item => item.Type == ItemType.Consumable).ToList();
                    break;
                    
                case InventoryTab.Material:
                    _filteredItems = _allItems.Where(item => item.Type == ItemType.Material).ToList();
                    break;
                    
                case InventoryTab.Quest:
                    _filteredItems = _allItems.Where(item => item.Type == ItemType.Quest).ToList();
                    break;
                    
                case InventoryTab.Other:
                    _filteredItems = _allItems.Where(item => item.Type == ItemType.Other).ToList();
                    break;
            }
            
            // 重置选中索引
            _selectedItemIndex = 0;
            
            _logService.Info($"过滤完成，显示 {_filteredItems.Count} 个物品");
        }
        
        /// <summary>
        /// 切换标签页
        /// </summary>
        public async UniTask SwitchTab(InventoryTab tab)
        {
            if (_currentTab == tab) return;
            
            _logService.Info($"切换标签页: {_currentTab} -> {tab}");
            _currentTab = tab;
            
            // 播放标签切换音效
            PlayTabSwitchSFX();
            
            // 过滤物品
            FilterItemsByTab(tab);
            
            // 更新UI显示
            await UpdateTabUI();
        }
        
        /// <summary>
        /// 更新标签页UI
        /// </summary>
        private async UniTask UpdateTabUI()
        {
            // 更新UI以显示新标签页的物品
            await UniTask.Delay(300);
        }
        
        /// <summary>
        /// 使用物品
        /// </summary>
        public async UniTask UseItem(int itemIndex)
        {
            if (itemIndex < 0 || itemIndex >= _filteredItems.Count)
            {
                _logService.Warning($"无效的物品索引: {itemIndex}");
                return;
            }
            
            var item = _filteredItems[itemIndex];
            _logService.Info($"使用物品: {item.Name}");
            
            // 检查物品是否可以使用
            if (!CanUseItem(item))
            {
                _logService.Warning($"物品 {item.Name} 无法使用");
                PlayErrorSFX();
                await ShowCannotUseMessage(item);
                return;
            }
            
            // 显示使用确认对话框
            bool confirmed = await ShowUseItemConfirmDialog(item);
            
            if (confirmed)
            {
                // 执行使用物品的逻辑
                await ExecuteUseItem(item);
                
                // 更新背包显示
                await RefreshInventoryDisplay();
                
                _logService.Info($"物品 {item.Name} 使用成功");
            }
        }
        
        /// <summary>
        /// 检查物品是否可以使用
        /// </summary>
        private bool CanUseItem(InventoryItem item)
        {
            switch (item.Type)
            {
                case ItemType.Consumable:
                    return true; // 消耗品通常可以使用
                    
                case ItemType.Equipment:
                    return true; // 装备可以装备
                    
                case ItemType.Quest:
                    return false; // 任务物品通常不能直接使用
                    
                case ItemType.Material:
                    return false; // 材料不能直接使用
                    
                default:
                    return false;
            }
        }
        
        /// <summary>
        /// 执行使用物品
        /// </summary>
        private async UniTask ExecuteUseItem(InventoryItem item)
        {
            switch (item.Type)
            {
                case ItemType.Consumable:
                    await UseConsumableItem(item);
                    break;
                    
                case ItemType.Equipment:
                    await EquipItem(item);
                    break;
            }
        }
        
        /// <summary>
        /// 使用消耗品
        /// </summary>
        private async UniTask UseConsumableItem(InventoryItem item)
        {
            _logService.Info($"使用消耗品: {item.Name}");
            
            // 播放使用音效
            PlayUseItemSFX();
            
            // 执行物品效果（这里是模拟）
            await UniTask.Delay(500);
            
            // 减少物品数量
            item.Count--;
            
            // 如果数量为0，从背包中移除
            if (item.Count <= 0)
            {
                _allItems.Remove(item);
                FilterItemsByTab(_currentTab);
            }
            
            // 显示使用效果
            await ShowItemEffect(item);
        }
        
        /// <summary>
        /// 装备物品
        /// </summary>
        private async UniTask EquipItem(InventoryItem item)
        {
            _logService.Info($"装备物品: {item.Name}");
            
            // 播放装备音效
            PlayEquipItemSFX();
            
            // 执行装备逻辑
            await UniTask.Delay(300);
            
            // 显示装备成功消息
            await ShowEquipSuccessMessage(item);
        }
        
        /// <summary>
        /// 丢弃物品
        /// </summary>
        public async UniTask DiscardItem(int itemIndex, int count = 1)
        {
            if (itemIndex < 0 || itemIndex >= _filteredItems.Count)
            {
                _logService.Warning($"无效的物品索引: {itemIndex}");
                return;
            }
            
            var item = _filteredItems[itemIndex];
            _logService.Info($"丢弃物品: {item.Name} x{count}");
            
            // 显示丢弃确认对话框
            bool confirmed = await ShowDiscardConfirmDialog(item, count);
            
            if (confirmed)
            {
                // 减少物品数量
                item.Count -= count;
                
                // 如果数量为0或负数，从背包中移除
                if (item.Count <= 0)
                {
                    _allItems.Remove(item);
                    FilterItemsByTab(_currentTab);
                }
                
                // 播放丢弃音效
                PlayDiscardItemSFX();
                
                // 更新背包显示
                await RefreshInventoryDisplay();
                
                _logService.Info($"物品 {item.Name} 丢弃完成");
            }
        }
        
        /// <summary>
        /// 刷新背包显示
        /// </summary>
        private async UniTask RefreshInventoryDisplay()
        {
            // 重新过滤和显示物品
            FilterItemsByTab(_currentTab);
            await UpdateInventoryUI();
        }
        
        /// <summary>
        /// 更新背包UI
        /// </summary>
        private async UniTask UpdateInventoryUI()
        {
            // 更新UI显示
            await UniTask.Delay(200);
        }
        
        #region 音效播放
        
        private void PlayInventoryOpenSFX()
        {
            // var openSound = ResourceService.LoadAsset<AudioClip>("InventoryOpen");
            // _audioManager.PlaySFX(openSound);
        }
        
        private void PlayTabSwitchSFX()
        {
            // var switchSound = ResourceService.LoadAsset<AudioClip>("TabSwitch");
            // _audioManager.PlaySFX(switchSound);
        }
        
        private void PlayUseItemSFX()
        {
            // var useSound = ResourceService.LoadAsset<AudioClip>("UseItem");
            // _audioManager.PlaySFX(useSound);
        }
        
        private void PlayEquipItemSFX()
        {
            // var equipSound = ResourceService.LoadAsset<AudioClip>("EquipItem");
            // _audioManager.PlaySFX(equipSound);
        }
        
        private void PlayDiscardItemSFX()
        {
            // var discardSound = ResourceService.LoadAsset<AudioClip>("DiscardItem");
            // _audioManager.PlaySFX(discardSound);
        }
        
        private void PlayErrorSFX()
        {
            // var errorSound = ResourceService.LoadAsset<AudioClip>("Error");
            // _audioManager.PlaySFX(errorSound);
        }
        
        #endregion
        
        #region UI对话框
        
        private async UniTask<bool> ShowUseItemConfirmDialog(InventoryItem item)
        {
            // 显示"确定要使用 [物品名] 吗？"对话框
            await UniTask.Delay(1000);
            return true; // 模拟用户确认
        }
        
        private async UniTask<bool> ShowDiscardConfirmDialog(InventoryItem item, int count)
        {
            // 显示"确定要丢弃 [物品名] x[数量] 吗？"对话框
            await UniTask.Delay(1000);
            return false; // 模拟用户取消
        }
        
        private async UniTask ShowCannotUseMessage(InventoryItem item)
        {
            // 显示"无法使用此物品"消息
            await UniTask.Delay(1500);
        }
        
        private async UniTask ShowItemEffect(InventoryItem item)
        {
            // 显示物品使用效果
            await UniTask.Delay(1000);
        }
        
        private async UniTask ShowEquipSuccessMessage(InventoryItem item)
        {
            // 显示装备成功消息
            await UniTask.Delay(1000);
        }
        
        #endregion
        
        #region 事件处理
        
        /// <summary>
        /// 关闭背包
        /// </summary>
        private async void OnCloseInventory()
        {
            _logService.Info("关闭背包界面");
            await _subFlowManager.PopSubFlow();
        }
        
        /// <summary>
        /// 使用选中的物品
        /// </summary>
        private async void OnUseItem()
        {
            if (_isInventoryLoaded && _filteredItems.Count > 0)
            {
                await UseItem(_selectedItemIndex);
            }
        }
        
        /// <summary>
        /// 显示物品详情
        /// </summary>
        private void OnShowItemDetail()
        {
            if (_isInventoryLoaded && _filteredItems.Count > 0)
            {
                var item = _filteredItems[_selectedItemIndex];
                _logService.Info($"显示物品详情: {item.Name} - {item.Description}");
                // 这里可以显示物品详情对话框
            }
        }
        
        #endregion
        
        /// <summary>
        /// 退出背包界面
        /// </summary>
        protected override async UniTask OnExitInternal()
        {
            _logService.Info("退出背包界面");
            
            // 清理输入事件监听
            _inputManager.OnBackPressed -= OnCloseInventory;
            _inputManager.OnConfirmPressed -= OnUseItem;
            _inputManager.OnMenuPressed -= OnShowItemDetail;
            
            // 保存背包数据（如果有修改）
            await SaveInventoryData();
            
            // 隐藏背包UI
            await HideInventoryUI();
            
            // 播放背包关闭音效
            PlayInventoryCloseSFX();
            
            _logService.Info("背包界面退出完成");
        }
        
        /// <summary>
        /// 保存背包数据
        /// </summary>
        private async UniTask SaveInventoryData()
        {
            // 如果背包数据有修改，保存到服务器或本地
            await UniTask.Delay(300);
        }
        
        /// <summary>
        /// 隐藏背包UI
        /// </summary>
        private async UniTask HideInventoryUI()
        {
            // 隐藏背包界面
            await UniTask.Delay(400);
        }
        
        /// <summary>
        /// 播放背包关闭音效
        /// </summary>
        private void PlayInventoryCloseSFX()
        {
            // var closeSound = ResourceService.LoadAsset<AudioClip>("InventoryClose");
            // _audioManager.PlaySFX(closeSound);
        }
        
        #region 数据类
        
        /// <summary>
        /// 背包物品
        /// </summary>
        private class InventoryItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public ItemType Type { get; set; }
            public int Count { get; set; }
            public string Description { get; set; }
            public ItemRarity Rarity { get; set; }
        }
        
        /// <summary>
        /// 物品稀有度
        /// </summary>
        private enum ItemRarity
        {
            Common,     // 普通
            Uncommon,   // 不常见
            Rare,       // 稀有
            Epic,       // 史诗
            Legendary   // 传说
        }
        
        #endregion
    }
}