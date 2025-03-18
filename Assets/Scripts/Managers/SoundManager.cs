using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager
{   
    AudioSource[] _audioSources = new AudioSource[(int)Define.Sound.MaxCount];
    //캐싱하는법1
    Dictionary<string, AudioClip> _audioClips = new Dictionary<string, AudioClip>();

    //MP3 player -> AudioSource
    //MP3 음원 -> AudioClip
    //관객(귀) -> AudioListener
    //spatial sound 3dsound 작업

    public void Init()
    {
        GameObject root = GameObject.Find("@Sound");
        if(root == null)
        {
            root = new GameObject { name = "@Sound"};
            UnityEngine.Object.DontDestroyOnLoad(root);

            string[] soundNames = System.Enum.GetNames(typeof(Define.Sound));
            for(int i = 0; i < soundNames.Length-1; i++)
            {
                GameObject go =new GameObject { name = soundNames[i]};
                _audioSources[i] = go.AddComponent<AudioSource>();
                go.transform.parent = root.transform;
            }

            _audioSources[(int)Define.Sound.Bgm].loop = true;
        }
    }
    public void Clear()
    {   
        foreach(AudioSource audioSource in _audioSources)
        {
            audioSource.clip =null;
            audioSource.Stop();
        }
        _audioClips.Clear();  
    }
    public void Play(String path, Define.Sound type = Define.Sound.Effect ,float pitch = 1.0f)
    {
        AudioClip audioClip =GetOrAddAudioClip(path, type);
        Play(audioClip, type, pitch);
    }

    public void Play(AudioClip audioClip , Define.Sound type = Define.Sound.Effect ,float pitch = 1.0f)
    {   if (audioClip == null)
            return;
        if(type == Define.Sound.Bgm)
        {
            AudioSource audioSource =_audioSources[(int)Define.Sound.Bgm];
            if(audioSource.isPlaying)
                audioSource.Stop();
            
            audioSource.pitch = pitch;
            audioSource.clip =audioClip;
            audioSource.Play();
        }
        else
        {
            AudioSource audioSource =_audioSources[(int)Define.Sound.Effect];
            audioSource.pitch = pitch;
            audioSource.PlayOneShot(audioClip);
        }
    }
    //캐싱2
    AudioClip GetOrAddAudioClip(string path, Define.Sound type = Define.Sound.Effect)
    {   if(path.Contains("Sounds/")== false)
        {
            path = $"Sounds/{path}";
        }
        AudioClip audioClip = null;
        if(type == Define.Sound.Bgm)
        {
            audioClip = Managers.Resource.Load<AudioClip>(path);
        }
        else
        {            
            if (_audioClips.TryGetValue(path, out audioClip) ==false)
            {
            audioClip =  Managers.Resource.Load<AudioClip>(path);
            _audioClips.Add(path, audioClip);
            }
            audioClip = GetOrAddAudioClip(path);
            if (audioClip == null)
            {
                Debug.Log("AudioClip Missing");
            }
        }
        
        if (audioClip == null)
        {
            Debug.Log("AudioClip Missing");
        }
        
        return audioClip;
    }


}
