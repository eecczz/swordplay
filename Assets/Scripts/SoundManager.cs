using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
public class SoundManager : SingletonBase<SoundManager>
{
    public AudioMixer mixer;
    public AudioSource bgSound;
    public AudioClip[] bglist;
    private void Start()
    {
        if(bglist.Length>0)
            BgSoundPlay(bglist[0]);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    //효과음
    public GameObject SFXPlay(string sfxName, AudioClip clip)
    {
        GameObject go = new GameObject(sfxName + "Sound");
        AudioSource audiosource = go.AddComponent<AudioSource>();
        audiosource.outputAudioMixerGroup = mixer.FindMatchingGroups("SFX")[0];
        audiosource.clip = clip;
        audiosource.Play();
        Destroy(go, clip.length);
        return go;
    }
    //배경음
    public void BgSoundPlay(AudioClip clip)
    {
        bgSound.outputAudioMixerGroup = mixer.FindMatchingGroups("BGM")[0];
        bgSound.clip = clip;
        bgSound.loop = true;
        bgSound.volume = 1f;
        bgSound.Play();
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        for (int i = 0; i < bglist.Length; i++)
            if (scene.name == bglist[i].name)
                BgSoundPlay(bglist[i]);
    }
}
