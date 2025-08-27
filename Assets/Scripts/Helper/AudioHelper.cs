using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Helper
{
    /// <summary>
    /// 便捷调用音效系统  （由于表格和AB资源加载未完善  需要等待才能继续开发）
    /// 作者：容泳森
    /// 创建时间：2025-8-12
    /// </summary>
    public static class AudioHelper
    {
        #region 背景音乐

        private static string _playingBackgroundClipName;

        public static void StopBackground()
        {
            if (!string.IsNullOrEmpty(_playingBackgroundClipName))
            {
                SoundManager.Stop(_playingBackgroundClipName);
                _playingBackgroundClipName = null;
            }
        }

        /// <summary>
        /// 播放背景音乐
        /// </summary>
        /// <param name="clipName"></param>
        public static void PlayBackground(string clipName)
        {
            StopBackground();
            var path = $"Sound/Background/{clipName}";
            SoundManager.Play(path, soundTrack.BackgroundSound, true);
            _playingBackgroundClipName = path;
        }

        #endregion

        #region 效果音乐

        /// <summary>
        /// 播放效果
        /// </summary>
        /// <param name="effectId"></param>
        /// <param name="loop"></param>
        /// <param name="delay"></param>
        public static void PlayEffectAudio(string effectId, bool loop = false, float delay = 0)
        {
            var path = $"Sound/Effect/{effectId}";
            SoundManager.Play(path, soundTrack.EffectSound, loop, delay);
        }

        #endregion

        #region 音效

        /// <summary>
        /// 播放音效
        /// </summary>
        /// <param name="soundEffect"> 音效id</param>
        /// <param name="loop"> 是否连续 </param>
        /// <param name="delay">延时播放 0为不延时</param>
        public static void PlaySoundEffectAudio(string soundEffect, bool loop = false, float delay = 0)
        {
            var path = $"Sound/Click/{soundEffect}";
            SoundManager.Play(path, soundTrack.VoiceSound, loop, delay);
        }

        #endregion
    }
}