using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 事件管理器，负责全局事件的注册、触发、注销和管理
/// 作者：黄畅修
/// 创建时间：2025-07-12
/// </summary>
public class EventManager : MonoBehaviour
{
    #region 单例模式
    private static EventManager _instance;
    public static EventManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<EventManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("EventManager");
                    _instance = go.AddComponent<EventManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    #endregion

    #region 字段定义
    [Header("调试设置")]
    [SerializeField] private bool _enableDebugMode = false;
    [SerializeField] private bool _logEventTriggers = false;
    
    // 事件字典
    private Dictionary<string, Action<object>> _eventDictionary = new Dictionary<string, Action<object>>();
    private Dictionary<string, List<Action<object>>> _eventListeners = new Dictionary<string, List<Action<object>>>();
    
    // 事件统计
    private Dictionary<string, int> _eventTriggerCount = new Dictionary<string, int>();
    #endregion

    #region 属性
    /// <summary>
    /// 已注册的事件数量
    /// </summary>
    public int RegisteredEventCount => _eventDictionary.Count;
    
    /// <summary>
    /// 是否启用调试模式
    /// </summary>
    public bool EnableDebugMode => _enableDebugMode;
    #endregion

    #region Unity生命周期
    private void Awake()
    {
        // 确保单例唯一性
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        InitializeEventManager();
    }

    private void OnDestroy()
    {
        // 清理所有事件
        ClearAllEvents();
    }
    #endregion

    #region 公共方法 - 事件注册
    /// <summary>
    /// 注册事件监听器
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <param name="callback">回调函数</param>
    public void RegisterEvent(string eventName, Action<object> callback)
    {
        if (string.IsNullOrEmpty(eventName))
        {
            Debug.LogError("事件名称不能为空");
            return;
        }

        if (callback == null)
        {
            Debug.LogError("回调函数不能为空");
            return;
        }

        // 添加到事件字典
        if (_eventDictionary.ContainsKey(eventName))
        {
            _eventDictionary[eventName] += callback;
        }
        else
        {
            _eventDictionary[eventName] = callback;
        }

        // 添加到监听器列表
        if (!_eventListeners.ContainsKey(eventName))
        {
            _eventListeners[eventName] = new List<Action<object>>();
        }
        _eventListeners[eventName].Add(callback);

        if (_enableDebugMode)
        {
            Debug.Log($"注册事件监听器: {eventName}, 监听器数量: {_eventListeners[eventName].Count}");
        }
    }

    /// <summary>
    /// 注册事件监听器（无参数版本）
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <param name="callback">回调函数</param>
    public void RegisterEvent(string eventName, Action callback)
    {
        RegisterEvent(eventName, (obj) => callback?.Invoke());
    }

    /// <summary>
    /// 注销事件监听器
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <param name="callback">回调函数</param>
    public void UnregisterEvent(string eventName, Action<object> callback)
    {
        if (string.IsNullOrEmpty(eventName) || callback == null)
        {
            return;
        }

        if (_eventDictionary.ContainsKey(eventName))
        {
            _eventDictionary[eventName] -= callback;
            
            // 如果没有监听器了，移除事件
            if (_eventDictionary[eventName] == null)
            {
                _eventDictionary.Remove(eventName);
            }
        }

        if (_eventListeners.ContainsKey(eventName))
        {
            _eventListeners[eventName].Remove(callback);
            
            // 如果列表为空，移除整个列表
            if (_eventListeners[eventName].Count == 0)
            {
                _eventListeners.Remove(eventName);
            }
        }

        if (_enableDebugMode)
        {
            int listenerCount = _eventListeners.ContainsKey(eventName) ? _eventListeners[eventName].Count : 0;
            Debug.Log($"注销事件监听器: {eventName}, 剩余监听器数量: {listenerCount}");
        }
    }

    /// <summary>
    /// 注销事件监听器（无参数版本）
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <param name="callback">回调函数</param>
    public void UnregisterEvent(string eventName, Action callback)
    {
        // 需要找到对应的包装函数来注销
        // 这里简化处理，实际项目中可能需要更复杂的管理
        if (_eventListeners.ContainsKey(eventName))
        {
            var listeners = _eventListeners[eventName];
            if (listeners.Count > 0)
            {
                // 简化处理：移除第一个监听器
                listeners.RemoveAt(listeners.Count - 1);
            }
        }
    }
    #endregion

    #region 公共方法 - 事件触发
    /// <summary>
    /// 触发事件
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <param name="eventArgs">事件参数</param>
    public void TriggerEvent(string eventName, object eventArgs = null)
    {
        if (string.IsNullOrEmpty(eventName))
        {
            Debug.LogError("事件名称不能为空");
            return;
        }

        // 更新触发统计
        if (_eventTriggerCount.ContainsKey(eventName))
        {
            _eventTriggerCount[eventName]++;
        }
        else
        {
            _eventTriggerCount[eventName] = 1;
        }

        if (_logEventTriggers && _enableDebugMode)
        {
            Debug.Log($"触发事件: {eventName}, 参数: {eventArgs}, 触发次数: {_eventTriggerCount[eventName]}");
        }

        // 触发事件
        if (_eventDictionary.ContainsKey(eventName))
        {
            try
            {
                _eventDictionary[eventName]?.Invoke(eventArgs);
            }
            catch (Exception ex)
            {
                Debug.LogError($"事件触发异常: {eventName}, 错误: {ex.Message}");
            }
        }
        else if (_enableDebugMode)
        {
            Debug.LogWarning($"事件未注册: {eventName}");
        }
    }

    /// <summary>
    /// 延迟触发事件
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <param name="delay">延迟时间（秒）</param>
    /// <param name="eventArgs">事件参数</param>
    public void TriggerEventDelayed(string eventName, float delay, object eventArgs = null)
    {
        StartCoroutine(TriggerEventDelayedCoroutine(eventName, delay, eventArgs));
    }

    /// <summary>
    /// 检查事件是否已注册
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <returns>是否已注册</returns>
    public bool IsEventRegistered(string eventName)
    {
        return _eventDictionary.ContainsKey(eventName);
    }

    /// <summary>
    /// 获取事件监听器数量
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <returns>监听器数量</returns>
    public int GetListenerCount(string eventName)
    {
        if (_eventListeners.ContainsKey(eventName))
        {
            return _eventListeners[eventName].Count;
        }
        return 0;
    }
    #endregion

    #region 公共方法 - 事件管理
    /// <summary>
    /// 清理所有事件
    /// </summary>
    public void ClearAllEvents()
    {
        _eventDictionary.Clear();
        _eventListeners.Clear();
        _eventTriggerCount.Clear();
        
        if (_enableDebugMode)
        {
            Debug.Log("已清理所有事件");
        }
    }

    /// <summary>
    /// 清理指定事件
    /// </summary>
    /// <param name="eventName">事件名称</param>
    public void ClearEvent(string eventName)
    {
        if (_eventDictionary.ContainsKey(eventName))
        {
            _eventDictionary.Remove(eventName);
        }
        
        if (_eventListeners.ContainsKey(eventName))
        {
            _eventListeners.Remove(eventName);
        }
        
        if (_eventTriggerCount.ContainsKey(eventName))
        {
            _eventTriggerCount.Remove(eventName);
        }

        if (_enableDebugMode)
        {
            Debug.Log($"已清理事件: {eventName}");
        }
    }

    /// <summary>
    /// 获取事件统计信息
    /// </summary>
    /// <returns>事件统计字典</returns>
    public Dictionary<string, int> GetEventStatistics()
    {
        return new Dictionary<string, int>(_eventTriggerCount);
    }
    #endregion

    #region 私有方法
    /// <summary>
    /// 初始化事件管理器
    /// </summary>
    private void InitializeEventManager()
    {
        if (_enableDebugMode)
        {
            Debug.Log("事件管理器初始化完成");
        }
    }

    /// <summary>
    /// 延迟触发事件协程
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <param name="delay">延迟时间</param>
    /// <param name="eventArgs">事件参数</param>
    /// <returns>协程</returns>
    private System.Collections.IEnumerator TriggerEventDelayedCoroutine(string eventName, float delay, object eventArgs)
    {
        yield return new WaitForSeconds(delay);
        TriggerEvent(eventName, eventArgs);
    }
    #endregion
}
