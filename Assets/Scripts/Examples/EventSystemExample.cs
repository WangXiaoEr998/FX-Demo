using UnityEngine;
using FanXing.Data;

/// <summary>
/// 事件系统使用示例，展示如何通过Global统一入口使用事件系统
/// 避免模块间相互依赖，统一事件通信规范
/// 作者：黄畅修
/// 创建时间：2025-07-13
/// </summary>
public class EventSystemExample : MonoBehaviour
{
    #region 字段定义
    [Header("示例配置")]
    [SerializeField] private bool _enableExamples = true;
    [SerializeField] private float _exampleInterval = 5.0f;
    
    private float _timer = 0f;
    #endregion

    #region Unity生命周期
    private void Start()
    {
        if (_enableExamples)
        {
            RegisterEventListeners();
            Debug.Log("[EventSystemExample] 事件系统示例已启动");
        }
    }

    private void Update()
    {
        if (!_enableExamples)
            return;

        _timer += Time.deltaTime;
        if (_timer >= _exampleInterval)
        {
            TriggerExampleEvents();
            _timer = 0f;
        }
    }

    private void OnDestroy()
    {
        if (_enableExamples)
        {
            UnregisterEventListeners();
        }
    }
    #endregion

    #region 事件监听器注册
    /// <summary>
    /// 注册事件监听器示例
    /// </summary>
    private void RegisterEventListeners()
    {
        // 使用Global统一入口注册事件监听器
        
        // 游戏状态事件
        Global.RegisterEvent<GameStateEventArgs>(Global.Events.Game.STATE_CHANGED, OnGameStateChanged);
        Global.RegisterEvent(Global.Events.Game.STARTED, OnGameStarted);
        Global.RegisterEvent(Global.Events.Game.PAUSED, OnGamePaused);

        // 玩家事件
        Global.RegisterEvent<PlayerCreatedEventArgs>(Global.Events.Player.CREATED, OnPlayerCreated);
        Global.RegisterEvent<PlayerLevelUpEventArgs>(Global.Events.Player.LEVEL_UP, OnPlayerLevelUp);
        Global.RegisterEvent<PlayerHealthChangedEventArgs>(Global.Events.Player.HEALTH_CHANGED, OnPlayerHealthChanged);

        // 技能事件
        Global.RegisterEvent<SkillLearnedEventArgs>(Global.Events.Skill.LEARNED, OnSkillLearned);
        Global.RegisterEvent<SkillUsedEventArgs>(Global.Events.Skill.USED, OnSkillUsed);

        // 物品事件
        Global.RegisterEvent<ItemAcquiredEventArgs>(Global.Events.Item.ACQUIRED, OnItemAcquired);
        Global.RegisterEvent<ItemUsedEventArgs>(Global.Events.Item.USED, OnItemUsed);

        // NPC事件
        Global.RegisterEvent(Global.Events.NPC.DIALOGUE_STARTED, OnNPCDialogueStarted);
        Global.RegisterEvent(Global.Events.NPC.INTERACTION_STARTED, OnNPCInteractionStarted);

        // 任务事件
        Global.RegisterEvent<QuestAcceptedEventArgs>(Global.Events.Quest.ACCEPTED, OnQuestAccepted);
        Global.RegisterEvent<QuestCompletedEventArgs>(Global.Events.Quest.COMPLETED, OnQuestCompleted);

        // 战斗事件
        Global.RegisterEvent(Global.Events.Combat.STARTED, OnCombatStarted);
        Global.RegisterEvent<DamageDealtEventArgs>(Global.Events.Combat.DAMAGE_DEALT, OnDamageDealt);

        // 经济事件
        Global.RegisterEvent<GoldGainedEventArgs>(Global.Events.Economy.GOLD_GAINED, OnGoldGained);
        Global.RegisterEvent<ItemBoughtEventArgs>(Global.Events.Economy.ITEM_BOUGHT, OnItemBought);

        // 种田事件
        Global.RegisterEvent<CropPlantedEventArgs>(Global.Events.Farming.CROP_PLANTED, OnCropPlanted);
        Global.RegisterEvent<CropHarvestedEventArgs>(Global.Events.Farming.CROP_HARVESTED, OnCropHarvested);

        // UI事件
        Global.RegisterEvent(Global.Events.UI.PANEL_OPENED, OnUIPanelOpened);
        Global.RegisterEvent(Global.Events.UI.BUTTON_CLICKED, OnUIButtonClicked);

        // 数据事件
        Global.RegisterEvent(Global.Events.Data.SAVED, OnDataSaved);
        Global.RegisterEvent(Global.Events.Data.LOADED, OnDataLoaded);
        
        Debug.Log("[EventSystemExample] 所有事件监听器已注册");
    }

    /// <summary>
    /// 注销事件监听器示例
    /// </summary>
    private void UnregisterEventListeners()
    {
        // 注销所有事件监听器
        // 注意：这里只是示例，实际项目中应该精确注销对应的监听器
        
        Debug.Log("[EventSystemExample] 所有事件监听器已注销");
    }
    #endregion

    #region 事件触发示例
    /// <summary>
    /// 触发示例事件
    /// </summary>
    private void TriggerExampleEvents()
    {
        // 随机触发一个示例事件
        int eventIndex = Random.Range(0, 10);
        
        switch (eventIndex)
        {
            case 0:
                TriggerGameStateEvent();
                break;
            case 1:
                TriggerPlayerEvent();
                break;
            case 2:
                TriggerSkillEvent();
                break;
            case 3:
                TriggerItemEvent();
                break;
            case 4:
                TriggerNPCEvent();
                break;
            case 5:
                TriggerQuestEvent();
                break;
            case 6:
                TriggerCombatEvent();
                break;
            case 7:
                TriggerEconomyEvent();
                break;
            case 8:
                TriggerFarmingEvent();
                break;
            case 9:
                TriggerUIEvent();
                break;
        }
    }

    private void TriggerGameStateEvent()
    {
        var args = new GameStateEventArgs(GameState.MainMenu, GameState.Playing, Time.time);
        Global.TriggerEvent(Global.Events.Game.STATE_CHANGED, args);
    }

    private void TriggerPlayerEvent()
    {
        var args = new PlayerLevelUpEventArgs(5, 6);
        Global.TriggerEvent(Global.Events.Player.LEVEL_UP, args);
    }

    private void TriggerSkillEvent()
    {
        var args = new SkillLearnedEventArgs("TestSkill", "测试技能", 1);
        Global.TriggerEvent(Global.Events.Skill.LEARNED, args);
    }

    private void TriggerItemEvent()
    {
        var itemData = new ItemData { itemName = "测试物品", itemType = ItemType.Consumable, currentStack = 1 };
        var args = new ItemAcquiredEventArgs(itemData, 1, "拾取");
        Global.TriggerEvent(Global.Events.Item.ACQUIRED, args);
    }

    private void TriggerNPCEvent()
    {
        Global.TriggerEvent(Global.Events.NPC.DIALOGUE_STARTED, null);
    }

    private void TriggerQuestEvent()
    {
        var args = new QuestAcceptedEventArgs("quest_001", "测试任务", QuestType.Side);
        Global.TriggerEvent(Global.Events.Quest.ACCEPTED, args);
    }

    private void TriggerCombatEvent()
    {
        Global.TriggerEvent(Global.Events.Combat.STARTED, null);
    }

    private void TriggerEconomyEvent()
    {
        var args = new GoldGainedEventArgs(100, "任务奖励");
        Global.TriggerEvent(Global.Events.Economy.GOLD_GAINED, args);
    }

    private void TriggerFarmingEvent()
    {
        var args = new CropPlantedEventArgs(CropType.Wheat, Vector3Int.zero, "TestPlayer");
        Global.TriggerEvent(Global.Events.Farming.CROP_PLANTED, args);
    }

    private void TriggerUIEvent()
    {
        Global.TriggerEvent(Global.Events.UI.PANEL_OPENED, null);
    }
    #endregion

    #region 事件处理器示例
    // 游戏状态事件处理器
    private void OnGameStateChanged(GameStateEventArgs args)
    {
        Debug.Log($"[EventSystemExample] 游戏状态改变: {args.PreviousState} -> {args.NewState}");
    }

    private void OnGameStarted(object args)
    {
        Debug.Log("[EventSystemExample] 游戏开始");
    }

    private void OnGamePaused(object args)
    {
        Debug.Log("[EventSystemExample] 游戏暂停");
    }

    // 玩家事件处理器
    private void OnPlayerCreated(PlayerCreatedEventArgs args)
    {
        Debug.Log($"[EventSystemExample] 玩家创建: {args.PlayerData.playerName}");
    }

    private void OnPlayerLevelUp(PlayerLevelUpEventArgs args)
    {
        Debug.Log($"[EventSystemExample] 玩家升级: {args.PreviousLevel} -> {args.NewLevel}");
    }

    private void OnPlayerHealthChanged(PlayerHealthChangedEventArgs args)
    {
        Debug.Log($"[EventSystemExample] 玩家生命值改变: {args.OldHealth} -> {args.NewHealth}");
    }

    // 技能事件处理器
    private void OnSkillLearned(SkillLearnedEventArgs args)
    {
        Debug.Log($"[EventSystemExample] 技能学习: {args.SkillName} 等级 {args.SkillLevel}");
    }

    private void OnSkillUsed(SkillUsedEventArgs args)
    {
        Debug.Log($"[EventSystemExample] 技能使用: {args.SkillId}");
    }

    // 物品事件处理器
    private void OnItemAcquired(ItemAcquiredEventArgs args)
    {
        Debug.Log($"[EventSystemExample] 物品获得: {args.ItemData.itemName} x{args.Quantity}");
    }

    private void OnItemUsed(ItemUsedEventArgs args)
    {
        Debug.Log($"[EventSystemExample] 物品使用: {args.Item.itemName}");
    }

    // NPC事件处理器
    private void OnNPCDialogueStarted(object args)
    {
        Debug.Log("[EventSystemExample] NPC对话开始");
    }

    private void OnNPCInteractionStarted(object args)
    {
        Debug.Log("[EventSystemExample] NPC交互开始");
    }

    // 任务事件处理器
    private void OnQuestAccepted(QuestAcceptedEventArgs args)
    {
        Debug.Log($"[EventSystemExample] 任务接受: {args.QuestName}");
    }

    private void OnQuestCompleted(QuestCompletedEventArgs args)
    {
        Debug.Log($"[EventSystemExample] 任务完成: {args.QuestName}");
    }

    // 战斗事件处理器
    private void OnCombatStarted(object args)
    {
        Debug.Log("[EventSystemExample] 战斗开始");
    }

    private void OnDamageDealt(DamageDealtEventArgs args)
    {
        Debug.Log($"[EventSystemExample] 造成伤害: {args.Damage}");
    }

    // 经济事件处理器
    private void OnGoldGained(GoldGainedEventArgs args)
    {
        Debug.Log($"[EventSystemExample] 获得金钱: {args.Amount} ({args.Source})");
    }

    private void OnItemBought(ItemBoughtEventArgs args)
    {
        Debug.Log($"[EventSystemExample] 购买物品: {args.ItemName} x{args.Quantity}");
    }

    // 种田事件处理器
    private void OnCropPlanted(CropPlantedEventArgs args)
    {
        Debug.Log($"[EventSystemExample] 种植作物: {args.CropType} 在 {args.Position}");
    }

    private void OnCropHarvested(CropHarvestedEventArgs args)
    {
        Debug.Log($"[EventSystemExample] 收获作物: {args.CropType} x{args.Quantity}");
    }

    // UI事件处理器
    private void OnUIPanelOpened(object args)
    {
        Debug.Log("[EventSystemExample] UI面板打开");
    }

    private void OnUIButtonClicked(object args)
    {
        Debug.Log("[EventSystemExample] UI按钮点击");
    }

    // 数据事件处理器
    private void OnDataSaved(object args)
    {
        Debug.Log("[EventSystemExample] 数据保存");
    }

    private void OnDataLoaded(object args)
    {
        Debug.Log("[EventSystemExample] 数据加载");
    }
    #endregion
}
