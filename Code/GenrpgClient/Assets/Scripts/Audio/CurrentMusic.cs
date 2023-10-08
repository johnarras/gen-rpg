using System;
using UnityEngine;
using GEntity = UnityEngine.GameObject;

public class CurrentMusic
{
    public AudioClipList clipList;
    public AudioSource source;
    public AudioSource prevSource;
    public PlayAudioData playData;
    public DateTime NextRandomizeTime;


    public float GetRandomIzeSeconds()
    {
        if (playData == null || playData.musicData == null)
        {
            return 0.0f;
        }

        return playData.musicData.RandomizeSeconds;
    }

    public void StopAll()
    {
        if (clipList != null)
        {
            clipList.StopAll(0);
        }
        clipList.IsActiveMusic = false;
    }
}