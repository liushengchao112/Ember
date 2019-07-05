using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using Resource;
using Utils;
using DG.Tweening;
using Data;

public class SoundManager : MonoBehaviour
{
    private static SoundManager instance;

    public static SoundManager Instance
    {
        get
        {
            return instance;
        }
    }

    private AudioSource musicAudioSource;

    private List<AudioSource> unusedSoundAudioSourceList;

    private List<AudioSource> usedSoundAudioSourceList;

    private Dictionary<int, AudioClip> audioClipDict;

    private Dictionary<int, AudioSource> aloneSoundPlayingDict;

    private float musicVolume = 1;

    private float soundVolume = 1;

    private int storeCount = 3;

    void Awake()
    {
        instance = this;

        musicAudioSource = gameObject.AddComponent<AudioSource>();
        unusedSoundAudioSourceList = new List<AudioSource>();
        usedSoundAudioSourceList = new List<AudioSource>();
        audioClipDict = new Dictionary<int, AudioClip>();
        aloneSoundPlayingDict = new Dictionary<int, AudioSource>();

        MessageDispatcher.AddObserver( OnMusicVolumeChanged, Constants.MessageType.MusicVolume );
        MessageDispatcher.AddObserver( OnSoundVolumeChanged, Constants.MessageType.SoundVolume );
    }

    void Destroy()
    {
        MessageDispatcher.RemoveObserver( OnMusicVolumeChanged, Constants.MessageType.MusicVolume );
        MessageDispatcher.RemoveObserver( OnSoundVolumeChanged, Constants.MessageType.SoundVolume );
    }

    public void PlayMusic( int id, bool loop = true )
    {
        //DOTween.To( () => musicAudioSource.volume, value => musicAudioSource.volume = value, 0, 0.5f ).OnComplete(()=>
        //{
        //    musicAudioSource.clip = GetAudioClip( id );
        //    musicAudioSource.clip.LoadAudioData();
        //    musicAudioSource.loop = loop;
        //    musicAudioSource.volume = musicVolume;
        //    musicAudioSource.Play();
        //    DOTween.To( () => musicAudioSource.volume, value => musicAudioSource.volume = value, musicVolume, 0.5f );
        //} );
        musicAudioSource.clip = GetAudioClip( id );
        musicAudioSource.clip.LoadAudioData();
        musicAudioSource.loop = loop;
        musicAudioSource.volume = musicVolume;
        musicAudioSource.Play();
        //DOTween.To( () => musicAudioSource.volume, value => musicAudioSource.volume = value, musicVolume, 0.5f );

        DebugUtils.Log( DebugUtils.Type.Sound, string.Format( "play sound {0} {1}", id, musicAudioSource.clip.name ) );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="isAlone"> If this sound alone?</param>
    /// <param name="action"> Call back when this sound play end</param>
    public void PlaySound( int id, bool isAlone = false, Action<int> action = null )
    {
        if ( isAlone )
        {
            if ( aloneSoundPlayingDict.ContainsKey(id) )
            {
                AudioSource audioSource = aloneSoundPlayingDict[id];
                audioSource.Play();

                StartCoroutine( WaitPlaySoundEnd( id, audioSource, action ) );
            }
            else
            {
                if ( unusedSoundAudioSourceList.Count != 0 )
                {
                    AudioSource audioSource = UnusedToUsed();
                    audioSource.clip = GetAudioClip( id );
                    if ( audioSource.clip != null )
                    {
                        audioSource.clip.LoadAudioData();
                        audioSource.Play();

                        aloneSoundPlayingDict.Add( id, audioSource );

                        StartCoroutine( WaitPlaySoundEnd( id, audioSource, action ) );
                    }
                }
                else
                {
                    AddAudioSource();

                    AudioSource audioSource = UnusedToUsed();
                    audioSource.clip = GetAudioClip( id );
                    if ( audioSource.clip != null )
                    {
                        audioSource.clip.LoadAudioData();
                        audioSource.volume = soundVolume;
                        audioSource.loop = false;
                        audioSource.Play();

                        aloneSoundPlayingDict.Add( id, audioSource );

                        StartCoroutine( WaitPlaySoundEnd( id, audioSource, action ) );
                    }
                }
            }
        }
        else
        {

            if ( unusedSoundAudioSourceList.Count != 0 )
            {
                AudioSource audioSource = UnusedToUsed();
                audioSource.clip = GetAudioClip( id );
                if ( audioSource.clip != null )
                {
                    audioSource.clip.LoadAudioData();
                    audioSource.Play();

                    StartCoroutine( WaitPlaySoundEnd( id, audioSource, action ) );
                }
            }
            else
            {
                AddAudioSource();

                AudioSource audioSource = UnusedToUsed();
                audioSource.clip = GetAudioClip( id );
                if ( audioSource.clip != null )
                {
                    audioSource.clip.LoadAudioData();
                    audioSource.volume = soundVolume;
                    audioSource.loop = false;
                    audioSource.Play();

                    StartCoroutine( WaitPlaySoundEnd( id, audioSource, action ) );
                }
            }
        }
    }

    public void Play3dSound( int id, Vector3 position )
    {
        AudioClip ac = GameResourceLoadManager.GetInstance().LoadAsset<AudioClip>( id );
        AudioSource.PlayClipAtPoint( ac, position );
    }

    IEnumerator WaitPlaySoundEnd( int id, AudioSource audioSource, Action<int> action = null )
    {
        yield return new WaitUntil( () => { return !audioSource.isPlaying; } );

        UsedToUnused( audioSource );

        if ( aloneSoundPlayingDict.ContainsKey( id ) )
        {
            aloneSoundPlayingDict.Remove( id );
        }
        if ( action != null )
        {
            action( id );
        }
    }

    private string GetAudioPath( int id )
    {
        ResourcesProto.Resources resourcesProto = DataManager.GetInstance().resourcesProtoData.Find( p => p.ID == id );

        if ( resourcesProto != null )
        {
            return resourcesProto.ResourcePath;
        }
        return string.Empty;
    }

    private AudioClip GetAudioClip( int id )
    {
        if ( !audioClipDict.ContainsKey(id) )
        {
            AudioClip ac = GameResourceLoadManager.GetInstance().LoadAsset<AudioClip>( id );
            audioClipDict.Add( id, ac );
        }
        return audioClipDict[id];
    }

    private AudioSource AddAudioSource()
    {
        if ( unusedSoundAudioSourceList.Count != 0 )
        {
            return UnusedToUsed();
        }
        else
        {
            AudioSource audioSource = gameObject.AddComponent<AudioSource>();
            unusedSoundAudioSourceList.Add( audioSource );
            return audioSource;
        }
    }
    
    private AudioSource UnusedToUsed()
    {
        AudioSource audioSource = unusedSoundAudioSourceList[0];
        unusedSoundAudioSourceList.RemoveAt( 0 );
        usedSoundAudioSourceList.Add( audioSource );
        return audioSource;
    }

    private void UsedToUnused( AudioSource audioSource )
    {
        if ( usedSoundAudioSourceList.Contains( audioSource ) )
        {
            usedSoundAudioSourceList.Remove( audioSource );
        }


        unusedSoundAudioSourceList.Add( audioSource );
    }

    void OnMusicVolumeChanged( object obj )
    {
        float volume = (float)obj;
        musicVolume = volume;
        musicAudioSource.volume = volume;

        DebugUtils.Log( DebugUtils.Type.Sound, string.Format( "Modified music volume to {0}", volume ) );
    }

    void OnSoundVolumeChanged( object obj )
    {
        float volume = (float)obj;
        soundVolume = volume;
        for ( int i = 0; i < unusedSoundAudioSourceList.Count; i++ )
        {
            unusedSoundAudioSourceList[i].volume = volume;
        }
        for ( int i = 0; i < usedSoundAudioSourceList.Count; i++ )
        {
            usedSoundAudioSourceList[i].volume = volume;
        }

        DebugUtils.Log( DebugUtils.Type.Sound, string.Format( "Modified sound volume to {0}", volume ) );

    }
}
