using System;
using UnityEngine;

/// <summary>
/// 泛型单例基类，分为不继承monobehaviour和继承monobehaviour两种，前者采用懒汉模式，后者采用饿汉模式（采用饿汉模式的原因是保持统一)
/// 作者：徐锵
/// 创建时间：2025-07-24
/// </summary>
/// <typeparam name="T"></typeparam>
public class Singleton<T> where T : Singleton<T>
{
    private static object locker = new object();

    // 多线程修改时，确保这个字段在任何时刻呈现的都是最新的值
    private volatile static T _instance;

    public static T Instance
    {
        get
        {
            // 双检锁
            if (_instance == null)
            {
                lock (locker)
                {
                    if (_instance == null)
                    {
                        _instance = Activator.CreateInstance(typeof(T), true) as T;
                    }
                }
            }
            return _instance;
        }
    }
}

public class SingletonMono<T> : MonoBehaviour where T : SingletonMono<T>
{
    public static T Instance { get; private set; }

    protected virtual void Awake()
    {
        // 确保单例唯一性
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this as T;
        DontDestroyOnLoad(Instance);
    }
}