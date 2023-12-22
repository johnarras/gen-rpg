
using GEntity = UnityEngine.GameObject;
using UnityEngine; // Needed
using Genrpg.Shared.Audio.Settings;

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
