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
        void PlaySound(UnityGameState gs, string name, object parent = null, float volume = 1.0f);
        void PlayMusic(UnityGameState gs, IMusicRegion region);
        void StopAllAudio(UnityGameState gs);

        void SetSoundActive(UnityGameState gs, bool val);
        bool IsSoundActive(UnityGameState gs);

        void SetMusicActive(UnityGameState gs, bool val);
        bool IsMusicActive(UnityGameState gs);
    }
}
