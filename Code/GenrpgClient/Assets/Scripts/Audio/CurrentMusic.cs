using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Genrpg.Shared.Core.Entities;

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