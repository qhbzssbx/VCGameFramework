using System;
using UnityEngine;

namespace Game.Infrastructure.Managers
{
    /// <summary>
    /// 音频管理器接口，用于控制游戏音频的播放和暂停
    /// </summary>
    public interface IAudioManager
    {
        /// <summary>
        /// 主音量（0-1）
        /// </summary>
        float MasterVolume { get; set; }
        
        /// <summary>
        /// 音效音量（0-1）
        /// </summary>
        float SFXVolume { get; set; }
        
        /// <summary>
        /// 音乐音量（0-1）
        /// </summary>
        float MusicVolume { get; set; }
        
        /// <summary>
        /// 所有音效是否静音
        /// </summary>
        bool IsSFXMuted { get; }
        
        /// <summary>
        /// 音乐是否静音
        /// </summary>
        bool IsMusicMuted { get; }
        
        /// <summary>
        /// 所有音效是否暂停
        /// </summary>
        bool IsSFXPaused { get; }
        
        /// <summary>
        /// 音乐是否暂停
        /// </summary>
        bool IsMusicPaused { get; }
        
        /// <summary>
        /// 当前播放的音乐
        /// </summary>
        AudioClip CurrentMusic { get; }
        
        /// <summary>
        /// 音量改变事件
        /// </summary>
        event Action<float> OnMasterVolumeChanged;
        event Action<float> OnSFXVolumeChanged;
        event Action<float> OnMusicVolumeChanged;
        
        /// <summary>
        /// 静音状态改变事件
        /// </summary>
        event Action<bool> OnSFXMuteChanged;
        event Action<bool> OnMusicMuteChanged;
        
        /// <summary>
        /// 暂停所有音效
        /// </summary>
        void PauseAllSFX();
        
        /// <summary>
        /// 恢复所有音效
        /// </summary>
        void ResumeAllSFX();
        
        /// <summary>
        /// 暂停音乐
        /// </summary>
        void PauseMusic();
        
        /// <summary>
        /// 恢复音乐
        /// </summary>
        void ResumeMusic();
        
        /// <summary>
        /// 停止所有音效
        /// </summary>
        void StopAllSFX();
        
        /// <summary>
        /// 停止音乐
        /// </summary>
        void StopMusic();
        
        /// <summary>
        /// 播放音效
        /// </summary>
        /// <param name="clip">音效片段</param>
        /// <param name="volume">音量（可选，默认使用SFXVolume）</param>
        /// <param name="pitch">音调（可选，默认1.0）</param>
        /// <returns>播放的AudioSource</returns>
        AudioSource PlaySFX(AudioClip clip, float? volume = null, float pitch = 1.0f);
        
        /// <summary>
        /// 在指定位置播放3D音效
        /// </summary>
        /// <param name="clip">音效片段</param>
        /// <param name="position">世界坐标位置</param>
        /// <param name="volume">音量（可选，默认使用SFXVolume）</param>
        /// <param name="pitch">音调（可选，默认1.0）</param>
        /// <returns>播放的AudioSource</returns>
        AudioSource PlaySFXAtPosition(AudioClip clip, Vector3 position, float? volume = null, float pitch = 1.0f);
        
        /// <summary>
        /// 播放音乐
        /// </summary>
        /// <param name="clip">音乐片段</param>
        /// <param name="loop">是否循环播放</param>
        /// <param name="fadeInDuration">淡入时间（秒）</param>
        void PlayMusic(AudioClip clip, bool loop = true, float fadeInDuration = 0f);
        
        /// <summary>
        /// 切换音乐
        /// </summary>
        /// <param name="clip">新音乐片段</param>
        /// <param name="fadeOutDuration">淡出时间（秒）</param>
        /// <param name="fadeInDuration">淡入时间（秒）</param>
        /// <param name="loop">是否循环播放</param>
        void SwitchMusic(AudioClip clip, float fadeOutDuration = 1f, float fadeInDuration = 1f, bool loop = true);
        
        /// <summary>
        /// 设置音效静音状态
        /// </summary>
        /// <param name="muted">是否静音</param>
        void SetSFXMuted(bool muted);
        
        /// <summary>
        /// 设置音乐静音状态
        /// </summary>
        /// <param name="muted">是否静音</param>
        void SetMusicMuted(bool muted);
        
        /// <summary>
        /// 切换音效静音状态
        /// </summary>
        void ToggleSFXMute();
        
        /// <summary>
        /// 切换音乐静音状态
        /// </summary>
        void ToggleMusicMute();
        
        /// <summary>
        /// 获取活跃的音效源数量
        /// </summary>
        /// <returns>活跃音效源数量</returns>
        int GetActiveSFXCount();
        
        /// <summary>
        /// 清理所有音频资源
        /// </summary>
        void Cleanup();
    }
}