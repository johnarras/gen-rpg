using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;

namespace Assets.Scripts.Interfaces
{
    public interface IAudioService : IInitializable
    {
        void PlaySound(string name, object parent = null, float volume = 1.0f);
        void PlayMusic(IMusicRegion region);
        void StopAllAudio(IUnityGameState gs);

        void SetSoundActive(bool val);
        bool IsSoundActive(IUnityGameState gs);

        void SetMusicActive(bool val);
        bool IsMusicActive(IUnityGameState gs);
    }
}
