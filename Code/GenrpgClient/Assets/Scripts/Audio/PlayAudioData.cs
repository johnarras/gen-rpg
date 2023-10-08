
using GEntity = UnityEngine.GameObject;
using Genrpg.Shared.Audio.Entities;
using UnityEngine; // Needed

public class PlayAudioData
{
    public MusicType musicData;
    public string audioName;
    public float volume;
    public GEntity parent;
    public AudioCategory category;
    public bool looping;
    public AudioSource source;
}
