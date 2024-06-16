using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Assets.Scripts.Interfaces
{
    public interface IAudioService : IInitializable
    {
        void PlaySound(string name, object parent = null, float volume = 1.0f);
        void PlayMusic(IMusicRegion region);
        void StopAllAudio();

        void SetSoundActive(bool val);
        bool IsSoundActive();

        void SetMusicActive(bool val);
        bool IsMusicActive();
    }
}
