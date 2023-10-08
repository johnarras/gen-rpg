using System;
using System.Collections.Generic;
using UnityEngine; // Needed

public class AudioClipList : BaseBehaviour
{

    public bool KeepLoaded;
    public List<AudioClip> Clips;
    public bool IsActiveMusic { get; set; }

    // Not set in editor because it might be different depending on how we do overrides and such
    public string Name { get; set; }

    protected List<AudioSource> _sources = new List<AudioSource>();
    protected DateTime lastPlayTime = DateTime.UtcNow;

    private bool _isValid = false;
    public bool IsValid()
    {
        if (_isValid)
        {
            return true;
        }

        if (Clips == null || Clips.Count < 1)
        {
            return false;
        }

        for (int c = 0; c < Clips.Count; c++)
        {
            if (Clips[c] == null)
            {
                _isValid = false;
                return false;
            }
        }
        _isValid = true;
        return true;
    }


    public double SecondsSinceLastPlay()
    {
        if (_sources.Count > 0)
        {
            return 0;
        }

        return (DateTime.UtcNow - lastPlayTime).TotalSeconds;
    }

    public AudioSource Play(PlayAudioData playData, int index = -1)
    {
        lastPlayTime = DateTime.UtcNow;
        if (!_isValid || playData == null)
        {
            return null;
        }

        AudioClip clip = null;
        if (index < 0 || index >= Clips.Count)
        {
            clip = Clips[_gs.rand.Next() % Clips.Count];
        }
        else
        {
            clip = Clips[index];
        }

        if (playData.parent == null)
        {
            playData.parent = entity;
        }

        if (clip == null)
        {
            return null;
        }

        AudioSource source = playData.parent.AddComponent<AudioSource>();
        source.clip = clip;
        source.loop = playData.looping;
        source.volume = playData.volume;
        if (playData.musicData != null)
        {
            source.spatialBlend = 0;
            source.minDistance = 100000;
            source.maxDistance = 200000;
        }
        else
        {
            source.spatialBlend = 1;
            source.minDistance = 10;
            source.maxDistance = 50;
        }

        if (!source.loop)
        {
            source.PlayOneShot(source.clip, source.volume);
        }
        else
        {
            source.volume = playData.volume;
            source.Play();
        }
        _sources.Add(source);
        return source;
    }


    public void StopSource(AudioSource source)
    {
        if (source == null || !_sources.Contains(source))
        {
            return;
        }

        _sources.Remove(source);
        GEntityUtils.Destroy(source);
    }
     

    public void StopAll(float fadeTime = 0.0f)
    {
        List<AudioSource> sources = new List<AudioSource>(_sources);
        foreach (AudioSource source in sources)
        {
            StopSource(source);
        }      
    }

    public bool IsEqual (AudioClipList other)
    {
        if (Clips == null || other == null || other.Clips == null || Clips.Count < 1 || Clips.Count != other.Clips.Count)
        {
            return false;
        }

        for (int c = 0; c < Clips.Count; c++)
        {
            if (Clips[c] == null || other.Clips[c] == null || Clips[c].name != other.Clips[c].name)
            {
                return false;
            }
        }

        return true;

    }

    

     

    public bool ShouldRemoveAudio()
    {
        // If we have sources, see if any were deleted or ended and remove them.
        if (_sources.Count > 0)
        {
            for (int s = 0; s < _sources.Count; s++)
            {
                AudioSource source = _sources[s];

                if (source == null)
                {
                    _sources.RemoveAt(s);
                    s--;
                }

                if (!source.isPlaying)
                {
                    source.clip = null;
                    GEntityUtils.Destroy(source);
                    _sources.Remove(source);
                    s--;
                }
            }
            if (_sources.Count < 1)
            {
                lastPlayTime = DateTime.UtcNow;
            }
            return false;
        }
        else if (!KeepLoaded && !IsActiveMusic && 
            (DateTime.UtcNow-lastPlayTime).TotalSeconds > 10)
        {
            return true;
        }
        return false;
    }

    public void Clear()
    {
        if (Clips != null)
        {
            foreach (AudioClip clip in Clips)
            {
                if (clip != null)
                {
                    AssetUtils.UnloadAsset(clip);
                }
            }
        }
        Clips = new List<AudioClip>();
    }

}