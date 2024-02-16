using GEntity = UnityEngine.GameObject;
using System;
using System.Linq;
using System.Collections.Generic;

using Assets.Scripts.Interfaces;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Users.Entities;
using System.Threading;
using Cysharp.Threading.Tasks;
using Assets.Scripts.Tokens;
using UnityEngine; // Needed
using Genrpg.Shared.Audio.Settings;

public class UnityAudioService : BaseBehaviour, IAudioService, IGameTokenService
{
    public const float MusicVolumeScale = 0.3f;
    public List<MusicChannel> MusicChannels;

    private Dictionary<string, AudioClipList> _audioCache = new Dictionary<string, AudioClipList>();

    protected CancellationToken _token;


    public void SetGameToken(CancellationToken token)
    {
        _token = token;
        CheckRemoveAudio(_token).Forget();
    }

    public override void Initialize(UnityGameState gs)
    {
        base.Initialize(gs);
        AddUpdate(AudioUpdate, UpdateType.Regular);
        _gs.loc.Set<IAudioService>(this);
        if (MusicChannels == null)
        {
            MusicChannels = new List<MusicChannel>();
        }
    }

    void AudioUpdate()
    {
        UpdateMusic();
    }

    #region ToggleAudio
    public void StopAllAudio(UnityGameState gs)
    {
        foreach (AudioClipList cont in _audioCache.Values)
        {
            cont.StopAll();
        }
    }

    public void SetMusicActive(UnityGameState gs, bool val)
    {
        SetAudioActive(gs, UserFlags.MusicActive, val);
    }

    public void SetSoundActive(UnityGameState gs, bool val)
    {
        SetAudioActive(gs, UserFlags.SoundActive, val);
    }

    public bool IsMusicActive(UnityGameState gs)
    {
        return IsAudioActive(gs, UserFlags.MusicActive);
    }

    public bool IsSoundActive(UnityGameState gs)
    {
        return IsAudioActive(gs, UserFlags.SoundActive);
    }

    protected void SetAudioActive(UnityGameState gs, int flag, bool val)
    {
        gs.UpdateUserFlags(flag, val);
    }

    protected bool IsAudioActive(UnityGameState gs, int flag)
    {
        if (gs.user == null)
        {
            InitialClientConfig config = gs.GetConfig();
            if (config != null)
            {
                return FlagUtils.IsSet(config.UserFlags, flag);
            }
            return false;
        }
        return gs.user.HasFlag(flag);
    }
    #endregion


    private async UniTask CheckRemoveAudio(CancellationToken token)
    {
        List<AudioClipList> _removeList = null;
        while (true)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(1.1f), cancellationToken: token);

            foreach (AudioClipList cont in _audioCache.Values)
            { 
                if (cont.ShouldRemoveAudio())
                {
                    if (_removeList == null)
                    {
                        _removeList = new List<AudioClipList>();
                    }

                    _removeList.Add(cont);
                }
            }

            if (_removeList != null && _removeList.Count > 0)
            {
                foreach (AudioClipList cont in _removeList)
                {
                    if (cont == null)
                    {
                        continue;
                    }
                    _audioCache.Remove(cont.Name);
                    cont.Clear();
                    GEntityUtils.Destroy(cont.entity());
                }
            }
            else
            {
                _removeList = null;
            }
        }
    }



    public void PlaySound(UnityGameState gs, string soundName, object parent = null, float volume = 1f)
    {
        PlayAudioData playData = new PlayAudioData()
        {
            audioName = soundName,
            volume = volume,
            parent = parent as GEntity,
            category = AudioCategory.Sound,
            looping = false,
        };
        PlayAudio(gs, playData);
    }


    protected void PlayAudio(UnityGameState gs, PlayAudioData playData)
    {
        if (playData == null)
        {
            return;
        }

        if (_audioCache.ContainsKey(name))
        {
            AudioClipList cont = _audioCache[name];
            PlayLoadedAudio(gs, cont, playData);
            return;
        }



        _assetService.LoadAsset(gs, AssetCategoryNames.Audio, playData.audioName, OnDownloadAudio, playData, entity, _token);
    }

    private void OnDownloadAudio(UnityGameState gs, object obj, object data, CancellationToken token)
    {
        GEntity go = obj as GEntity;
        if (go == null)
        {
            return;
        }

        AudioClipList cont = go.GetComponent<AudioClipList>();
        if (cont == null || !cont.IsValid())
        {
            GEntityUtils.Destroy(go);
            return;
        }

        PlayAudioData playData = data as PlayAudioData;
        if (playData == null || string.IsNullOrEmpty(playData.audioName))
        {
            GEntityUtils.Destroy(go);
            return;
        }

        if (_audioCache.ContainsKey(playData.audioName))
        {
            GEntityUtils.Destroy(go);
            cont = _audioCache[playData.audioName];
                
        }
        else
        {
            _audioCache[playData.audioName] = cont;
            cont.Name = playData.audioName;
        }
        PlayLoadedAudio(gs, cont, playData);
    }


    protected void PlayLoadedAudio(UnityGameState gs, AudioClipList clipList, PlayAudioData playData)
    {
        if (clipList == null || playData == null)
        {
            return;
        }

        AudioSource source = clipList.Play(playData);

        if (playData.musicData != null)
        {
            MusicChannel categoryCont = GetMusicChannel(playData.category);
            CurrentMusic musicCont = new CurrentMusic()
            {                
                playData=playData,
                clipList = clipList,
                source = source,
            };
            clipList.IsActiveMusic = true;
            
           
            SetNewMusic(musicCont);
        }
    }

    #region Music

    protected MusicChannel GetMusicChannel(AudioCategory cat)
    {
        return MusicChannels.FirstOrDefault(x => x.category == cat);
    }



    /// <summary>
    /// Keep list of default Ids for channels to be read in from a music region.
    /// </summary>
    private Dictionary<AudioCategory, long> _channelIds = new Dictionary<AudioCategory, long>();
 
    public void PlayMusic(UnityGameState gs, IMusicRegion region)
    {
        if (gs.data == null || MusicChannels == null)
        {
            return;
        }

        _channelIds[AudioCategory.Music] = 1;
        _channelIds[AudioCategory.Ambient] = 0;
        
        if (region != null)
        {
            _channelIds[AudioCategory.Music] = region.MusicTypeId;
            _channelIds[AudioCategory.Ambient] = region.AmbientMusicTypeId;
        }

        for (int i = 0; i < MusicChannels.Count; i++)
        {
            if (i == (int)AudioCategory.Ambient)
            {
                continue;
            }


            MusicChannel ch = MusicChannels[i];
            long musicId = 0;
            if(_channelIds.ContainsKey(ch.category))
            {
                musicId = _channelIds[ch.category];
            }
            else
            {
                continue;
            }

            MusicType mtype = gs.data.Get<MusicTypeSettings>(gs.ch)?.Get(musicId);

            string musicName = "";
            if (mtype != null)
            {
                musicName = mtype.Art;
            }
            else
            {
                musicName = "IntroMusic";
            }

            if (ch.curr != null && ch.curr.playData != null &&
                ch.curr.playData.audioName == musicName)
            {
                continue;
            }

            FadeOutMusic(ch.category);

            PlayAudioData playData = new PlayAudioData()
            {
                musicData=mtype,
                audioName = musicName,
                volume = ch.Volume,
                category = ch.category,
                looping = ch.Looping,
                parent = entity,
                
            };


            PlayAudio(gs, playData);
        }
    }



    private void FadeOutMusic(AudioCategory cat)
    {
        MusicChannel cont = GetMusicChannel(cat);
        if (cont == null)
        {
            return;
        }

        if (cont.curr != null)
        {
            cont.prevList.Add(cont.curr);
            cont.curr = null;
        }
    }

    private void SetNewMusic (CurrentMusic musicCont)
    {
        if (musicCont == null || musicCont.clipList == null || musicCont.source == null ||
            musicCont.playData == null ||
            string.IsNullOrEmpty(musicCont.playData.audioName))
        {
            return;
        }

        MusicChannel catCont = GetMusicChannel(musicCont.playData.category);

        if (catCont == null)
        {
            return;
        }

        if (catCont.curr != null && catCont.curr.clipList.IsEqual(musicCont.clipList))
        {
            musicCont.clipList.StopSource(musicCont.source);
            return;
        }

        CurrentMusic currentFadingOutMusic = null;

        foreach (CurrentMusic music in catCont.prevList)
        {
            if (music.clipList == musicCont.clipList)
            {
                currentFadingOutMusic = music;
                break;
            }
        }

        CurrentMusic newMusic = null;

        if (currentFadingOutMusic != null)
        {
            musicCont.clipList.StopSource(musicCont.source);
            catCont.prevList.Remove(currentFadingOutMusic);
            newMusic = currentFadingOutMusic;
            
        }
        else
        {
            newMusic = musicCont;
        }

        if (catCont.curr != null)
        {
            catCont.prevList.Add(catCont.curr);
        }
        catCont.curr = newMusic;
        if (catCont.curr != null && catCont.curr.GetRandomIzeSeconds() > 0)
        {
            float randTime = catCont.curr.GetRandomIzeSeconds();
            float newRandTime = MathUtils.FloatRange(randTime / 2, randTime * 3 / 2, _gs.rand);
            catCont.curr.NextRandomizeTime = DateTime.UtcNow.AddSeconds(newRandTime);
        }
    }

    int fadeFrames = 50;
    List<CurrentMusic> removeList = null;
    private void UpdateMusic()
    {
        foreach (MusicChannel cont in MusicChannels)
        {
            removeList = null;
            if (cont.curr != null)
            {
                FadeSourceTo(cont.curr.source, MusicVolumeScale, fadeFrames);
                FadeSourceTo(cont.curr.prevSource, 0.0f, fadeFrames);
            }
            foreach (CurrentMusic music in cont.prevList)
            {
                float volume = FadeSourceTo(music.source, 0, fadeFrames);
                FadeSourceTo(music.prevSource, 0, fadeFrames);
                if (volume <= 0)
                {
                    if (removeList == null)
                    {
                        removeList = new List<CurrentMusic>();
                    }

                    removeList.Add(music);
                }
            }

            if (cont.curr != null && 
                (removeList == null || !removeList.Contains(cont.curr)) &&
                cont.curr.GetRandomIzeSeconds() > 0 && 
                DateTime.UtcNow > cont.curr.NextRandomizeTime)
            {
                cont.ChooseNewRandomSound(_gs);
            }

            if (removeList != null)
            {

                foreach (CurrentMusic item in removeList)
                {
                    item.StopAll();
                    cont.prevList.Remove(item);
                }
            }
        }
    }

    private float FadeSourceTo(AudioSource source, float targetVolume, int fadeFrames)
    {
        if (source == null)
        {
            return 0.0f;
        }

        float deltaIncrement = 1.0f;
        if (fadeFrames > 0)
        {
            deltaIncrement = 1.0f / (fadeFrames + 1);
        }
        if (source.volume > targetVolume)
        {
            source.volume -= deltaIncrement;
            if (source.volume < 0)
            {
                source.volume = 0;
            }
        }
        else if (source.volume < targetVolume)
        {
            source.volume += deltaIncrement;
            if (source.volume > targetVolume)
            {
                source.volume = targetVolume;
            }
        }
        return source.volume;
    }
    #endregion

}

