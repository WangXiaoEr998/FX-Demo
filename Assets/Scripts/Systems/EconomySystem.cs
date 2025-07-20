using UnityEngine;
using System.Collections.Generic;
using FanXing.Data;

/// <summary>
/// 经济系统，负责金钱管理、物品交易、价格计算等功能
/// 作者：黄畅修
/// 创建时间：2025-07-13
/// </summary>
public class EconomySystem : BaseGameSystem
{
    #region 字段定义
    [Header("经济系统配置")]
    [SerializeField] private int _initialPlayerGold = 100;
    [SerializeField] private float _priceFluctuationRate = 0.1f;
    [SerializeField] private float _marketUpdateInterval = 60.0f;
    
    [Header("交易配置")]
    [SerializeField] private float _sellPriceRatio = 0.7f; // 出售价格为购买价格的70%
    [SerializeField] private float _taxRate = 0.05f; // 5%的交易税
    
    // 经济数据
    private int _playerGold;
    private Dictionary<ItemType, float> _marketPrices = new Dictionary<ItemType, float>();
    private Dictionary<ItemType, int> _marketDemand = new Dictionary<ItemType, int>();
    private Dictionary<ItemType, int> _marketSupply = new Dictionary<ItemType, int>();
    
    // 交易记录
    private List<TransactionRecord> _transactionHistory = new List<TransactionRecord>();
    
    // 更新计时器
    private float _updateTimer = 0f;
    #endregion

    #region 内部类
    /// <summary>
    /// 交易记录
    /// </summary>
    [System.Serializable]
    public class TransactionRecord
    {
        public string itemName;
        public ItemType itemType;
        public int quantity;
        public float unitPrice;
        public float totalPrice;
        public bool isBuy; // true为购买，false为出售
        public float timestamp;
        public string npcName;
        
        public TransactionRecord(string item, ItemType type, int qty, float price, bool buy, string npc)
        {
            itemName = item;
            itemType = type;
            quantity = qty;
            unitPrice = price;
            totalPrice = price * qty;
            isBuy = buy;
            timestamp = Time.time;
            npcName = npc;
        }
    }

    /// <summary>
    /// 市场数据
    /// </summary>
    [System.Serializable]
    public class MarketData
    {
        public ItemType itemType;
        public float basePrice;
        public float currentPrice;
        public int demand;
        public int supply;
        public float priceHistory;
        
        public MarketData(ItemType type, float price)
        {
            itemType = type;
            basePrice = price;
            currentPrice = price;
            demand = 0;
            supply = 0;
            priceHistory = price;
        }
    }
    #endregion

    #region 属性
    /// <summary>
    /// 系统名称
    /// </summary>
    public override string SystemName => "经济系统";
    
    /// <summary>
    /// 玩家金钱
    /// </summary>
    public int PlayerGold => _playerGold;
    
    /// <summary>
    /// 交易记录数量
    /// </summary>
    public int TransactionCount => _transactionHistory.Count;
    
    /// <summary>
    /// 今日总收入
    /// </summary>
    public float TodayIncome
    {
        get
        {
            float income = 0f;
            float todayStart = Time.time - 86400f; // 24小时前
            
            foreach (var record in _transactionHistory)
            {
                if (record.timestamp >= todayStart && !record.isBuy)
                {
                    income += record.totalPrice;
                }
            }
            
            return income;
        }
    }
    
    /// <summary>
    /// 今日总支出
    /// </summary>
    public float TodayExpense
    {
        get
        {
            float expense = 0f;
            float todayStart = Time.time - 86400f; // 24小时前
            
            foreach (var record in _transactionHistory)
            {
                if (record.timestamp >= todayStart && record.isBuy)
                {
                    expense += record.totalPrice;
                }
            }
            
            return expense;
        }
    }
    #endregion

    #region BaseGameSystem抽象方法实现
    /// <summary>
    /// 系统初始化时调用
    /// </summary>
    protected override void OnInitialize()
    {
        LogDebug("经济系统初始化开始");

        // 初始化玩家金钱
        _playerGold = _initialPlayerGold;

        // 初始化市场价格
        InitializeMarketPrices();

        // 注册事件监听器
        RegisterEventListeners();

        LogDebug($"经济系统初始化完成，玩家初始金钱: {_playerGold}");
    }

    /// <summary>
    /// 系统启动时调用
    /// </summary>
    protected override void OnStart()
    {
        LogDebug("经济系统启动");

        // 开始更新计时器
        _updateTimer = 0f;

        LogDebug("经济系统启动完成");
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

        // 定期更新市场价格
        if (_updateTimer >= _marketUpdateInterval)
        {
            UpdateMarketPrices();
            _updateTimer = 0f;
        }
    }
    #endregion

    #region 私有方法
    /// <summary>
    /// 初始化市场价格
    /// </summary>
    private void InitializeMarketPrices()
    {
        // 设置基础物品价格
        _marketPrices[ItemType.Consumable] = 10f;
        _marketPrices[ItemType.Equipment] = 50f;
        _marketPrices[ItemType.Material] = 5f;
        _marketPrices[ItemType.Valuable] = 100f;
        _marketPrices[ItemType.QuestItem] = 0f; // 任务物品不可交易
        
        // 初始化市场供需
        foreach (ItemType itemType in System.Enum.GetValues(typeof(ItemType)))
        {
            _marketDemand[itemType] = Random.Range(10, 50);
            _marketSupply[itemType] = Random.Range(10, 50);
        }
    }

    /// <summary>
    /// 更新市场价格
    /// </summary>
    private void UpdateMarketPrices()
    {
        foreach (var kvp in _marketPrices)
        {
            ItemType itemType = kvp.Key;
            float currentPrice = kvp.Value;
            
            // 根据供需关系调整价格
            int demand = _marketDemand[itemType];
            int supply = _marketSupply[itemType];
            
            float supplyDemandRatio = (float)supply / demand;
            float priceChange = (1.0f - supplyDemandRatio) * _priceFluctuationRate;
            
            // 添加随机波动
            priceChange += Random.Range(-0.05f, 0.05f);
            
            float newPrice = currentPrice * (1.0f + priceChange);
            newPrice = Mathf.Max(1f, newPrice); // 价格不能低于1
            
            _marketPrices[itemType] = newPrice;
        }

        if (_enableDebugMode)
        {
            Debug.Log("市场价格已更新");
        }
    }

    /// <summary>
    /// 保存经济数据
    /// </summary>
    private void SaveEconomyData()
    {
        // TODO: 实现经济数据保存
        // 例如：保存到PlayerPrefs或文件
    }

    /// <summary>
    /// 注册事件监听
    /// </summary>
    private void RegisterEventListeners()
    {
        // TODO: 注册相关事件监听
    }

    /// <summary>
    /// 注销事件监听
    /// </summary>
    private void UnregisterEventListeners()
    {
        // TODO: 注销相关事件监听
    }
    #endregion

    #region 公共方法
    /// <summary>
    /// 添加金钱
    /// </summary>
    /// <param name="amount">金额</param>
    public void AddGold(int amount)
    {
        _playerGold += amount;
        
        if (_enableDebugMode)
        {
            Debug.Log($"获得金钱: {amount}，当前金钱: {_playerGold}");
        }
    }

    /// <summary>
    /// 扣除金钱
    /// </summary>
    /// <param name="amount">金额</param>
    /// <returns>是否扣除成功</returns>
    public bool SpendGold(int amount)
    {
        if (_playerGold < amount)
        {
            if (_enableDebugMode)
            {
                Debug.LogWarning($"金钱不足，需要: {amount}，当前: {_playerGold}");
            }
            return false;
        }

        _playerGold -= amount;
        
        if (_enableDebugMode)
        {
            Debug.Log($"消费金钱: {amount}，剩余金钱: {_playerGold}");
        }
        
        return true;
    }

    /// <summary>
    /// 购买物品
    /// </summary>
    /// <param name="itemData">物品数据</param>
    /// <param name="quantity">数量</param>
    /// <param name="npcName">NPC名称</param>
    /// <returns>是否购买成功</returns>
    public bool BuyItem(ItemData itemData, int quantity, string npcName = "")
    {
        float unitPrice = GetBuyPrice(itemData.itemType);
        float totalPrice = unitPrice * quantity;
        float tax = totalPrice * _taxRate;
        float finalPrice = totalPrice + tax;

        if (!SpendGold(Mathf.RoundToInt(finalPrice)))
        {
            return false;
        }

        // 记录交易
        var record = new TransactionRecord(itemData.itemName, itemData.itemType, quantity, unitPrice, true, npcName);
        _transactionHistory.Add(record);

        // 更新市场供需
        _marketDemand[itemData.itemType] += quantity;
        _marketSupply[itemData.itemType] -= quantity;

        if (_enableDebugMode)
        {
            Debug.Log($"购买物品: {itemData.itemName} x{quantity}，花费: {finalPrice:F1}");
        }

        return true;
    }

    /// <summary>
    /// 出售物品
    /// </summary>
    /// <param name="itemData">物品数据</param>
    /// <param name="quantity">数量</param>
    /// <param name="npcName">NPC名称</param>
    /// <returns>获得的金钱</returns>
    public int SellItem(ItemData itemData, int quantity, string npcName = "")
    {
        float unitPrice = GetSellPrice(itemData.itemType);
        float totalPrice = unitPrice * quantity;
        float tax = totalPrice * _taxRate;
        float finalPrice = totalPrice - tax;

        int goldEarned = Mathf.RoundToInt(finalPrice);
        AddGold(goldEarned);

        // 记录交易
        var record = new TransactionRecord(itemData.itemName, itemData.itemType, quantity, unitPrice, false, npcName);
        _transactionHistory.Add(record);

        // 更新市场供需
        _marketDemand[itemData.itemType] -= quantity;
        _marketSupply[itemData.itemType] += quantity;

        if (_enableDebugMode)
        {
            Debug.Log($"出售物品: {itemData.itemName} x{quantity}，获得: {goldEarned}");
        }

        return goldEarned;
    }

    /// <summary>
    /// 获取购买价格
    /// </summary>
    /// <param name="itemType">物品类型</param>
    /// <returns>购买价格</returns>
    public float GetBuyPrice(ItemType itemType)
    {
        if (_marketPrices.TryGetValue(itemType, out float price))
        {
            return price;
        }
        return 10f; // 默认价格
    }

    /// <summary>
    /// 获取出售价格
    /// </summary>
    /// <param name="itemType">物品类型</param>
    /// <returns>出售价格</returns>
    public float GetSellPrice(ItemType itemType)
    {
        return GetBuyPrice(itemType) * _sellPriceRatio;
    }

    /// <summary>
    /// 获取市场数据
    /// </summary>
    /// <param name="itemType">物品类型</param>
    /// <returns>市场数据</returns>
    public MarketData GetMarketData(ItemType itemType)
    {
        var data = new MarketData(itemType, GetBuyPrice(itemType));
        data.currentPrice = GetBuyPrice(itemType);
        data.demand = _marketDemand.GetValueOrDefault(itemType, 0);
        data.supply = _marketSupply.GetValueOrDefault(itemType, 0);
        return data;
    }

    /// <summary>
    /// 获取交易历史
    /// </summary>
    /// <param name="count">获取数量，-1为全部</param>
    /// <returns>交易记录列表</returns>
    public List<TransactionRecord> GetTransactionHistory(int count = -1)
    {
        if (count == -1 || count >= _transactionHistory.Count)
        {
            return new List<TransactionRecord>(_transactionHistory);
        }

        var recentRecords = new List<TransactionRecord>();
        int startIndex = Mathf.Max(0, _transactionHistory.Count - count);
        
        for (int i = startIndex; i < _transactionHistory.Count; i++)
        {
            recentRecords.Add(_transactionHistory[i]);
        }

        return recentRecords;
    }

    /// <summary>
    /// 检查是否有足够金钱
    /// </summary>
    /// <param name="amount">需要的金额</param>
    /// <returns>是否有足够金钱</returns>
    public bool HasEnoughGold(int amount)
    {
        return _playerGold >= amount;
    }
    #endregion
}
