using UnityEngine;
using System.IO;
using System.Collections.Generic;
using FanXing.Data;

/// <summary>
/// 数据管理器，负责游戏数据的保存、加载、配置管理
/// 作者：黄畅修，徐锵
/// 创建时间：2025-07-24
/// </summary>
public class DataManager : SingletonMono<DataManager>
{
    #region 字段定义

    [Header("数据配置")] [SerializeField] private string _saveFileName = "gamedata.json";
    [SerializeField] private string _configPath = "Configs/";
    [SerializeField] private bool _enableEncryption = false;

    [Header("调试设置")] [SerializeField] private bool _enableDebugMode = false;

    // 游戏数据
    private GameData _gameData;
    private Dictionary<string, ScriptableObject> _configs = new Dictionary<string, ScriptableObject>();

    // 文件路径
    private string _saveFilePath;
    private string _configFolderPath;

    #endregion

    #region 属性

    /// <summary>
    /// 当前游戏数据
    /// </summary>
    public GameData CurrentGameData => _gameData;

    /// <summary>
    /// 存档文件路径
    /// </summary>
    public string SaveFilePath => _saveFilePath;

    /// <summary>
    /// 是否有存档
    /// </summary>
    public bool HasSaveData => File.Exists(_saveFilePath);

    #endregion

    #region Unity生命周期

    protected override void Awake()
    {
        base.Awake();
        InitializeDataManager();
    }

    private void Start()
    {
        LoadConfigs();
        LoadGameData();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveGameData();
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            SaveGameData();
        }
    }

    #endregion

    #region 公共方法 - 存档管理

    /// <summary>
    /// 保存游戏数据
    /// </summary>
    public void SaveGameData()
    {
        if (_gameData == null)
        {
            Debug.LogError("游戏数据为空，无法保存");
            return;
        }

        try
        {
            // 更新保存时间
            _gameData.lastSaveTime = System.DateTime.Now.ToBinary();

            // 序列化数据
            string jsonData = JsonUtility.ToJson(_gameData, true);

            // 加密数据（如果启用）
            if (_enableEncryption)
            {
                jsonData = EncryptData(jsonData);
            }

            // 写入文件
            File.WriteAllText(_saveFilePath, jsonData);

            if (_enableDebugMode)
            {
                Debug.Log($"游戏数据已保存到: {_saveFilePath}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"保存游戏数据失败: {e.Message}");
        }
    }

    /// <summary>
    /// 加载游戏数据
    /// </summary>
    public void LoadGameData()
    {
        if (!HasSaveData)
        {
            CreateNewGameData();
            return;
        }

        try
        {
            // 读取文件
            string jsonData = File.ReadAllText(_saveFilePath);

            // 解密数据（如果启用）
            if (_enableEncryption)
            {
                jsonData = DecryptData(jsonData);
            }

            // 反序列化数据
            _gameData = JsonUtility.FromJson<GameData>(jsonData);

            if (_gameData == null)
            {
                Debug.LogWarning("存档数据损坏，创建新的游戏数据");
                CreateNewGameData();
                return;
            }

            if (_enableDebugMode)
            {
                Debug.Log($"游戏数据已加载: {_saveFilePath}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"加载游戏数据失败: {e.Message}");
            CreateNewGameData();
        }
    }

    /// <summary>
    /// 创建新的游戏数据
    /// </summary>
    public void CreateNewGameData()
    {
        _gameData = new GameData();
        _gameData.Initialize();

        if (_enableDebugMode)
        {
            Debug.Log("创建新的游戏数据");
        }
    }

    /// <summary>
    /// 删除存档
    /// </summary>
    public void DeleteSaveData()
    {
        if (HasSaveData)
        {
            try
            {
                File.Delete(_saveFilePath);
                CreateNewGameData();

                if (_enableDebugMode)
                {
                    Debug.Log("存档已删除");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"删除存档失败: {e.Message}");
            }
        }
    }

    #endregion

    #region 公共方法 - 配置管理

    /// <summary>
    /// 获取配置
    /// </summary>
    /// <typeparam name="T">配置类型</typeparam>
    /// <param name="configName">配置名称</param>
    /// <returns>配置对象</returns>
    public T GetConfig<T>(string configName) where T : ScriptableObject
    {
        if (_configs.ContainsKey(configName))
        {
            return _configs[configName] as T;
        }

        Debug.LogWarning($"配置不存在: {configName}");
        return null;
    }

    /// <summary>
    /// 加载配置
    /// </summary>
    /// <param name="configName">配置名称</param>
    /// <returns>是否加载成功</returns>
    public bool LoadConfig(string configName)
    {
        string configPath = _configPath + configName;
        ScriptableObject config = Resources.Load<ScriptableObject>(configPath);

        if (config != null)
        {
            _configs[configName] = config;

            if (_enableDebugMode)
            {
                Debug.Log($"配置已加载: {configName}");
            }

            return true;
        }

        Debug.LogError($"配置加载失败: {configPath}");
        return false;
    }

    /// <summary>
    /// 重新加载所有配置
    /// </summary>
    public void ReloadAllConfigs()
    {
        _configs.Clear();
        LoadConfigs();
    }

    #endregion

    #region 公共方法 - 数据访问

    /// <summary>
    /// 获取玩家数据
    /// </summary>
    /// <returns>玩家数据</returns>
    public PlayerData GetPlayerData()
    {
        return _gameData?.playerData;
    }

    /// <summary>
    /// 获取游戏设置
    /// </summary>
    /// <returns>游戏设置</returns>
    public GameSettings GetGameSettings()
    {
        return _gameData?.gameSettings;
    }

    /// <summary>
    /// 更新玩家数据
    /// </summary>
    /// <param name="playerData">新的玩家数据</param>
    public void UpdatePlayerData(PlayerData playerData)
    {
        if (_gameData != null)
        {
            _gameData.playerData = playerData;
        }
    }

    /// <summary>
    /// 更新游戏设置
    /// </summary>
    /// <param name="settings">新的游戏设置</param>
    public void UpdateGameSettings(GameSettings settings)
    {
        if (_gameData != null)
        {
            _gameData.gameSettings = settings;
        }
    }

    #endregion

    #region 私有方法

    /// <summary>
    /// 初始化数据管理器
    /// </summary>
    private void InitializeDataManager()
    {
        // 设置文件路径
        _saveFilePath = Path.Combine(Application.persistentDataPath, _saveFileName);
        _configFolderPath = Path.Combine(Application.streamingAssetsPath, "Configs");

        // 确保目录存在
        string saveDirectory = Path.GetDirectoryName(_saveFilePath);
        if (!Directory.Exists(saveDirectory))
        {
            Directory.CreateDirectory(saveDirectory);
        }

        if (_enableDebugMode)
        {
            Debug.Log($"数据管理器初始化完成");
            Debug.Log($"存档路径: {_saveFilePath}");
            Debug.Log($"配置路径: {_configFolderPath}");
        }
    }

    /// <summary>
    /// 加载所有配置
    /// </summary>
    private void LoadConfigs()
    {
        // 加载基础配置
        LoadConfig("GameConfig");
        LoadConfig("NPCConfig");
        LoadConfig("ItemConfig");
        LoadConfig("SkillConfig");

        if (_enableDebugMode)
        {
            Debug.Log($"已加载 {_configs.Count} 个配置文件");
        }
    }

    /// <summary>
    /// 加密数据
    /// </summary>
    /// <param name="data">原始数据</param>
    /// <returns>加密后的数据</returns>
    private string EncryptData(string data)
    {
        // 简单的加密实现，实际项目中应使用更安全的加密方法
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(data);
        return System.Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// 解密数据
    /// </summary>
    /// <param name="encryptedData">加密的数据</param>
    /// <returns>解密后的数据</returns>
    private string DecryptData(string encryptedData)
    {
        // 简单的解密实现
        byte[] bytes = System.Convert.FromBase64String(encryptedData);
        return System.Text.Encoding.UTF8.GetString(bytes);
    }

    #endregion
}

public class GameData
{
    public PlayerData playerData;
    public GameSettings gameSettings;
    public long lastSaveTime;

    public void Initialize()
    {
    }
}

public class GameSettings
{
}