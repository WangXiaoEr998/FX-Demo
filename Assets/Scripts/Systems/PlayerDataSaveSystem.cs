using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using FanXing.Data;

/// <summary>
/// 玩家数据保存系统，支持多存档槽、自动保存、云存档等功能
/// 作者：徐添乐
/// 创建时间：2025-10-19
/// </summary>
public class PlayerDataSaveSystem : SingletonMono<PlayerDataSaveSystem>
{
    #region 字段定义
    [Header("保存配置")]
    [SerializeField] private bool _enableAutoSave = true;
    [SerializeField] private float _autoSaveInterval = 300f; // 5分钟
    [SerializeField] private int _maxSaveSlots = 5;
    [SerializeField] private bool _enableBackup = true;
    [SerializeField] private int _maxBackupCount = 3;
    
    [Header("加密配置")]
    [SerializeField] private bool _enableEncryption = false;
    [SerializeField] private string _encryptionKey = "FX-Demo-2025";
    
    [Header("调试设置")]
    [SerializeField] private bool _enableDebugMode = false;
    
    // 当前数据
    private PlayerData _currentPlayerData;
    private int _currentSaveSlot = 0;
    
    // 自动保存
    private float _autoSaveTimer = 0f;
    private bool _isDirty = false; // 数据是否被修改
    
    // 路径
    private string _saveDirectory;
    private string _backupDirectory;
    
    // 保存槽信息
    private Dictionary<int, SaveSlotInfo> _saveSlots = new Dictionary<int, SaveSlotInfo>();
    
    #endregion

    #region 属性
    /// <summary>
    /// 当前玩家数据
    /// </summary>
    public PlayerData CurrentPlayerData => _currentPlayerData;
    
    /// <summary>
    /// 当前保存槽
    /// </summary>
    public int CurrentSaveSlot => _currentSaveSlot;
    
    /// <summary>
    /// 是否有数据需要保存
    /// </summary>
    public bool IsDirty => _isDirty;
    
    /// <summary>
    /// 所有保存槽信息
    /// </summary>
    public Dictionary<int, SaveSlotInfo> SaveSlots => _saveSlots;
    #endregion

    #region Unity生命周期
    protected override void Awake()
    {
        base.Awake();
        InitializeSaveSystem();
    }

    private void Start()
    {
        LoadSaveSlotInfos();
    }

    private void Update()
    {
        if (_enableAutoSave && _isDirty)
        {
            _autoSaveTimer += Time.deltaTime;
            
            if (_autoSaveTimer >= _autoSaveInterval)
            {
                AutoSave();
                _autoSaveTimer = 0f;
            }
        }
    }

    private void OnApplicationQuit()
    {
        // 退出时自动保存
        if (_isDirty && _currentPlayerData != null)
        {
            SavePlayerData(_currentSaveSlot);
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        // 应用暂停时保存（移动平台）
        if (pauseStatus && _isDirty && _currentPlayerData != null)
        {
            SavePlayerData(_currentSaveSlot);
        }
    }
    #endregion

    #region 公共方法 - 保存与加载
    /// <summary>
    /// 创建新的玩家数据
    /// </summary>
    /// <param name="playerName">玩家名称</param>
    /// <param name="profession">初始职业</param>
    /// <param name="slotIndex">保存槽索引</param>
    /// <returns>是否创建成功</returns>
    public bool CreateNewPlayerData(string playerName, ProfessionType profession, int slotIndex = 0)
    {
        if (slotIndex < 0 || slotIndex >= _maxSaveSlots)
        {
            LogError($"保存槽索引超出范围: {slotIndex}");
            return false;
        }

        if (_saveSlots.ContainsKey(slotIndex) && _saveSlots[slotIndex] != null)
        {
            LogWarning($"保存槽 {slotIndex} 已存在数据");
        }

        // 创建新的玩家数据
        _currentPlayerData = new PlayerData();
        _currentPlayerData.Initialize(playerName, profession);
        _currentSaveSlot = slotIndex;
        _isDirty = true;

        // 立即保存
        bool success = SavePlayerData(slotIndex);
        
        if (success)
        {
            // 触发玩家创建事件
            Global.Event.TriggerEvent(Global.Events.Player.CREATED, 
                new PlayerCreatedEventArgs(_currentPlayerData));
            
            LogDebug($"创建新玩家数据: {playerName}, 保存槽: {slotIndex}");
        }

        return success;
    }

    /// <summary>
    /// 保存玩家数据到指定槽位
    /// </summary>
    /// <param name="slotIndex">保存槽索引</param>
    /// <returns>是否保存成功</returns>
    public bool SavePlayerData(int slotIndex)
    {
        if (_currentPlayerData == null)
        {
            LogError("当前玩家数据为空，无法保存");
            return false;
        }

        if (slotIndex < 0 || slotIndex >= _maxSaveSlots)
        {
            LogError($"保存槽索引超出范围: {slotIndex}");
            return false;
        }

        try
        {
            // 更新保存时间
            _currentPlayerData.lastSaveTime = DateTime.Now.ToBinary();
            
            // 验证数据
            if (!_currentPlayerData.IsValid())
            {
                LogWarning("玩家数据无效，尝试修复");
                _currentPlayerData.FixInvalidData();
            }

            // 创建保存数据包装器
            SaveDataWrapper wrapper = new SaveDataWrapper
            {
                playerData = _currentPlayerData,
                saveTime = DateTime.Now.ToBinary(),
                version = Application.version,
                slotIndex = slotIndex
            };

            // 序列化为JSON
            string jsonData = JsonUtility.ToJson(wrapper, true);

            // 加密（如果启用）
            if (_enableEncryption)
            {
                jsonData = EncryptData(jsonData);
            }

            // 获取保存路径
            string savePath = GetSaveFilePath(slotIndex);

            // 备份旧文件（如果存在）
            if (_enableBackup && File.Exists(savePath))
            {
                CreateBackup(slotIndex);
            }

            // 写入文件
            File.WriteAllText(savePath, jsonData);

            // 更新保存槽信息
            UpdateSaveSlotInfo(slotIndex, _currentPlayerData);

            // 保存槽信息列表
            SaveSaveSlotInfos();

            _isDirty = false;
            _autoSaveTimer = 0f;

            // 触发保存事件
            Global.Event.TriggerEvent(Global.Events.Data.SAVED, 
                new PlayerDataSavedEventArgs(_currentPlayerData));

            LogDebug($"玩家数据已保存到槽位 {slotIndex}: {savePath}");
            return true;
        }
        catch (Exception e)
        {
            LogError($"保存玩家数据失败: {e.Message}\n{e.StackTrace}");
            return false;
        }
    }

    /// <summary>
    /// 从指定槽位加载玩家数据
    /// </summary>
    /// <param name="slotIndex">保存槽索引</param>
    /// <returns>是否加载成功</returns>
    public bool LoadPlayerData(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _maxSaveSlots)
        {
            LogError($"保存槽索引超出范围: {slotIndex}");
            return false;
        }

        string savePath = GetSaveFilePath(slotIndex);

        if (!File.Exists(savePath))
        {
            LogWarning($"保存槽 {slotIndex} 不存在存档文件");
            return false;
        }

        try
        {
            // 读取文件
            string jsonData = File.ReadAllText(savePath);

            // 解密（如果启用）
            if (_enableEncryption)
            {
                jsonData = DecryptData(jsonData);
            }

            // 反序列化
            SaveDataWrapper wrapper = JsonUtility.FromJson<SaveDataWrapper>(jsonData);

            if (wrapper == null || wrapper.playerData == null)
            {
                LogError("存档数据损坏");
                
                // 尝试从备份恢复
                if (_enableBackup && TryRestoreFromBackup(slotIndex))
                {
                    LogDebug("从备份恢复成功");
                    return LoadPlayerData(slotIndex);
                }
                
                return false;
            }

            // 验证数据
            if (!wrapper.playerData.IsValid())
            {
                LogWarning("加载的数据无效，尝试修复");
                wrapper.playerData.FixInvalidData();
            }

            // 更新当前数据
            _currentPlayerData = wrapper.playerData;
            _currentSaveSlot = slotIndex;
            _isDirty = false;
            _autoSaveTimer = 0f;

            // 更新最后登录时间
            _currentPlayerData.lastLoginTime = DateTime.Now.ToBinary();

            // 触发加载事件
            Global.Event.TriggerEvent(Global.Events.Player.LOADED, 
                new PlayerLoadedEventArgs(_currentPlayerData));

            LogDebug($"玩家数据已加载: {_currentPlayerData.playerName}, 槽位: {slotIndex}");
            return true;
        }
        catch (Exception e)
        {
            LogError($"加载玩家数据失败: {e.Message}\n{e.StackTrace}");
            
            // 尝试从备份恢复
            if (_enableBackup && TryRestoreFromBackup(slotIndex))
            {
                return LoadPlayerData(slotIndex);
            }
            
            return false;
        }
    }

    /// <summary>
    /// 删除指定槽位的存档
    /// </summary>
    /// <param name="slotIndex">保存槽索引</param>
    /// <returns>是否删除成功</returns>
    public bool DeleteSaveData(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _maxSaveSlots)
        {
            LogError($"保存槽索引超出范围: {slotIndex}");
            return false;
        }

        try
        {
            string savePath = GetSaveFilePath(slotIndex);

            if (File.Exists(savePath))
            {
                File.Delete(savePath);
            }

            // 删除备份
            DeleteBackups(slotIndex);

            // 移除槽位信息
            if (_saveSlots.ContainsKey(slotIndex))
            {
                _saveSlots.Remove(slotIndex);
                SaveSaveSlotInfos();
            }

            // 如果删除的是当前槽位，清空当前数据
            if (slotIndex == _currentSaveSlot)
            {
                _currentPlayerData = null;
                _currentSaveSlot = -1;
            }

            LogDebug($"删除存档槽位 {slotIndex}");
            return true;
        }
        catch (Exception e)
        {
            LogError($"删除存档失败: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// 标记数据为脏数据（需要保存）
    /// </summary>
    public void MarkDirty()
    {
        _isDirty = true;
    }

    /// <summary>
    /// 获取指定槽位的存档信息
    /// </summary>
    /// <param name="slotIndex">保存槽索引</param>
    /// <returns>存档信息，如果不存在返回null</returns>
    public SaveSlotInfo GetSaveSlotInfo(int slotIndex)
    {
        if (_saveSlots.TryGetValue(slotIndex, out SaveSlotInfo info))
        {
            return info;
        }
        return null;
    }

    /// <summary>
    /// 检查指定槽位是否有存档
    /// </summary>
    /// <param name="slotIndex">保存槽索引</param>
    /// <returns>是否有存档</returns>
    public bool HasSaveData(int slotIndex)
    {
        return File.Exists(GetSaveFilePath(slotIndex));
    }
    #endregion

    #region 私有方法 - 初始化
    /// <summary>
    /// 初始化保存系统
    /// </summary>
    private void InitializeSaveSystem()
    {
        // 设置保存目录
        _saveDirectory = Path.Combine(Application.persistentDataPath, "SaveData");
        _backupDirectory = Path.Combine(_saveDirectory, "Backups");

        // 创建目录
        if (!Directory.Exists(_saveDirectory))
        {
            Directory.CreateDirectory(_saveDirectory);
        }

        if (_enableBackup && !Directory.Exists(_backupDirectory))
        {
            Directory.CreateDirectory(_backupDirectory);
        }

        LogDebug($"保存系统初始化完成\n保存目录: {_saveDirectory}");
    }

    /// <summary>
    /// 加载所有保存槽信息
    /// </summary>
    private void LoadSaveSlotInfos()
    {
        string infoPath = Path.Combine(_saveDirectory, "SaveSlots.json");
        
        if (!File.Exists(infoPath))
        {
            LogDebug("没有找到保存槽信息文件");
            return;
        }

        try
        {
            string jsonData = File.ReadAllText(infoPath);
            SaveSlotInfoList infoList = JsonUtility.FromJson<SaveSlotInfoList>(jsonData);
            
            if (infoList != null && infoList.slots != null)
            {
                _saveSlots.Clear();
                foreach (var info in infoList.slots)
                {
                    _saveSlots[info.slotIndex] = info;
                }
                
                LogDebug($"加载了 {_saveSlots.Count} 个保存槽信息");
            }
        }
        catch (Exception e)
        {
            LogError($"加载保存槽信息失败: {e.Message}");
        }
    }

    /// <summary>
    /// 保存所有保存槽信息
    /// </summary>
    private void SaveSaveSlotInfos()
    {
        try
        {
            SaveSlotInfoList infoList = new SaveSlotInfoList
            {
                slots = _saveSlots.Values.ToList()
            };

            string jsonData = JsonUtility.ToJson(infoList, true);
            string infoPath = Path.Combine(_saveDirectory, "SaveSlots.json");
            
            File.WriteAllText(infoPath, jsonData);
        }
        catch (Exception e)
        {
            LogError($"保存保存槽信息失败: {e.Message}");
        }
    }

    /// <summary>
    /// 更新保存槽信息
    /// </summary>
    private void UpdateSaveSlotInfo(int slotIndex, PlayerData playerData)
    {
        SaveSlotInfo info = new SaveSlotInfo
        {
            slotIndex = slotIndex,
            playerName = playerData.playerName,
            level = playerData.level,
            profession = playerData.currentProfession,
            playTime = playerData.totalPlayTime,
            saveTime = DateTime.Now.ToBinary(),
            location = playerData.position
        };

        _saveSlots[slotIndex] = info;
    }
    #endregion

    #region 私有方法 - 文件操作
    /// <summary>
    /// 获取保存文件路径
    /// </summary>
    private string GetSaveFilePath(int slotIndex)
    {
        return Path.Combine(_saveDirectory, $"SaveSlot_{slotIndex}.json");
    }

    /// <summary>
    /// 获取备份文件路径
    /// </summary>
    private string GetBackupFilePath(int slotIndex, int backupIndex)
    {
        return Path.Combine(_backupDirectory, $"SaveSlot_{slotIndex}_Backup_{backupIndex}.json");
    }

    /// <summary>
    /// 创建备份
    /// </summary>
    private void CreateBackup(int slotIndex)
    {
        try
        {
            string savePath = GetSaveFilePath(slotIndex);
            
            if (!File.Exists(savePath))
                return;

            // 轮换备份
            for (int i = _maxBackupCount - 1; i > 0; i--)
            {
                string oldBackup = GetBackupFilePath(slotIndex, i - 1);
                string newBackup = GetBackupFilePath(slotIndex, i);
                
                if (File.Exists(oldBackup))
                {
                    if (File.Exists(newBackup))
                        File.Delete(newBackup);
                    
                    File.Move(oldBackup, newBackup);
                }
            }

            // 创建新备份
            string backupPath = GetBackupFilePath(slotIndex, 0);
            File.Copy(savePath, backupPath, true);
            
            LogDebug($"创建备份: 槽位 {slotIndex}");
        }
        catch (Exception e)
        {
            LogError($"创建备份失败: {e.Message}");
        }
    }

    /// <summary>
    /// 尝试从备份恢复
    /// </summary>
    private bool TryRestoreFromBackup(int slotIndex)
    {
        try
        {
            // 尝试从最新的备份开始恢复
            for (int i = 0; i < _maxBackupCount; i++)
            {
                string backupPath = GetBackupFilePath(slotIndex, i);
                
                if (File.Exists(backupPath))
                {
                    string savePath = GetSaveFilePath(slotIndex);
                    File.Copy(backupPath, savePath, true);
                    
                    LogDebug($"从备份 {i} 恢复槽位 {slotIndex}");
                    return true;
                }
            }
            
            LogWarning($"没有找到可用的备份: 槽位 {slotIndex}");
            return false;
        }
        catch (Exception e)
        {
            LogError($"从备份恢复失败: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// 删除所有备份
    /// </summary>
    private void DeleteBackups(int slotIndex)
    {
        try
        {
            for (int i = 0; i < _maxBackupCount; i++)
            {
                string backupPath = GetBackupFilePath(slotIndex, i);
                
                if (File.Exists(backupPath))
                {
                    File.Delete(backupPath);
                }
            }
        }
        catch (Exception e)
        {
            LogError($"删除备份失败: {e.Message}");
        }
    }
    #endregion

    #region 私有方法 - 加密
    /// <summary>
    /// 加密数据
    /// </summary>
    private string EncryptData(string data)
    {
        try
        {
            // XOR加密
            byte[] dataBytes = System.Text.Encoding.UTF8.GetBytes(data);
            byte[] keyBytes = System.Text.Encoding.UTF8.GetBytes(_encryptionKey);
            
            byte[] encryptedBytes = new byte[dataBytes.Length];
            
            for (int i = 0; i < dataBytes.Length; i++)
            {
                encryptedBytes[i] = (byte)(dataBytes[i] ^ keyBytes[i % keyBytes.Length]);
            }
            
            return Convert.ToBase64String(encryptedBytes);
        }
        catch (Exception e)
        {
            LogError($"加密数据失败: {e.Message}");
            return data;
        }
    }

    /// <summary>
    /// 解密数据
    /// </summary>
    private string DecryptData(string encryptedData)
    {
        try
        {
            byte[] encryptedBytes = Convert.FromBase64String(encryptedData);
            byte[] keyBytes = System.Text.Encoding.UTF8.GetBytes(_encryptionKey);
            
            byte[] decryptedBytes = new byte[encryptedBytes.Length];
            
            for (int i = 0; i < encryptedBytes.Length; i++)
            {
                decryptedBytes[i] = (byte)(encryptedBytes[i] ^ keyBytes[i % keyBytes.Length]);
            }
            
            return System.Text.Encoding.UTF8.GetString(decryptedBytes);
        }
        catch (Exception e)
        {
            LogError($"解密数据失败: {e.Message}");
            return encryptedData;
        }
    }
    #endregion

    #region 私有方法 - 自动保存
    /// <summary>
    /// 自动保存
    /// </summary>
    private void AutoSave()
    {
        if (_currentPlayerData == null)
            return;

        LogDebug("执行自动保存");
        SavePlayerData(_currentSaveSlot);
    }
    #endregion

    #region 日志方法
    private void LogDebug(string message)
    {
        if (_enableDebugMode)
        {
            Debug.Log($"[PlayerDataSaveSystem] {message}");
        }
    }

    private void LogWarning(string message)
    {
        Debug.LogWarning($"[PlayerDataSaveSystem] {message}");
    }

    private void LogError(string message)
    {
        Debug.LogError($"[PlayerDataSaveSystem] {message}");
    }
    #endregion
}

#region 数据结构
/// <summary>
/// 保存数据包装器
/// </summary>
[Serializable]
public class SaveDataWrapper
{
    public PlayerData playerData;
    public long saveTime;
    public string version;
    public int slotIndex;
}

/// <summary>
/// 保存槽信息
/// </summary>
[Serializable]
public class SaveSlotInfo
{
    public int slotIndex;
    public string playerName;
    public int level;
    public ProfessionType profession;
    public float playTime;
    public long saveTime;
    public Vector3 location;

    public DateTime SaveDateTime => DateTime.FromBinary(saveTime);
    public string SaveTimeString => SaveDateTime.ToString("yyyy-MM-dd HH:mm:ss");
    public string PlayTimeString => TimeSpan.FromSeconds(playTime).ToString(@"hh\:mm\:ss");
}

/// <summary>
/// 保存槽信息列表（用于序列化）
/// </summary>
[Serializable]
public class SaveSlotInfoList
{
    public List<SaveSlotInfo> slots;
}
#endregion
