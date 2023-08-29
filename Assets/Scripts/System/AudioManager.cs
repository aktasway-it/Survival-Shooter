using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : SingletonBehavior<AudioManager>
{
    public delegate void AudioStateChanged(bool isMute);

    public static event AudioStateChanged onAudioStateChanged;

    public class SFX
    {
        public int id;
        public string name;
        public string tag;
        public AudioSource audiosource;
    }

    public bool IsMute
    {
        get { return _isMute; }
    }

    public float MusicVolume
    {
        get { return _musicVolume; }
    }

    [SerializeField] private AudioMixerGroup _audioMixerGroup;
    [SerializeField] private int _maxSFXs = 5;

    [SerializeField] private AudioClip _themeMusic;
    [SerializeField] private AudioClip _gameMusic;
    [SerializeField] private AudioClip _buttonSFX;

    private List<SFX> _sfxs = new List<SFX>();

    private bool _isMute;
    private float _musicVolume = 1.0f;
    private AudioSource _audiosource;
    private Transform _sfxPoolTransform;

    protected override void Awake()
    {
        base.Awake();
        _audiosource = GetComponent<AudioSource>();

        _sfxPoolTransform = new GameObject("SFXPool").transform;
        _sfxPoolTransform.parent = transform;
    }

    void Update()
    {
        foreach (SFX sfx in _sfxs)
        {
            sfx.audiosource.gameObject.SetActive(sfx.audiosource.isPlaying && !_isMute);
        }
    }

    public void Play(AudioSource source, AudioClip clip, float fadeInTime = 0, float volume = 1, float pitch = 1,
        bool loop = false, float startTime = 0.0f, string tag = "")
    {
        if (_isMute)
            return;

        if (source.isPlaying && source.clip && source.clip.Equals(clip))
            return;

        source.clip = clip;
        source.volume = volume;
        source.pitch = pitch;
        source.loop = loop;
        source.time = startTime;

        Play(source, volume, fadeInTime);
    }

    public int Play(AudioClip clip, float fadeInTime = 0, float volume = 1, float pitch = 1, bool loop = false,
        float startTime = 0.0f, AudioMixerGroup audioMixerGroup = null, string tag = "", Vector3 position = default, float maxDistance = 100)
    {
        if (_isMute)
            return -1;

        int sfxObjIndex = _sfxs.FindIndex(s => s.name.Equals("SFX_" + clip.name));

        if (sfxObjIndex != -1)
        {
            if (_sfxs[sfxObjIndex].audiosource.isPlaying)
                Stop(_sfxs[sfxObjIndex].audiosource);
            else
                _sfxs[sfxObjIndex].audiosource.gameObject.SetActive(true);

            _sfxs[sfxObjIndex].audiosource.transform.position = position;
            _sfxs[sfxObjIndex].audiosource.maxDistance = maxDistance;
            _sfxs[sfxObjIndex].audiosource.spatialBlend = position == default ? 0 : 1;
            Play(_sfxs[sfxObjIndex].audiosource, clip, fadeInTime, volume, pitch, loop, startTime, tag);

            return _sfxs[sfxObjIndex].id;
        }
        else
        {
            if (_sfxs.Count < _maxSFXs)
            {
                GameObject sfx = new GameObject("SFX_" + clip.name);
                sfx.transform.parent = _sfxPoolTransform;
                sfx.transform.position = position;
                AudioSource audioSource = sfx.AddComponent<AudioSource>();
                audioSource.maxDistance = maxDistance;
                audioSource.spatialBlend = position == default ? 0 : 1;
                audioSource.rolloffMode = AudioRolloffMode.Linear;

                SFX sfxObj = new SFX
                {
                    id = sfx.GetInstanceID(),
                    name = "SFX_" + clip.name,
                    tag = tag,
                    audiosource = audioSource
                };
                sfxObj.audiosource.outputAudioMixerGroup = audioMixerGroup != null ? audioMixerGroup : _audioMixerGroup;

                _sfxs.Insert(0, sfxObj);

                Play(sfxObj.audiosource, clip, fadeInTime, volume, pitch, loop, startTime, tag);

                return sfxObj.id;
            }
            else
            {
                for (int i = _sfxs.Count - 1; i >= 0; --i)
                {
                    if (!_sfxs[i].audiosource.gameObject.activeSelf)
                    {
                        _sfxs[i].name = "SFX_" + clip.name;
                        _sfxs[i].tag = tag;
                        _sfxs[i].audiosource.name = "SFX_" + clip.name;

                        _sfxs[i].audiosource.gameObject.SetActive(true);
                        _sfxs[i].audiosource.transform.position = position;
                        _sfxs[i].audiosource.maxDistance = maxDistance;
                        _sfxs[i].audiosource.spatialBlend = position == default ? 0 : 1;
                        
                        Play(_sfxs[i].audiosource, clip, fadeInTime, volume, pitch, loop, startTime, tag);

                        return _sfxs[i].id;
                    }
                }

                return -1;
            }
        }
    }

    public void Play(AudioSource source, float volume = 1, float fadeInTime = 0)
    {
        if (fadeInTime > 0)
            StartCoroutine(FadeIn(source, volume, fadeInTime));
        else
            source.Play();
    }

    public void Stop(AudioSource source, float fadeOutTime = 0)
    {
        if (fadeOutTime > 0)
            StartCoroutine(FadeOut(source, fadeOutTime));
        else
        {
            if (source)
                source.Stop();
        }
    }

    public void Stop(string name)
    {
        int index = _sfxs.FindIndex(s => s.name.Equals("SFX_" + name));

        if (index != -1)
            Stop(_sfxs[index].audiosource);
    }

    public void Stop(int sfxId)
    {
        int index = _sfxs.FindIndex(sfx => sfx.id.Equals(sfxId));

        if (index != -1)
            Stop(_sfxs[index].audiosource);
    }

    public void StopAllByTag(string tag)
    {
        List<SFX> sfxs = _sfxs.FindAll(sfx => sfx.tag.Equals(tag));

        foreach (SFX sfx in sfxs)
        {
            Stop(sfx.id);
        }
    }

    public void PlayButtonSFX(AudioClip buttonSfx = null)
    {
        if (_isMute)
            return;

        Play(buttonSfx != null ? buttonSfx : _buttonSFX, 0f, 0.5f, 1f, false, 0f, null, "");
    }

    public void SetMusicVolume(float volume, float fadeTime = 0.0f)
    {
        _musicVolume = volume;

        StopCoroutine("ChangeVolume");
        StartCoroutine("ChangeVolume", fadeTime);
    }

    public void PlayThemeMusic()
    {
        PlayMusic(_themeMusic, 0.5f, 0.5f);
    }

    public void PlayGameMusic()
    {
        PlayMusic(_gameMusic, 0.5f, 0.5f);
    }

    public void PlayMusic(AudioClip music, float fadeInTime = 1, float fadeOutTime = 1)
    {
        if ((_audiosource.clip != null && _audiosource.clip.name.Equals(music.name)) || _isMute)
        {
            if (_isMute)
                _audiosource.clip = music;

            return;
        }

        _audiosource.loop = true;

        StopAllCoroutines();
        StartCoroutine(CrossFadeMusic(music, fadeInTime, fadeOutTime));
    }

    public void SetGeneralVolume(float volume)
    {
        AudioListener.volume = volume;
    }

    IEnumerator ChangeVolume(float fadeTime)
    {
        float deltaVolume = _musicVolume - _audiosource.volume;

        if (fadeTime > 0.0f)
        {
            while (Mathf.Abs(_audiosource.volume - _musicVolume) > 0.1f)
            {
                _audiosource.volume += (deltaVolume / fadeTime) * Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
        }

        _audiosource.volume = _musicVolume;
    }

    IEnumerator CrossFadeMusic(AudioClip newClip, float fadeInTime = 1, float fadeOutTime = 1)
    {
        _audiosource.loop = true;

        yield return StartCoroutine(FadeOut(_audiosource, fadeOutTime));

        _audiosource.clip = newClip;
        Play(_audiosource, _musicVolume);

        StartCoroutine(FadeIn(_audiosource, _musicVolume, fadeInTime));
    }

    IEnumerator FadeIn(AudioSource source, float volume, float time)
    {
        float timer = Time.realtimeSinceStartup;
        float lastTime = timer;
        source.volume = 0;

        Play(source, 0);

        while ((Time.realtimeSinceStartup - timer) < time)
        {
            if (source == null)
                yield break;

            float deltaTime = Time.realtimeSinceStartup - lastTime;
            lastTime = Time.realtimeSinceStartup;

            source.volume += (volume / time) * deltaTime;

            if (source.volume >= 1)
                break;

            yield return new WaitForEndOfFrame();
        }

        if (source == null)
            yield break;

        source.volume = volume;
        if (!source.isPlaying)
            Play(source, volume);
    }

    IEnumerator FadeOut(AudioSource source, float time)
    {
        float timer = Time.realtimeSinceStartup;
        float lastTime = timer;
        while ((Time.realtimeSinceStartup - timer) < time)
        {
            if (source == null)
                yield break;

            float deltaTime = Time.realtimeSinceStartup - lastTime;
            lastTime = Time.realtimeSinceStartup;

            source.volume -= (_musicVolume / time) * deltaTime;

            if (source.volume <= 0)
                break;

            yield return new WaitForEndOfFrame();
        }

        if (source == null)
            yield break;

        source.volume = 0;
        Stop(source);
    }

    public void Mute(bool mute)
    {
        _isMute = mute;
        AudioListener.volume = mute ? 0 : 1;
        onAudioStateChanged?.Invoke(_isMute);

        if (mute)
        {
            StopMusic();

            foreach (SFX sfx in _sfxs)
            {
                sfx.audiosource.Stop();
            }
        }
        else
        {
            ResumeMusic();
        }
    }

    void ResumeMusic()
    {
        _audiosource.Play();
    }

    void StopMusic()
    {
        Stop(_audiosource);
    }
}