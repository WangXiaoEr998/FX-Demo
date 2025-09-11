using System;
using UnityEngine;

/// <summary>
/// ���͵������࣬��Ϊ���̳�monobehaviour�ͼ̳�monobehaviour���֣�ǰ�߲�������ģʽ�����߲��ö���ģʽ�����ö���ģʽ��ԭ���Ǳ���ͳһ)
/// ���ߣ�����
/// ����ʱ�䣺2025-07-24
/// </summary>
/// <typeparam name="T"></typeparam>
public class Singleton<T> where T : Singleton<T>
{
    private static object locker = new object();

    // ���߳��޸�ʱ��ȷ������ֶ����κ�ʱ�̳��ֵĶ������µ�ֵ
    private volatile static T _instance;

    public static T Instance
    {
        get
        {
            // ˫����
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
        // ȷ������Ψһ��
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this as T;
        DontDestroyOnLoad(Instance);
    }
}