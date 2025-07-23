using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 事件管理器，负责全局事件的注册、触发、注销和管理 最多可支持4个参数泛型 
/// 作者：容泳森
/// 修改时间：2025-07-22
/// </summary>
public class EventManager
{
    private static EventManager _instance;
    public static EventManager Instance => _instance ??= new EventManager();
    private readonly Dictionary<string, List<Delegate>> _eventDelegatesDic = new();

    #region RegisterEvent/注册监听事件

    public  void Register(string eventType, Action callback)
    {
        Delegate d = callback;
        Register(eventType, d);
    }

    public  void Register<T>(string eventType, Action<T> callback)
    {
        Delegate d = callback;
        Register(eventType, d);
    }

    public  void Register<T, T2>(string eventType, Action<T, T2> callback)
    {
        Delegate d = callback;
        Register(eventType, d);
    }

    public  void Register<T, T2, T3>(string eventType, Action<T, T2, T3> callback)
    {
        Delegate d = callback;
        Register(eventType, d);
    }

    public  void Register<T, T2, T3, T4>(string eventType, Action<T, T2, T3, T4> callback)
    {
        Delegate d = callback;
        Register(eventType, d);
    }

    private  void Register(string eventType, Delegate callback)
    {
        if (_eventDelegatesDic.TryGetValue(eventType, out var delegates))
        {
            if (delegates.Contains(callback))
            {
                Debug.LogError(" 重复注册事件: " + eventType.ToString());
                return;
            }

            delegates.Add(callback);
        }
        else
        {
            _eventDelegatesDic[eventType] = new List<Delegate>() { callback };
        }
    }

    #endregion

    #region UnRegisterEvent/注销事件监听

    public  void UnRegister(string eventType, Action callback)
    {
        Delegate d = callback;
        UnRegister(eventType, d);
    }

    public  void UnRegister<T>(string eventType, Action<T> callback)
    {
        Delegate d = callback;
        UnRegister(eventType, d);
    }

    public  void UnRegister<T, T2>(string eventType, Action<T, T2> callback)
    {
        Delegate d = callback;
        UnRegister(eventType, d);
    }

    public  void UnRegister<T, T2, T3>(string eventType, Action<T, T2, T3> callback)
    {
        Delegate d = callback;
        UnRegister(eventType, d);
    }

    public  void UnRegister<T, T2, T3, T4>(string eventType, Action<T, T2, T3, T4> callback)
    {
        Delegate d = callback;
        UnRegister(eventType, d);
    }

    private  void UnRegister(string eventType, Delegate callback)
    {
        if (_eventDelegatesDic.TryGetValue(eventType, out var delegates))
        {
            if (!delegates.Contains(callback)) return;
            delegates.Remove(callback);
            if (delegates.Count == 0)
                _eventDelegatesDic.Remove(eventType);
        }
    }

    #endregion

    #region TriggerEvent/事件发送

    public  void TriggerEvent(string eventType)
    {
        TriggerEvent(eventType, action => { ((Action)action)(); });
    }

    public  void TriggerEvent<T>(string eventType, T data)
    {
        TriggerEvent(eventType, action => { ((Action<T>)action)(data); });
    }

    public  void TriggerEvent<T, T2>(string eventType, T data, T2 data2)
    {
        TriggerEvent(eventType, action => { ((Action<T, T2>)action)(data, data2); });
    }

    public  void TriggerEvent<T, T2, T3>(string eventType, T data, T2 data2, T3 data3)
    {
        TriggerEvent(eventType, action => { ((Action<T, T2, T3>)action)(data, data2, data3); });
    }

    public  void TriggerEvent<T, T2, T3, T4>(string eventType, T data, T2 data2, T3 data3, T4 data4)
    {
        TriggerEvent(eventType, action => { ((Action<T, T2, T3, T4>)action)(data, data2, data3, data4); });
    }

    private  void TriggerEvent(string eventType, Action<Delegate> foreachAction, bool isQueue = false)
    {
        if (_eventDelegatesDic.TryGetValue(eventType, out var delegates))
        {
            //用foreach可能导致执行和移除冲突  不能改为foreach
            for (int i = 0; i < delegates.Count; i++)
            {
                foreachAction(delegates[i]);
            }
        }
    }

    #endregion
}