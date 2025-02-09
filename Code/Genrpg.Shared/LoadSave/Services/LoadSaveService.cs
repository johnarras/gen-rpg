using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.LoadSave.Constants;
using Genrpg.Shared.LoadSave.PlayerData;

namespace Genrpg.Shared.LoadSave.Services
{
    public interface ILoadSaveService : IInjectable
    {
        T ContinueGame<T>() where T : BasePlayerData, INamedUpdateData;
        T LoadSlot<T>(long slotId) where T : BasePlayerData, INamedUpdateData;
        bool OkSlotId(long slotId);
        bool Save<T>(T playerData, long slotId) where T : BasePlayerData, INamedUpdateData;
        bool HaveCurrentGame<T>() where T : BasePlayerData, INamedUpdateData;
        bool HaveSaveGame<T>(long slotId) where T : BasePlayerData, INamedUpdateData;
        void Delete<T>(long slotId) where T : BasePlayerData, INamedUpdateData;
    }

    public class LoadSaveService : ILoadSaveService
    {
        private IRepositoryService _repoService;
        public T ContinueGame<T>() where T : BasePlayerData, INamedUpdateData
        {
            SaveSlotData slotData = _repoService.Load<SaveSlotData>(SaveSlotData.Filename).Result;

            if (slotData == null)
            {
                return null;
            }

            return LoadSlot<T>(slotData.SlotId);
        }

        public bool OkSlotId(long slotId)
        {
            return slotId >= LoadSaveConstants.MinSlot && slotId <= LoadSaveConstants.MaxSlot;
        }

        private string GetFilenameFromSlot<T>(long slotId)
        {
            return typeof(T).Name + slotId;
        }

        public T LoadSlot<T>(long slotId) where T : BasePlayerData, INamedUpdateData
        {

            if (!OkSlotId(slotId))
            {
                return null;
            }

            T playerData = _repoService.Load<T>(GetFilenameFromSlot<T>(slotId)).Result;

            if (playerData == null)
            {
                return null;
            }

            UpdateCurrentSaveSlot(slotId);

            return playerData;
        }

        public bool Save<T>(T playerData, long slotId) where T : BasePlayerData, INamedUpdateData
        {
            if (!OkSlotId(slotId))
            {
                return false;
            }

            playerData.Id = GetFilenameFromSlot<T>(slotId);

            _repoService.Save(playerData);

            UpdateCurrentSaveSlot(slotId);

            return true;
        }

        public void Delete<T>(long slotId) where T : BasePlayerData, INamedUpdateData
        {
            T data = LoadSlot<T>(slotId);
            if (data != null)
            {
                _repoService.Delete<T>(data);
            }
        }

        public bool HaveCurrentGame<T>() where T : BasePlayerData, INamedUpdateData
        {
            return ContinueGame<T>() != null;
        }

        private void UpdateCurrentSaveSlot(long slotId)
        {
            if (!OkSlotId(slotId))
            {
                return;
            }

            SaveSlotData slotData = _repoService.Load<SaveSlotData>(SaveSlotData.Filename).Result;

            if (slotData == null)
            {
                slotData = new SaveSlotData() { Id = SaveSlotData.Filename };
            }

            slotData.SlotId = slotId;
            _repoService.Save(slotData).Wait();
        }

        public bool HaveSaveGame<T>(long slotId) where T : BasePlayerData, INamedUpdateData
        {
            return LoadSlot<T>(slotId) != null;
        }
    }
}
