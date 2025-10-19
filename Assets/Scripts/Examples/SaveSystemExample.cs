using UnityEngine;
using FanXing.Data;

/// <summary>
/// 玩家数据保存系统使用示例
/// 演示如何创建、保存、加载和删除玩家存档
/// 作者： 徐添乐
/// 创建时间：2025-10-19
/// </summary>
public class SaveSystemExample : MonoBehaviour
{
    [Header("测试配置")]
    [SerializeField] private string _testPlayerName = "TestPlayer";
    [SerializeField] private ProfessionType _testProfession = ProfessionType.Merchant;
    [SerializeField] private int _testSlotIndex = 0;

    private void Start()
    {
        // 演示如何使用保存系统
        ExampleUsage();
    }

    /// <summary>
    /// 使用示例
    /// </summary>
    private void ExampleUsage()
    {
        Debug.Log("=== 玩家数据保存系统使用示例 ===");

        // 1. 获取保存系统实例
        var saveSystem = PlayerDataSaveSystem.Instance;
        if (saveSystem == null)
        {
            Debug.LogError("保存系统未初始化！");
            return;
        }

        // 2. 查看所有存档槽
        ListAllSaveSlots();
    }

    /// <summary>
    /// 示例1：创建新存档
    /// </summary>
    [ContextMenu("Example 1: Create New Save")]
    public void Example_CreateNewSave()
    {
        Debug.Log("--- 示例1：创建新存档 ---");

        var saveSystem = PlayerDataSaveSystem.Instance;
        
        // 创建新的玩家数据并保存到槽位0
        bool success = saveSystem.CreateNewPlayerData(_testPlayerName, _testProfession, _testSlotIndex);
        
        if (success)
        {
            Debug.Log($"成功创建新存档：{_testPlayerName}");
            Debug.Log($"  职业: {_testProfession}");
            Debug.Log($"  槽位: {_testSlotIndex}");
        }
        else
        {
            Debug.LogError("创建存档失败");
        }
    }

    /// <summary>
    /// 示例2：保存当前数据
    /// </summary>
    [ContextMenu("Example 2: Save Current Data")]
    public void Example_SaveCurrentData()
    {
        Debug.Log("--- 示例2：保存当前数据 ---");

        var saveSystem = PlayerDataSaveSystem.Instance;
        
        if (saveSystem.CurrentPlayerData == null)
        {
            Debug.LogWarning("没有当前玩家数据，请先创建或加载存档");
            return;
        }

        // 修改一些数据
        saveSystem.CurrentPlayerData.gold += 100;
        saveSystem.CurrentPlayerData.experience += 50;
        
        // 标记为脏数据
        saveSystem.MarkDirty();
        
        // 手动保存
        bool success = saveSystem.SavePlayerData(saveSystem.CurrentSaveSlot);
        
        if (success)
        {
            Debug.Log($"数据已保存到槽位 {saveSystem.CurrentSaveSlot}");
            Debug.Log($"  金币: {saveSystem.CurrentPlayerData.gold}");
            Debug.Log($"  经验: {saveSystem.CurrentPlayerData.experience}");
        }
        else
        {
            Debug.LogError("保存失败");
        }
    }

    /// <summary>
    /// 示例3：加载存档
    /// </summary>
    [ContextMenu("Example 3: Load Save Data")]
    public void Example_LoadSaveData()
    {
        Debug.Log("--- 示例3：加载存档 ---");

        var saveSystem = PlayerDataSaveSystem.Instance;
        
        // 检查槽位是否有存档
        if (!saveSystem.HasSaveData(_testSlotIndex))
        {
            Debug.LogWarning($"槽位 {_testSlotIndex} 没有存档");
            return;
        }

        // 加载存档
        bool success = saveSystem.LoadPlayerData(_testSlotIndex);
        
        if (success)
        {
            var playerData = saveSystem.CurrentPlayerData;
            Debug.Log($"成功加载存档：{playerData.playerName}");
            Debug.Log($"  等级: {playerData.level}");
            Debug.Log($"  金币: {playerData.gold}");
            Debug.Log($"  经验: {playerData.experience}");
            Debug.Log($"  职业: {playerData.currentProfession}");
        }
        else
        {
            Debug.LogError("✗ 加载失败");
        }
    }

    /// <summary>
    /// 示例4：查看存档信息
    /// </summary>
    [ContextMenu("Example 4: View Save Info")]
    public void Example_ViewSaveInfo()
    {
        Debug.Log("--- 示例4：查看存档信息 ---");

        var saveSystem = PlayerDataSaveSystem.Instance;
        
        SaveSlotInfo info = saveSystem.GetSaveSlotInfo(_testSlotIndex);
        
        if (info != null)
        {
            Debug.Log($"槽位 {info.slotIndex} 信息：");
            Debug.Log($"  玩家名: {info.playerName}");
            Debug.Log($"  等级: {info.level}");
            Debug.Log($"  职业: {info.profession}");
            Debug.Log($"  游戏时长: {info.PlayTimeString}");
            Debug.Log($"  保存时间: {info.SaveTimeString}");
            Debug.Log($"  位置: {info.location}");
        }
        else
        {
            Debug.LogWarning($"槽位 {_testSlotIndex} 没有存档信息");
        }
    }

    /// <summary>
    /// 示例5：列出所有存档
    /// </summary>
    [ContextMenu("Example 5: List All Saves")]
    public void Example_ListAllSaves()
    {
        ListAllSaveSlots();
    }

    /// <summary>
    /// 示例6：删除存档
    /// </summary>
    [ContextMenu("Example 6: Delete Save")]
    public void Example_DeleteSave()
    {
        Debug.Log("--- 示例6：删除存档 ---");

        var saveSystem = PlayerDataSaveSystem.Instance;
        
        if (!saveSystem.HasSaveData(_testSlotIndex))
        {
            Debug.LogWarning($"槽位 {_testSlotIndex} 没有存档");
            return;
        }

        bool success = saveSystem.DeleteSaveData(_testSlotIndex);
        
        if (success)
        {
            Debug.Log($"已删除槽位 {_testSlotIndex} 的存档");
        }
        else
        {
            Debug.LogError("删除失败");
        }
    }

    /// <summary>
    /// 示例7：模拟游戏进程
    /// </summary>
    [ContextMenu("Example 7: Simulate Gameplay")]
    public void Example_SimulateGameplay()
    {
        Debug.Log("--- 示例7：模拟游戏进程 ---");

        var saveSystem = PlayerDataSaveSystem.Instance;
        
        if (saveSystem.CurrentPlayerData == null)
        {
            Debug.LogWarning("请先创建或加载存档");
            return;
        }

        var playerData = saveSystem.CurrentPlayerData;

        // 模拟获得金币
        Debug.Log("模拟：完成任务，获得奖励...");
        playerData.gold += 500;
        playerData.experience += 100;
        playerData.totalQuestsCompleted++;

        // 模拟升级
        if (playerData.experience >= playerData.ExperienceToNextLevel)
        {
            playerData.level++;
            Debug.Log($"恭喜升级！当前等级：{playerData.level}");
        }

        // 模拟获得物品
        Debug.Log("模拟：获得物品...");
        // playerData.inventory.Add(new ItemData()); // 需要实际的ItemData

        // 标记数据为脏数据（触发自动保存）
        saveSystem.MarkDirty();

        Debug.Log($"游戏进程已更新");
        Debug.Log($"  金币: {playerData.gold}");
        Debug.Log($"  经验: {playerData.experience}");
        Debug.Log($"  等级: {playerData.level}");
        Debug.Log("  (数据将在自动保存间隔后保存)");
    }

    /// <summary>
    /// 示例8：强制立即保存
    /// </summary>
    [ContextMenu("Example 8: Force Save Now")]
    public void Example_ForceSaveNow()
    {
        Debug.Log("--- 示例8：强制立即保存 ---");

        var saveSystem = PlayerDataSaveSystem.Instance;
        
        if (saveSystem.CurrentPlayerData == null)
        {
            Debug.LogWarning("没有当前玩家数据");
            return;
        }

        bool success = saveSystem.SavePlayerData(saveSystem.CurrentSaveSlot);
        
        if (success)
        {
            Debug.Log("数据已立即保存");
        }
        else
        {
            Debug.LogError("保存失败");
        }
    }

    /// <summary>
    /// 列出所有存档槽
    /// </summary>
    private void ListAllSaveSlots()
    {
        Debug.Log("--- 所有存档槽 ---");

        var saveSystem = PlayerDataSaveSystem.Instance;
        var saveSlots = saveSystem.SaveSlots;

        if (saveSlots.Count == 0)
        {
            Debug.Log("没有找到任何存档");
            return;
        }

        foreach (var kvp in saveSlots)
        {
            var info = kvp.Value;
            Debug.Log($"槽位 {info.slotIndex}:");
            Debug.Log($"  玩家: {info.playerName} (Lv.{info.level})");
            Debug.Log($"  职业: {info.profession}");
            Debug.Log($"  保存时间: {info.SaveTimeString}");
            Debug.Log($"  游戏时长: {info.PlayTimeString}");
        }
    }

    #region 完整的工作流示例
    /// <summary>
    /// 完整工作流示例：从创建到保存
    /// </summary>
    [ContextMenu("Full Workflow Example")]
    public void Example_FullWorkflow()
    {
        Debug.Log("=== 完整工作流示例 ===\n");

        var saveSystem = PlayerDataSaveSystem.Instance;

        // 步骤1：创建新存档
        Debug.Log("步骤1：创建新存档...");
        if (saveSystem.CreateNewPlayerData("WorkflowTest", ProfessionType.Merchant, 1))
        {
            Debug.Log("✓ 存档创建成功\n");
        }

        // 步骤2：获取并修改数据
        Debug.Log("步骤2：修改玩家数据...");
        var playerData = saveSystem.CurrentPlayerData;
        playerData.gold = 1000;
        playerData.level = 5;
        playerData.experience = 500;
        Debug.Log("✓ 数据已修改\n");

        // 步骤3：保存数据
        Debug.Log("步骤3：保存数据...");
        saveSystem.MarkDirty();
        if (saveSystem.SavePlayerData(1))
        {
            Debug.Log("数据保存成功\n");
        }

        // 步骤4：清除当前数据
        Debug.Log("步骤4：模拟关闭游戏...\n");
        
        // 步骤5：重新加载
        Debug.Log("步骤5：重新加载存档...");
        if (saveSystem.LoadPlayerData(1))
        {
            var loadedData = saveSystem.CurrentPlayerData;
            Debug.Log($"存档加载成功");
            Debug.Log($"  玩家: {loadedData.playerName}");
            Debug.Log($"  金币: {loadedData.gold}");
            Debug.Log($"  等级: {loadedData.level}");
            Debug.Log($"  经验: {loadedData.experience}");
        }

        Debug.Log("\n=== 工作流完成 ===");
    }
    #endregion
}
