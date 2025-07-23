using UnityEngine;
using System.Collections.Generic;
using FanXing.Data;

/// <summary>
/// 商店系统，负责商店管理、商品交易、库存管理等功能
/// 作者：黄畅修
/// 创建时间：2025-07-13
/// </summary>
public class ShopSystem : BaseGameSystem
{
    #region 字段定义
    [Header("商店系统配置")]
    [SerializeField] private int _maxShops = 20;
    [SerializeField] private float _shopUpdateInterval = 300.0f; // 5分钟更新一次商店
    [SerializeField] private int _defaultShopCapacity = 20;
    
    [Header("商店租赁配置")]
    [SerializeField] private int _shopRentCost = 1000;
    [SerializeField] private float _rentDuration = 86400f; // 24小时
    
    // 商店管理
    private List<Shop> _activeShops = new List<Shop>();
    private Dictionary<int, Shop> _shopDictionary = new Dictionary<int, Shop>();
    private List<ShopSlot> _availableShopSlots = new List<ShopSlot>();
    
    // 更新计时器
    private float _updateTimer = 0f;
    
    // 商店ID计数器
    private int _nextShopId = 1;
    #endregion

    #region 内部类
    /// <summary>
    /// 商店数据
    /// </summary>
    [System.Serializable]
    public class Shop
    {
        public int shopId;
        public string shopName;
        public string ownerName;
        public ShopType shopType;
        public Vector3 position;
        public List<ShopItem> inventory;
        public int maxCapacity;
        public float rentExpireTime;
        public bool isPlayerOwned;
        public float totalRevenue;
        
        public Shop()
        {
            inventory = new List<ShopItem>();
        }
    }

    /// <summary>
    /// 商店物品
    /// </summary>
    [System.Serializable]
    public class ShopItem
    {
        public int itemId;
        public string itemName;
        public int quantity;
        public int price;
        public ItemType itemType;
        public bool isForSale;
    }

    /// <summary>
    /// 商店位置槽
    /// </summary>
    [System.Serializable]
    public class ShopSlot
    {
        public int slotId;
        public Vector3 position;
        public bool isOccupied;
        public int rentCost;
        public Shop occupyingShop;
    }
    #endregion

    #region 属性
    /// <summary>
    /// 系统名称
    /// </summary>
    public override string SystemName => "商店系统";

    /// <summary>
    /// 活跃商店数量
    /// </summary>
    public int ActiveShopCount => _activeShops.Count;

    /// <summary>
    /// 可用商店位置数量
    /// </summary>
    public int AvailableSlotCount
    {
        get
        {
            int count = 0;
            foreach (var slot in _availableShopSlots)
            {
                if (!slot.isOccupied)
                    count++;
            }
            return count;
        }
    }
    #endregion

    #region BaseGameSystem抽象方法实现
    /// <summary>
    /// 系统初始化时调用
    /// </summary>
    protected override void OnInitialize()
    {
        LogDebug("商店系统初始化开始");
        
        // 初始化商店位置
        InitializeShopSlots();
        
        // 注册事件监听器
        RegisterEventListeners();
        
        LogDebug("商店系统初始化完成");
    }

    /// <summary>
    /// 系统启动时调用
    /// </summary>
    protected override void OnStart()
    {
        LogDebug("商店系统启动");
        
        // 加载商店数据
        LoadShopData();
        
        // 开始更新计时器
        _updateTimer = 0f;
        
        LogDebug("商店系统启动完成");
    }

    /// <summary>
    /// 系统更新时调用
    /// </summary>
    /// <param name="deltaTime">帧时间</param>
    protected override void OnUpdate(float deltaTime)
    {
        if (_isPaused)
            return;

        // 更新计时器
        _updateTimer += deltaTime;
        
        // 定期更新商店
        if (_updateTimer >= _shopUpdateInterval)
        {
            UpdateAllShops();
            _updateTimer = 0f;
        }
        
        // 检查租赁到期
        CheckRentExpiration();
    }
    #endregion

    #region 公共方法 - 商店管理
    /// <summary>
    /// 创建新商店
    /// </summary>
    /// <param name="shopName">商店名称</param>
    /// <param name="ownerName">店主名称</param>
    /// <param name="shopType">商店类型</param>
    /// <param name="position">商店位置</param>
    /// <returns>创建的商店</returns>
    public Shop CreateShop(string shopName, string ownerName, ShopType shopType, Vector3 position)
    {
        if (_activeShops.Count >= _maxShops)
        {
            LogWarning("已达到最大商店数量限制");
            return null;
        }

        var shop = new Shop
        {
            shopId = _nextShopId++,
            shopName = shopName,
            ownerName = ownerName,
            shopType = shopType,
            position = position,
            maxCapacity = _defaultShopCapacity,
            rentExpireTime = Time.time + _rentDuration,
            isPlayerOwned = true,
            totalRevenue = 0f
        };

        _activeShops.Add(shop);
        _shopDictionary[shop.shopId] = shop;

        // 占用商店位置
        var slot = FindNearestAvailableSlot(position);
        if (slot != null)
        {
            slot.isOccupied = true;
            slot.occupyingShop = shop;
        }

        LogDebug($"创建商店: {shopName} (ID: {shop.shopId})");
        
        // 触发商店创建事件
        Global.Event.TriggerEvent(Global.Events.Shop.CREATED, 
            new ShopCreatedEventArgs(shop.shopId, shopName, ownerName));

        return shop;
    }

    /// <summary>
    /// 关闭商店
    /// </summary>
    /// <param name="shopId">商店ID</param>
    /// <returns>是否成功关闭</returns>
    public bool CloseShop(int shopId)
    {
        if (!_shopDictionary.TryGetValue(shopId, out Shop shop))
        {
            LogWarning($"未找到商店 ID: {shopId}");
            return false;
        }

        // 释放商店位置
        var slot = FindSlotByShop(shop);
        if (slot != null)
        {
            slot.isOccupied = false;
            slot.occupyingShop = null;
        }

        _activeShops.Remove(shop);
        _shopDictionary.Remove(shopId);

        LogDebug($"关闭商店: {shop.shopName} (ID: {shopId})");
        
        // 触发商店关闭事件
        Global.Event.TriggerEvent(Global.Events.Shop.CLOSED, 
            new ShopClosedEventArgs(shopId, shop.shopName));

        return true;
    }

    /// <summary>
    /// 获取商店信息
    /// </summary>
    /// <param name="shopId">商店ID</param>
    /// <returns>商店信息</returns>
    public Shop GetShop(int shopId)
    {
        _shopDictionary.TryGetValue(shopId, out Shop shop);
        return shop;
    }

    /// <summary>
    /// 获取所有活跃商店
    /// </summary>
    /// <returns>活跃商店列表</returns>
    public List<Shop> GetAllActiveShops()
    {
        return new List<Shop>(_activeShops);
    }

    /// <summary>
    /// 获取玩家拥有的商店
    /// </summary>
    /// <returns>玩家商店列表</returns>
    public List<Shop> GetPlayerShops()
    {
        var playerShops = new List<Shop>();
        foreach (var shop in _activeShops)
        {
            if (shop.isPlayerOwned)
            {
                playerShops.Add(shop);
            }
        }
        return playerShops;
    }
    #endregion

    #region 公共方法 - 商品管理
    /// <summary>
    /// 向商店添加商品
    /// </summary>
    /// <param name="shopId">商店ID</param>
    /// <param name="item">商品信息</param>
    /// <returns>是否成功添加</returns>
    public bool AddItemToShop(int shopId, ShopItem item)
    {
        var shop = GetShop(shopId);
        if (shop == null)
        {
            LogWarning($"未找到商店 ID: {shopId}");
            return false;
        }

        if (shop.inventory.Count >= shop.maxCapacity)
        {
            LogWarning($"商店 {shop.shopName} 库存已满");
            return false;
        }

        // 检查是否已有相同物品
        var existingItem = shop.inventory.Find(i => i.itemId == item.itemId);
        if (existingItem != null)
        {
            existingItem.quantity += item.quantity;
        }
        else
        {
            shop.inventory.Add(item);
        }

        LogDebug($"向商店 {shop.shopName} 添加物品: {item.itemName} x{item.quantity}");
        
        // 触发物品添加事件
        Global.Event.TriggerEvent(Global.Events.Shop.ITEM_ADDED, 
            new ShopItemAddedEventArgs(shopId, item.itemId, item.quantity));

        return true;
    }

    /// <summary>
    /// 从商店移除商品
    /// </summary>
    /// <param name="shopId">商店ID</param>
    /// <param name="itemId">物品ID</param>
    /// <param name="quantity">移除数量</param>
    /// <returns>是否成功移除</returns>
    public bool RemoveItemFromShop(int shopId, int itemId, int quantity)
    {
        var shop = GetShop(shopId);
        if (shop == null)
        {
            LogWarning($"未找到商店 ID: {shopId}");
            return false;
        }

        var item = shop.inventory.Find(i => i.itemId == itemId);
        if (item == null)
        {
            LogWarning($"商店 {shop.shopName} 中未找到物品 ID: {itemId}");
            return false;
        }

        if (item.quantity < quantity)
        {
            LogWarning($"商店 {shop.shopName} 中物品 {item.itemName} 数量不足");
            return false;
        }

        item.quantity -= quantity;
        if (item.quantity <= 0)
        {
            shop.inventory.Remove(item);
        }

        LogDebug($"从商店 {shop.shopName} 移除物品: {item.itemName} x{quantity}");
        
        // 触发物品移除事件
        Global.Event.TriggerEvent(Global.Events.Shop.ITEM_REMOVED, 
            new ShopItemRemovedEventArgs(shopId, itemId, quantity));

        return true;
    }
    #endregion

    #region 私有方法
    /// <summary>
    /// 初始化商店位置
    /// </summary>
    private void InitializeShopSlots()
    {
        _availableShopSlots.Clear();

        // 创建默认商店位置
        for (int i = 0; i < _maxShops; i++)
        {
            var slot = new ShopSlot
            {
                slotId = i + 1,
                position = new Vector3(i * 5f, 0f, 0f), // 简单的位置分配
                isOccupied = false,
                rentCost = _shopRentCost
            };
            _availableShopSlots.Add(slot);
        }

        LogDebug($"初始化了 {_maxShops} 个商店位置");
    }

    /// <summary>
    /// 注册事件监听器
    /// </summary>
    private void RegisterEventListeners()
    {
        // 注册相关事件监听器
        Global.Event.Register(Global.Events.Economy.ITEM_BOUGHT, OnItemBought);
        Global.Event.Register(Global.Events.Economy.GOLD_GAINED, OnGoldGained);
    }

    /// <summary>
    /// 加载商店数据
    /// </summary>
    private void LoadShopData()
    {
        // 从数据管理器加载商店数据
        // 这里可以添加具体的数据加载逻辑
        LogDebug("商店数据加载完成");
    }

    /// <summary>
    /// 更新所有商店
    /// </summary>
    private void UpdateAllShops()
    {
        foreach (var shop in _activeShops)
        {
            UpdateShopInventory(shop);
        }
    }

    /// <summary>
    /// 更新商店库存
    /// </summary>
    /// <param name="shop">要更新的商店</param>
    private void UpdateShopInventory(Shop shop)
    {
        // 这里可以添加商店库存更新逻辑
        // 例如：自动补货、价格调整等
        LogDebug($"更新商店库存: {shop.shopName}");
    }

    /// <summary>
    /// 检查租赁到期
    /// </summary>
    private void CheckRentExpiration()
    {
        var currentTime = Time.time;
        var expiredShops = new List<Shop>();

        foreach (var shop in _activeShops)
        {
            if (shop.isPlayerOwned && currentTime > shop.rentExpireTime)
            {
                expiredShops.Add(shop);
            }
        }

        // 处理到期的商店
        foreach (var shop in expiredShops)
        {
            HandleShopRentExpiration(shop);
        }
    }

    /// <summary>
    /// 处理商店租赁到期
    /// </summary>
    /// <param name="shop">到期的商店</param>
    private void HandleShopRentExpiration(Shop shop)
    {
        LogDebug($"商店 {shop.shopName} 租赁到期");

        // 触发租赁到期事件
        Global.Event.TriggerEvent(Global.Events.Shop.RENT_EXPIRED,
            new ShopRentExpiredEventArgs(shop.shopId, shop.shopName));

        // 关闭商店
        CloseShop(shop.shopId);
    }

    /// <summary>
    /// 查找最近的可用商店位置
    /// </summary>
    /// <param name="position">目标位置</param>
    /// <returns>最近的可用位置</returns>
    private ShopSlot FindNearestAvailableSlot(Vector3 position)
    {
        ShopSlot nearestSlot = null;
        float nearestDistance = float.MaxValue;

        foreach (var slot in _availableShopSlots)
        {
            if (!slot.isOccupied)
            {
                float distance = Vector3.Distance(position, slot.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestSlot = slot;
                }
            }
        }

        return nearestSlot;
    }

    /// <summary>
    /// 根据商店查找对应的位置槽
    /// </summary>
    /// <param name="shop">商店</param>
    /// <returns>位置槽</returns>
    private ShopSlot FindSlotByShop(Shop shop)
    {
        foreach (var slot in _availableShopSlots)
        {
            if (slot.occupyingShop == shop)
            {
                return slot;
            }
        }
        return null;
    }

    /// <summary>
    /// 物品购买事件处理
    /// </summary>
    private void OnItemBought()
    {
        // 处理物品购买事件
        LogDebug("处理物品购买事件");
    }

    /// <summary>
    /// 金币获得事件处理
    /// </summary>
    private void OnGoldGained()
    {
        // 处理金币获得事件
        LogDebug("处理金币获得事件");
    }
    #endregion
}
