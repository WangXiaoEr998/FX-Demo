using UnityEngine;
using System.Collections.Generic;
using FanXing.Data;

/// <summary>
/// 任务系统，负责任务管理、进度跟踪、奖励发放等功能
/// 作者：黄畅修
/// 创建时间：2025-07-13
/// </summary>
public class QuestSystem : BaseGameSystem
{
    #region 字段定义
    [Header("任务系统配置")]
    [SerializeField] private int _maxActiveQuests = 10;
    #pragma warning disable CS0414 // 字段已赋值但从未使用 - Demo版本中暂未实现相关功能
    [SerializeField] private int _maxCompletedQuests = 100;
    #pragma warning restore CS0414
    [SerializeField] private float _questUpdateInterval = 1.0f;
    
    // 任务管理
    private List<Quest> _activeQuests = new List<Quest>();
    private List<Quest> _completedQuests = new List<Quest>();
    private List<Quest> _availableQuests = new List<Quest>();
    private Dictionary<int, Quest> _questDictionary = new Dictionary<int, Quest>();
    
    // 更新计时器
    private float _updateTimer = 0f;
    
    // 任务ID计数器
    private int _nextQuestId = 1;
    #endregion

    #region 内部类
    /// <summary>
    /// 任务数据
    /// </summary>
    [System.Serializable]
    public class Quest
    {
        public int questId;
        public string questName;
        public string description;
        public QuestType questType;
        public QuestStatus status;
        public int currentProgress;
        public int targetProgress;
        public float acceptTime;
        public float completeTime;
        public string questGiver;
        public List<QuestReward> rewards;
        public List<QuestObjective> objectives;
        
        public Quest(int id, string name, string desc, QuestType type, string giver)
        {
            questId = id;
            questName = name;
            description = desc;
            questType = type;
            status = QuestStatus.Available;
            currentProgress = 0;
            targetProgress = 1;
            acceptTime = 0f;
            completeTime = 0f;
            questGiver = giver;
            rewards = new List<QuestReward>();
            objectives = new List<QuestObjective>();
        }
    }

    /// <summary>
    /// 任务奖励
    /// </summary>
    [System.Serializable]
    public class QuestReward
    {
        public RewardType rewardType;
        public string itemName;
        public int quantity;
        public int goldAmount;
        public int experienceAmount;
        
        public QuestReward(RewardType type, int amount)
        {
            rewardType = type;
            quantity = amount;
            goldAmount = type == RewardType.Gold ? amount : 0;
            experienceAmount = type == RewardType.Experience ? amount : 0;
        }
    }

    /// <summary>
    /// 任务目标
    /// </summary>
    [System.Serializable]
    public class QuestObjective
    {
        public string description;
        public ObjectiveType objectiveType;
        public string targetName;
        public int currentCount;
        public int targetCount;
        public bool isCompleted;
        
        public QuestObjective(string desc, ObjectiveType type, string target, int count)
        {
            description = desc;
            objectiveType = type;
            targetName = target;
            currentCount = 0;
            targetCount = count;
            isCompleted = false;
        }
    }
    #endregion

    #region 属性
    /// <summary>
    /// 系统名称
    /// </summary>
    public override string SystemName => "任务系统";
    
    /// <summary>
    /// 当前活跃任务数量
    /// </summary>
    public int ActiveQuestCount => _activeQuests.Count;
    
    /// <summary>
    /// 已完成任务数量
    /// </summary>
    public int CompletedQuestCount => _completedQuests.Count;
    
    /// <summary>
    /// 可接取任务数量
    /// </summary>
    public int AvailableQuestCount => _availableQuests.Count;
    #endregion

    #region BaseGameSystem抽象方法实现
    /// <summary>
    /// 系统初始化时调用
    /// </summary>
    protected override void OnInitialize()
    {
        LogDebug("任务系统初始化开始");

        // 初始化默认任务
        InitializeDefaultQuests();

        // 注册事件监听器
        RegisterEventListeners();

        LogDebug("任务系统初始化完成");
    }

    /// <summary>
    /// 系统启动时调用
    /// </summary>
    protected override void OnStart()
    {
        LogDebug("任务系统启动");

        // 加载任务数据
        LoadQuestData();

        // 开始更新计时器
        _updateTimer = 0f;

        LogDebug("任务系统启动完成");
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

        // 定期更新任务
        if (_updateTimer >= _questUpdateInterval)
        {
            UpdateQuests();
            _updateTimer = 0f;
        }
    }
    #endregion

    #region 私有方法
    /// <summary>
    /// 初始化默认任务
    /// </summary>
    private void InitializeDefaultQuests()
    {
        // 创建一些默认任务
        var quest1 = new Quest(_nextQuestId++, "新手指导", "学习基本操作", QuestType.Main, "村长");
        quest1.objectives.Add(new QuestObjective("移动到指定位置", ObjectiveType.MoveTo, "新手区域", 1));
        quest1.rewards.Add(new QuestReward(RewardType.Experience, 100));
        quest1.rewards.Add(new QuestReward(RewardType.Gold, 50));
        _availableQuests.Add(quest1);
        _questDictionary[quest1.questId] = quest1;

        var quest2 = new Quest(_nextQuestId++, "收集材料", "收集10个木材", QuestType.Side, "木匠");
        quest2.objectives.Add(new QuestObjective("收集木材", ObjectiveType.Collect, "木材", 10));
        quest2.rewards.Add(new QuestReward(RewardType.Gold, 100));
        _availableQuests.Add(quest2);
        _questDictionary[quest2.questId] = quest2;

        var quest3 = new Quest(_nextQuestId++, "击败敌人", "击败5只野狼", QuestType.Combat, "守卫队长");
        quest3.objectives.Add(new QuestObjective("击败野狼", ObjectiveType.Kill, "野狼", 5));
        quest3.rewards.Add(new QuestReward(RewardType.Experience, 200));
        _availableQuests.Add(quest3);
        _questDictionary[quest3.questId] = quest3;
    }

    /// <summary>
    /// 更新任务进度
    /// </summary>
    private void UpdateQuests()
    {
        for (int i = _activeQuests.Count - 1; i >= 0; i--)
        {
            var quest = _activeQuests[i];
            
            // 检查任务目标完成情况
            CheckQuestObjectives(quest);
            
            // 检查任务是否完成
            if (IsQuestCompleted(quest))
            {
                CompleteQuest(quest);
            }
        }
    }

    /// <summary>
    /// 检查任务目标
    /// </summary>
    /// <param name="quest">任务</param>
    private void CheckQuestObjectives(Quest quest)
    {
        bool allObjectivesCompleted = true;
        
        foreach (var objective in quest.objectives)
        {
            if (!objective.isCompleted)
            {
                allObjectivesCompleted = false;
                
                // 这里可以添加具体的目标检查逻辑
                // 例如：检查玩家位置、背包物品、击杀数量等
            }
        }

        if (allObjectivesCompleted && quest.status == QuestStatus.InProgress)
        {
            quest.status = QuestStatus.Completed;
        }
    }

    /// <summary>
    /// 检查任务是否完成
    /// </summary>
    /// <param name="quest">任务</param>
    /// <returns>是否完成</returns>
    private bool IsQuestCompleted(Quest quest)
    {
        return quest.status == QuestStatus.Completed;
    }

    /// <summary>
    /// 完成任务
    /// </summary>
    /// <param name="quest">任务</param>
    private void CompleteQuest(Quest quest)
    {
        quest.status = QuestStatus.Completed;
        quest.completeTime = Time.time;
        
        _activeQuests.Remove(quest);
        _completedQuests.Add(quest);
        
        // 发放奖励
        GiveQuestRewards(quest);
        
        if (_enableDebugMode)
        {
            Debug.Log($"任务完成: {quest.questName}");
        }
    }

    /// <summary>
    /// 发放任务奖励
    /// </summary>
    /// <param name="quest">任务</param>
    private void GiveQuestRewards(Quest quest)
    {
        foreach (var reward in quest.rewards)
        {
            switch (reward.rewardType)
            {
                case RewardType.Gold:
                    // 通过经济系统发放金钱奖励
                    if (_enableDebugMode)
                    {
                        Debug.Log($"获得金钱奖励: {reward.goldAmount}");
                    }
                    break;
                    
                case RewardType.Experience:
                    // 通过玩家系统发放经验奖励
                    if (_enableDebugMode)
                    {
                        Debug.Log($"获得经验奖励: {reward.experienceAmount}");
                    }
                    break;
                    
                case RewardType.Item:
                    // 通过背包系统发放物品奖励
                    if (_enableDebugMode)
                    {
                        Debug.Log($"获得物品奖励: {reward.itemName} x{reward.quantity}");
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// 保存任务数据
    /// </summary>
    private void SaveQuestData()
    {
        // TODO: 实现任务数据保存
        // 例如：保存到PlayerPrefs或文件
        LogDebug("任务数据保存完成");
    }

    /// <summary>
    /// 加载任务数据
    /// </summary>
    private void LoadQuestData()
    {
        // TODO: 实现任务数据加载
        // 例如：从PlayerPrefs或文件加载
        LogDebug("任务数据加载完成");
    }

    /// <summary>
    /// 注册事件监听
    /// </summary>
    private void RegisterEventListeners()
    {
        // TODO: 注册相关事件监听
        // 例如：玩家移动、物品收集、敌人击杀等事件
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
    /// 接取任务
    /// </summary>
    /// <param name="questId">任务ID</param>
    /// <returns>是否接取成功</returns>
    public bool AcceptQuest(int questId)
    {
        if (_activeQuests.Count >= _maxActiveQuests)
        {
            Debug.LogWarning("已达到最大活跃任务数量限制");
            return false;
        }

        var quest = _availableQuests.Find(q => q.questId == questId);
        if (quest == null)
        {
            Debug.LogWarning($"未找到任务: {questId}");
            return false;
        }

        quest.status = QuestStatus.InProgress;
        quest.acceptTime = Time.time;
        
        _availableQuests.Remove(quest);
        _activeQuests.Add(quest);

        if (_enableDebugMode)
        {
            Debug.Log($"接取任务: {quest.questName}");
        }

        return true;
    }

    /// <summary>
    /// 放弃任务
    /// </summary>
    /// <param name="questId">任务ID</param>
    /// <returns>是否放弃成功</returns>
    public bool AbandonQuest(int questId)
    {
        var quest = _activeQuests.Find(q => q.questId == questId);
        if (quest == null)
        {
            Debug.LogWarning($"未找到活跃任务: {questId}");
            return false;
        }

        quest.status = QuestStatus.Available;
        quest.acceptTime = 0f;
        quest.currentProgress = 0;
        
        // 重置任务目标
        foreach (var objective in quest.objectives)
        {
            objective.currentCount = 0;
            objective.isCompleted = false;
        }

        _activeQuests.Remove(quest);
        _availableQuests.Add(quest);

        if (_enableDebugMode)
        {
            Debug.Log($"放弃任务: {quest.questName}");
        }

        return true;
    }

    /// <summary>
    /// 更新任务进度
    /// </summary>
    /// <param name="questId">任务ID</param>
    /// <param name="objectiveType">目标类型</param>
    /// <param name="targetName">目标名称</param>
    /// <param name="amount">进度增量</param>
    public void UpdateQuestProgress(int questId, ObjectiveType objectiveType, string targetName, int amount = 1)
    {
        var quest = _activeQuests.Find(q => q.questId == questId);
        if (quest == null)
            return;

        foreach (var objective in quest.objectives)
        {
            if (objective.objectiveType == objectiveType && objective.targetName == targetName && !objective.isCompleted)
            {
                objective.currentCount += amount;
                
                if (objective.currentCount >= objective.targetCount)
                {
                    objective.currentCount = objective.targetCount;
                    objective.isCompleted = true;
                    
                    if (_enableDebugMode)
                    {
                        Debug.Log($"任务目标完成: {objective.description}");
                    }
                }
                
                break;
            }
        }
    }

    /// <summary>
    /// 获取任务信息
    /// </summary>
    /// <param name="questId">任务ID</param>
    /// <returns>任务信息</returns>
    public Quest GetQuest(int questId)
    {
        _questDictionary.TryGetValue(questId, out Quest quest);
        return quest;
    }

    /// <summary>
    /// 获取活跃任务列表
    /// </summary>
    /// <returns>活跃任务列表</returns>
    public List<Quest> GetActiveQuests()
    {
        return new List<Quest>(_activeQuests);
    }

    /// <summary>
    /// 获取可接取任务列表
    /// </summary>
    /// <returns>可接取任务列表</returns>
    public List<Quest> GetAvailableQuests()
    {
        return new List<Quest>(_availableQuests);
    }

    /// <summary>
    /// 获取已完成任务列表
    /// </summary>
    /// <returns>已完成任务列表</returns>
    public List<Quest> GetCompletedQuests()
    {
        return new List<Quest>(_completedQuests);
    }

    /// <summary>
    /// 添加新任务
    /// </summary>
    /// <param name="questName">任务名称</param>
    /// <param name="description">任务描述</param>
    /// <param name="questType">任务类型</param>
    /// <param name="questGiver">任务发布者</param>
    /// <returns>新任务的ID</returns>
    public int AddQuest(string questName, string description, QuestType questType, string questGiver)
    {
        var quest = new Quest(_nextQuestId++, questName, description, questType, questGiver);
        _availableQuests.Add(quest);
        _questDictionary[quest.questId] = quest;

        if (_enableDebugMode)
        {
            Debug.Log($"添加新任务: {questName}");
        }

        return quest.questId;
    }
    #endregion
}
