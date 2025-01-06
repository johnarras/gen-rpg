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
        bool Save<T>(T looperData, long slotId) where T : BasePlayerData, INamedUpdateData;
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

        public T LoadSlot<T>(long slotId) where T : BasePlayerData, INamedUpdateData
        {

            if (!OkSlotId(slotId))
            {
                return null;
            }

            T looperData = _repoService.Load<T>(slotId.ToString()).Result;

            if (looperData == null)
            {
                return null;
            }

            UpdateCurrentSaveSlot(slotId);

            return looperData;
        }

        public bool Save<T>(T looperData, long slotId) where T : BasePlayerData, INamedUpdateData
        {
            if (!OkSlotId(slotId))
            {
                return false;
            }

            looperData.Id = slotId.ToString();

            _repoService.Save(looperData);

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
