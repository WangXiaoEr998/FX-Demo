using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tools
{
    class TimerInfo
    {
        /// <summary> 计时器guid </summary>
        public long Guid;

        public float Target;

        /// <summary> 当前剩余秒/帧数 </summary>
        public float Current;

        /// <summary> 完成回调 </summary>
        public Action Callback;

        /// <summary> 连续执行次数 （-1 =连续） </summary>
        public int LoopCount = 1;

        /// <summary> 是否帧数计数 </summary>
        public bool IsFrame = false;

        /// <summary>
        /// 是否暂停
        /// </summary>
        public bool IsPuase;

        public string Flag;
    }

    /// <summary>
    /// 计时器（有等待帧，秒，毫秒，等）
    /// 有update委托，程序退出事件
    /// 作者：容泳森
    /// 创建时间：2025-8-12
    /// </summary>
    public class Observable : MonoBehaviour
    {
        /// <summary> 程序退出事件 </summary>
        public static event Action OnAppQuit;

        /// <summary> 每帧更新事件 </summary>
        public static event Action OnUpdate;

        public static event Action OnLateUpdate;
        public static event Action OnFixedUpdate;

        /// <summary> 当程序暂停； </summary>
        public static event Action<bool> OnAppPause;

        /// <summary> 当程序获得或失去焦点； </summary>
        public static event Action<bool> OnAppFocus;

        #region ----------------生命周期----------------

        private void OnApplicationQuit()
        {
            OnAppQuit?.Invoke();
        }

        private void Update()
        {
            OnUpdateTimer();
            OnUpdate?.Invoke();
        }

        private void FixedUpdate()
        {
            OnFixedUpdate?.Invoke();
        }

        private void LateUpdate()
        {
            OnLateUpdate?.Invoke();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            OnAppPause?.Invoke(pauseStatus);
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            OnAppFocus?.Invoke(hasFocus);
        }

        #endregion

        public static Vector2 MousePos => Input.mousePosition;

        public static bool GetKeyDown(KeyCode keycode)
        {
            return Input.GetKeyDown(keycode);
        }

        public static bool GetKey(KeyCode keycode)
        {
            return Input.GetKey(keycode);
        }

        public static bool GetKeyUp(KeyCode keycode)
        {
            return Input.GetKeyUp(keycode);
        }

        public static bool GetMouseButton(int index)
        {
            return Input.GetMouseButton(index);
        }

        public static bool GetMouseButtonDown(int index)
        {
            return Input.GetMouseButtonDown(index);
        }

        public static bool GetMouseButtonUp(int index)
        {
            return Input.GetMouseButtonUp(index);
        }

        #region ---------------------计时器-------------

        private static List<TimerInfo> _timerInfos = new List<TimerInfo>();

        /// <summary>
        /// 延迟一定时间
        /// </summary>
        /// <param name="second">秒</param>
        /// <param name="callback">回调</param>
        /// <param name="loopCount">连续执行次数 （-1为循环执行）</param>
        /// <returns>计时器guid</returns>
        public static long Delay(float second, Action callback, int loopCount = 1, string flag = null)
        {
            var info = AddTimer(second, callback, loopCount, false, flag);
            return info.Guid;
        }

        /// <summary>
        /// 延迟一定时间 循环执行
        /// </summary>
        /// <param name="second">秒</param>
        /// <param name="callback">回调</param>
        /// <returns>计时器guid</returns>
        public static long DelayLoop(float second, Action callback)
        {
            return Delay(second, callback, -1);
        }

        /// <summary>
        /// 延迟一定帧数
        /// </summary>
        /// <param name="frameCount">等待帧数</param>
        /// <param name="callback">回调</param>
        /// <param name="loopCount">连续执行次数 （-1为循环执行）</param>
        /// <returns>计时器guid</returns>
        public static long DelayFrame(float frameCount, Action callback, int loopCount = 1)
        {
            var info = AddTimer(frameCount, callback, loopCount, true);
            return info.Guid;
        }

        public static long NextFrame(Action callback)
        {
            var info = AddTimer(1, callback, 1, true);
            return info.Guid;
        }

        /// <summary>
        /// 延迟一定帧数 循环执行
        /// </summary>
        /// <param name="frameCount">等待帧数</param>
        /// <param name="callback">回调</param>
        /// <returns>计时器guid</returns>
        public static long DelayFrameLoop(float frameCount, Action callback)
        {
            return DelayFrame(frameCount, callback, -1);
        }

        static TimerInfo AddTimer(float count, Action callback, int loopCount, bool isFrame, string flag = null)
        {
            var info = new TimerInfo()
            {
                Guid = GetGuid(), Target = count, Current = count, Callback = callback, LoopCount = loopCount,
                IsFrame = isFrame,
                Flag = flag
            };
            _timerInfos.Add(info);
            return info;
        }

        public static bool RemoveTimer(long guid)
        {
            return _timerInfos.RemoveAll(info => info.Guid == guid) > 0;
        }

        public static bool RemoveTimer(Action callback)
        {
            return _timerInfos.RemoveAll(info => info.Callback == callback) > 0;
        }

        public static void RemoveTimersByFlag(string flag)
        {
            if (string.IsNullOrEmpty(flag))
                return;
            _timerInfos.RemoveAll(info => info.Flag == flag);
        }

        public static void SetPause(long guid, bool isPause)
        {
            var timer = _timerInfos.Find(p => p.Guid == guid);
            if (timer != null)
            {
                timer.IsPuase = isPause;
            }
        }

        public static void SetPauseAll(bool isPause)
        {
            for (int i = 0; i < _timerInfos.Count; i++)
            {
                _timerInfos[i].IsPuase = isPause;
            }
        }


        private void OnUpdateTimer()
        {
            //倒计时
            if (_timerInfos.Count > 0)
            {
                for (int i = 0; i < _timerInfos.Count; i++)
                {
                    var info = _timerInfos[i];
                    if (info.IsPuase)
                        continue;
                    info.Current -= info.IsFrame ? 1 : Time.deltaTime;
                    //达到延时目标值
                    if (info.Current <= 0)
                    {
                        info.LoopCount--;
                        info.Current = info.Target;
                        //执行完毕
                        if (info.LoopCount == 0)
                        {
                            RemoveTimer(info.Guid);
                            i--;
                        }

                        info.Callback?.Invoke();
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// 根据GUID获取19位的唯一数字序列
        /// </summary>
        /// <returns></returns>
        public static uint GetGuid()
        {
            byte[] buffer = Guid.NewGuid().ToByteArray();
            return BitConverter.ToUInt32(buffer, 0);
        }
    }
}