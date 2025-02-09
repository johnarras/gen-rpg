using Assets.Scripts.Crawler.Constants;
using Assets.Scripts.Interfaces;
using Genrpg.Shared.Accounts.PlayerData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Dungeons
{
    [Serializable]
    public class DungeonAsset : BaseBehaviour
    {
        private IAudioService _audioService;
        public Animator Animator;

        public List<MeshRenderer> Renderers = new List<MeshRenderer>();

        public List<MeshRenderer> DoorRenderers = new List<MeshRenderer>();


        public bool SetOpened(bool isOpen)
        {
            if (DoorRenderers.Count > 0)
            {
                if (isOpen)
                {
                    _audioService.PlaySound(CrawlerAudio.DoorOpen, null);
                }
                foreach (MeshRenderer renderer in DoorRenderers)
                {
                    _clientEntityService.SetActive(renderer.gameObject, !isOpen);
                }
                return true;
            }
            return false;
        }
    }
}
