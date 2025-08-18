using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Infrastructure.Managers
{
    /// <summary>
    /// 音频管理器实现
    /// </summary>
    public class AudioManager : MonoBehaviour, IAudioManager
    {
        private readonly List<AudioSource> _sfxSources = new();
        private readonly List<AudioSource> _tempSfxSources = new();
        private AudioSource _musicSource;
        private AudioSource _fadingMusicSource;
        
        [SerializeField] private float _masterVolume = 1.0f;
        [SerializeField] private float _sfxVolume = 1.0f;
        [SerializeField] private float _musicVolume = 1.0f;
        
        private bool _isSFXMuted = false;
        private bool _isMusicMuted = false;
        private bool _isSFXPaused = false;
        private bool _isMusicPaused = false;
        
        private Coroutine _musicFadeCoroutine;
        
        /// <summary>
        /// 主音量（0-1）
        /// </summary>
        public float MasterVolume
        {
            get => _masterVolume;
            set
            {
                _masterVolume = Mathf.Clamp01(value);
                UpdateAllVolumes();
                OnMasterVolumeChanged?.Invoke(_masterVolume);
                Debug.Log($"Master volume set to: {_masterVolume}");
            }
        }
        
        /// <summary>
        /// 音效音量（0-1）
        /// </summary>
        public float SFXVolume
        {
            get => _sfxVolume;
            set
            {
                _sfxVolume = Mathf.Clamp01(value);
                UpdateSFXVolumes();
                OnSFXVolumeChanged?.Invoke(_sfxVolume);
                Debug.Log($"SFX volume set to: {_sfxVolume}");
            }
        }
        
        /// <summary>
        /// 音乐音量（0-1）
        /// </summary>
        public float MusicVolume
        {
            get => _musicVolume;
            set
            {
                _musicVolume = Mathf.Clamp01(value);
                UpdateMusicVolume();
                OnMusicVolumeChanged?.Invoke(_musicVolume);
                Debug.Log($"Music volume set to: {_musicVolume}");
            }
        }
        
        /// <summary>
        /// 所有音效是否静音
        /// </summary>
        public bool IsSFXMuted => _isSFXMuted;
        
        /// <summary>
        /// 音乐是否静音
        /// </summary>
        public bool IsMusicMuted => _isMusicMuted;
        
        /// <summary>
        /// 所有音效是否暂停
        /// </summary>
        public bool IsSFXPaused => _isSFXPaused;
        
        /// <summary>
        /// 音乐是否暂停
        /// </summary>
        public bool IsMusicPaused => _isMusicPaused;
        
        /// <summary>
        /// 当前播放的音乐
        /// </summary>
        public AudioClip CurrentMusic => _musicSource?.clip;
        
        /// <summary>
        /// 音量改变事件
        /// </summary>
        public event Action<float> OnMasterVolumeChanged;
        public event Action<float> OnSFXVolumeChanged;
        public event Action<float> OnMusicVolumeChanged;
        
        /// <summary>
        /// 静音状态改变事件
        /// </summary>
        public event Action<bool> OnSFXMuteChanged;
        public event Action<bool> OnMusicMuteChanged;
        
        /// <summary>
        /// 初始化音频管理器
        /// </summary>
        private void Awake()
        {
            // 创建音乐AudioSource
            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.playOnAwake = false;
            _musicSource.loop = true;
            
            Debug.Log("AudioManager initialized");
        }
        
        /// <summary>
        /// 暂停所有音效
        /// </summary>
        public void PauseAllSFX()
        {
            if (_isSFXPaused)
            {
                Debug.LogWarning("SFX is already paused");
                return;
            }
            
            foreach (var source in _sfxSources.Where(s => s != null && s.isPlaying))
            {
                source.Pause();
            }
            
            foreach (var source in _tempSfxSources.Where(s => s != null && s.isPlaying))
            {
                source.Pause();
            }
            
            _isSFXPaused = true;
            Debug.Log("All SFX paused");
        }
        
        /// <summary>
        /// 恢复所有音效
        /// </summary>
        public void ResumeAllSFX()
        {
            if (!_isSFXPaused)
            {
                Debug.LogWarning("SFX is not paused");
                return;
            }
            
            foreach (var source in _sfxSources.Where(s => s != null))
            {
                source.UnPause();
            }
            
            foreach (var source in _tempSfxSources.Where(s => s != null))
            {
                source.UnPause();
            }
            
            _isSFXPaused = false;
            Debug.Log("All SFX resumed");
        }
        
        /// <summary>
        /// 暂停音乐
        /// </summary>
        public void PauseMusic()
        {
            if (_isMusicPaused)
            {
                Debug.LogWarning("Music is already paused");
                return;
            }
            
            if (_musicSource != null && _musicSource.isPlaying)
            {
                _musicSource.Pause();
            }
            
            if (_fadingMusicSource != null && _fadingMusicSource.isPlaying)
            {
                _fadingMusicSource.Pause();
            }
            
            _isMusicPaused = true;
            Debug.Log("Music paused");
        }
        
        /// <summary>
        /// 恢复音乐
        /// </summary>
        public void ResumeMusic()
        {
            if (!_isMusicPaused)
            {
                Debug.LogWarning("Music is not paused");
                return;
            }
            
            if (_musicSource != null)
            {
                _musicSource.UnPause();
            }
            
            if (_fadingMusicSource != null)
            {
                _fadingMusicSource.UnPause();
            }
            
            _isMusicPaused = false;
            Debug.Log("Music resumed");
        }
        
        /// <summary>
        /// 停止所有音效
        /// </summary>
        public void StopAllSFX()
        {
            foreach (var source in _sfxSources.Where(s => s != null))
            {
                source.Stop();
            }
            
            foreach (var source in _tempSfxSources.Where(s => s != null))
            {
                source.Stop();
            }
            
            _isSFXPaused = false;
            Debug.Log("All SFX stopped");
        }
        
        /// <summary>
        /// 停止音乐
        /// </summary>
        public void StopMusic()
        {
            if (_musicFadeCoroutine != null)
            {
                StopCoroutine(_musicFadeCoroutine);
                _musicFadeCoroutine = null;
            }
            
            if (_musicSource != null)
            {
                _musicSource.Stop();
            }
            
            if (_fadingMusicSource != null)
            {
                _fadingMusicSource.Stop();
                Destroy(_fadingMusicSource);
                _fadingMusicSource = null;
            }
            
            _isMusicPaused = false;
            Debug.Log("Music stopped");
        }
        
        /// <summary>
        /// 播放音效
        /// </summary>
        /// <param name="clip">音效片段</param>
        /// <param name="volume">音量（可选，默认使用SFXVolume）</param>
        /// <param name="pitch">音调（可选，默认1.0）</param>
        /// <returns>播放的AudioSource</returns>
        public AudioSource PlaySFX(AudioClip clip, float? volume = null, float pitch = 1.0f)
        {
            if (clip == null)
            {
                Debug.LogWarning("Cannot play null audio clip");
                return null;
            }
            
            // 查找可用的AudioSource
            var availableSource = _sfxSources.FirstOrDefault(s => s != null && !s.isPlaying);
            
            if (availableSource == null)
            {
                // 创建新的AudioSource
                availableSource = gameObject.AddComponent<AudioSource>();
                availableSource.playOnAwake = false;
                availableSource.loop = false;
                _sfxSources.Add(availableSource);
            }
            
            // 配置AudioSource
            availableSource.clip = clip;
            availableSource.volume = CalculateSFXVolume(volume ?? _sfxVolume);
            availableSource.pitch = pitch;
            availableSource.mute = _isSFXMuted;
            
            // 播放音效
            availableSource.Play();
            
            Debug.Log($"Playing SFX: {clip.name} (Volume: {availableSource.volume}, Pitch: {pitch})");
            return availableSource;
        }
        
        /// <summary>
        /// 在指定位置播放3D音效
        /// </summary>
        /// <param name="clip">音效片段</param>
        /// <param name="position">世界坐标位置</param>
        /// <param name="volume">音量（可选，默认使用SFXVolume）</param>
        /// <param name="pitch">音调（可选，默认1.0）</param>
        /// <returns>播放的AudioSource</returns>
        public AudioSource PlaySFXAtPosition(AudioClip clip, Vector3 position, float? volume = null, float pitch = 1.0f)
        {
            if (clip == null)
            {
                Debug.LogWarning("Cannot play null audio clip");
                return null;
            }
            
            // 创建临时GameObject和AudioSource
            var tempGO = new GameObject($"TempSFX_{clip.name}");
            tempGO.transform.position = position;
            
            var tempSource = tempGO.AddComponent<AudioSource>();
            tempSource.clip = clip;
            tempSource.volume = CalculateSFXVolume(volume ?? _sfxVolume);
            tempSource.pitch = pitch;
            tempSource.mute = _isSFXMuted;
            tempSource.spatialBlend = 1.0f; // 3D音效
            tempSource.playOnAwake = false;
            tempSource.loop = false;
            
            // 播放音效
            tempSource.Play();
            
            // 添加到临时列表并在播放完成后清理
            _tempSfxSources.Add(tempSource);
            StartCoroutine(CleanupTempSFX(tempSource, tempGO, clip.length / pitch));
            
            Debug.Log($"Playing 3D SFX: {clip.name} at position {position}");
            return tempSource;
        }
        
        /// <summary>
        /// 播放音乐
        /// </summary>
        /// <param name="clip">音乐片段</param>
        /// <param name="loop">是否循环播放</param>
        /// <param name="fadeInDuration">淡入时间（秒）</param>
        public void PlayMusic(AudioClip clip, bool loop = true, float fadeInDuration = 0f)
        {
            if (clip == null)
            {
                Debug.LogWarning("Cannot play null music clip");
                return;
            }
            
            StopMusic();
            
            _musicSource.clip = clip;
            _musicSource.loop = loop;
            _musicSource.mute = _isMusicMuted;
            
            if (fadeInDuration > 0f)
            {
                _musicSource.volume = 0f;
                _musicSource.Play();
                _musicFadeCoroutine = StartCoroutine(FadeMusic(_musicSource, 0f, CalculateMusicVolume(), fadeInDuration));
            }
            else
            {
                _musicSource.volume = CalculateMusicVolume();
                _musicSource.Play();
            }
            
            Debug.Log($"Playing music: {clip.name} (Loop: {loop}, Fade in: {fadeInDuration}s)");
        }
        
        /// <summary>
        /// 切换音乐
        /// </summary>
        /// <param name="clip">新音乐片段</param>
        /// <param name="fadeOutDuration">淡出时间（秒）</param>
        /// <param name="fadeInDuration">淡入时间（秒）</param>
        /// <param name="loop">是否循环播放</param>
        public void SwitchMusic(AudioClip clip, float fadeOutDuration = 1f, float fadeInDuration = 1f, bool loop = true)
        {
            if (clip == null)
            {
                Debug.LogWarning("Cannot switch to null music clip");
                return;
            }
            
            if (_musicFadeCoroutine != null)
            {
                StopCoroutine(_musicFadeCoroutine);
            }
            
            _musicFadeCoroutine = StartCoroutine(SwitchMusicCoroutine(clip, fadeOutDuration, fadeInDuration, loop));
            
            Debug.Log($"Switching music to: {clip.name} (Fade out: {fadeOutDuration}s, Fade in: {fadeInDuration}s)");
        }
        
        /// <summary>
        /// 设置音效静音状态
        /// </summary>
        /// <param name="muted">是否静音</param>
        public void SetSFXMuted(bool muted)
        {
            _isSFXMuted = muted;
            
            foreach (var source in _sfxSources.Where(s => s != null))
            {
                source.mute = muted;
            }
            
            foreach (var source in _tempSfxSources.Where(s => s != null))
            {
                source.mute = muted;
            }
            
            OnSFXMuteChanged?.Invoke(muted);
            Debug.Log($"SFX muted: {muted}");
        }
        
        /// <summary>
        /// 设置音乐静音状态
        /// </summary>
        /// <param name="muted">是否静音</param>
        public void SetMusicMuted(bool muted)
        {
            _isMusicMuted = muted;
            
            if (_musicSource != null)
            {
                _musicSource.mute = muted;
            }
            
            if (_fadingMusicSource != null)
            {
                _fadingMusicSource.mute = muted;
            }
            
            OnMusicMuteChanged?.Invoke(muted);
            Debug.Log($"Music muted: {muted}");
        }
        
        /// <summary>
        /// 切换音效静音状态
        /// </summary>
        public void ToggleSFXMute()
        {
            SetSFXMuted(!_isSFXMuted);
        }
        
        /// <summary>
        /// 切换音乐静音状态
        /// </summary>
        public void ToggleMusicMute()
        {
            SetMusicMuted(!_isMusicMuted);
        }
        
        /// <summary>
        /// 获取活跃的音效源数量
        /// </summary>
        /// <returns>活跃音效源数量</returns>
        public int GetActiveSFXCount()
        {
            int count = _sfxSources.Count(s => s != null && s.isPlaying);
            count += _tempSfxSources.Count(s => s != null && s.isPlaying);
            return count;
        }
        
        /// <summary>
        /// 清理所有音频资源
        /// </summary>
        public void Cleanup()
        {
            Debug.Log("Cleaning up AudioManager");
            
            StopAllSFX();
            StopMusic();
            
            // 清理音效源
            foreach (var source in _sfxSources.Where(s => s != null))
            {
                if (source != _musicSource)
                {
                    Destroy(source);
                }
            }
            _sfxSources.Clear();
            
            // 清理临时音效源
            foreach (var source in _tempSfxSources.Where(s => s != null))
            {
                if (source.gameObject != gameObject)
                {
                    Destroy(source.gameObject);
                }
            }
            _tempSfxSources.Clear();
            
            Debug.Log("AudioManager cleanup completed");
        }
        
        /// <summary>
        /// 计算音效最终音量
        /// </summary>
        private float CalculateSFXVolume(float baseVolume = 1.0f)
        {
            return _masterVolume * _sfxVolume * baseVolume;
        }
        
        /// <summary>
        /// 计算音乐最终音量
        /// </summary>
        private float CalculateMusicVolume()
        {
            return _masterVolume * _musicVolume;
        }
        
        /// <summary>
        /// 更新所有音量
        /// </summary>
        private void UpdateAllVolumes()
        {
            UpdateSFXVolumes();
            UpdateMusicVolume();
        }
        
        /// <summary>
        /// 更新音效音量
        /// </summary>
        private void UpdateSFXVolumes()
        {
            foreach (var source in _sfxSources.Where(s => s != null))
            {
                source.volume = CalculateSFXVolume();
            }
            
            foreach (var source in _tempSfxSources.Where(s => s != null))
            {
                source.volume = CalculateSFXVolume();
            }
        }
        
        /// <summary>
        /// 更新音乐音量
        /// </summary>
        private void UpdateMusicVolume()
        {
            if (_musicSource != null)
            {
                _musicSource.volume = CalculateMusicVolume();
            }
            
            if (_fadingMusicSource != null)
            {
                _fadingMusicSource.volume = CalculateMusicVolume();
            }
        }
        
        /// <summary>
        /// 音乐淡入淡出协程
        /// </summary>
        private IEnumerator FadeMusic(AudioSource source, float fromVolume, float toVolume, float duration)
        {
            float elapsedTime = 0f;
            source.volume = fromVolume;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float t = elapsedTime / duration;
                source.volume = Mathf.Lerp(fromVolume, toVolume, t);
                yield return null;
            }
            
            source.volume = toVolume;
        }
        
        /// <summary>
        /// 音乐切换协程
        /// </summary>
        private IEnumerator SwitchMusicCoroutine(AudioClip newClip, float fadeOutDuration, float fadeInDuration, bool loop)
        {
            // 淡出当前音乐
            if (_musicSource.isPlaying && fadeOutDuration > 0f)
            {
                yield return StartCoroutine(FadeMusic(_musicSource, _musicSource.volume, 0f, fadeOutDuration));
            }
            
            // 切换音乐
            _musicSource.Stop();
            _musicSource.clip = newClip;
            _musicSource.loop = loop;
            _musicSource.volume = 0f;
            _musicSource.Play();
            
            // 淡入新音乐
            if (fadeInDuration > 0f)
            {
                yield return StartCoroutine(FadeMusic(_musicSource, 0f, CalculateMusicVolume(), fadeInDuration));
            }
            else
            {
                _musicSource.volume = CalculateMusicVolume();
            }
        }
        
        /// <summary>
        /// 清理临时音效协程
        /// </summary>
        private IEnumerator CleanupTempSFX(AudioSource source, GameObject tempObject, float delay)
        {
            yield return new WaitForSeconds(delay + 0.1f); // 多等待0.1秒确保播放完成
            
            if (source != null)
            {
                _tempSfxSources.Remove(source);
            }
            
            if (tempObject != null)
            {
                Destroy(tempObject);
            }
        }
        
        /// <summary>
        /// 组件销毁时清理
        /// </summary>
        private void OnDestroy()
        {
            Cleanup();
        }
    }
}