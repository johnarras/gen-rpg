using Genrpg.Editor.Entities.Core;
using Genrpg.ServerShared.PlayerData;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Characters.Utils;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.Users.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Genrpg.Editor.Utils
{
    public static class EditorPlayerUtils
    {
        public static void ClearEditorUserData(EditorGameState gs)
        {
            gs.EditorUser = new EditorUser();
        }

        public static async Task LoadEditorUserData(EditorGameState gs, IRepositoryService repoService, long userId)
        {        

            gs.EditorUser.User = await repoService.Load<User>(userId.ToString());

            List<CharacterStub> charStubs = await gs.loc.Get<IPlayerDataService>().LoadCharacterStubs(gs, userId.ToString());

            foreach (CharacterStub stub in charStubs)
            {
                CoreCharacter coreChar = await repoService.Load<CoreCharacter>(stub.Id);
                if (coreChar != null)
                {
                    Character ch = new Character();
                    CharacterUtils.CopyDataFromTo(coreChar, ch);

                    EditorCharacter ech = new EditorCharacter() { Character = ch, CoreCharacter = coreChar };
                    gs.EditorUser.Characters.Add(ech);
                    await gs.loc.Get<IPlayerDataService>().LoadAllPlayerData(gs, ch);
                    foreach (IUnitData dataCont in ch.GetAllData().Values)
                    {
                        ech.Data.Add(new EditorUnitData() { Data = dataCont });
                    }
                }
            }
        }

        public static async Task SaveEditorUserData(EditorGameState gs, IRepositoryService repoService)
        {
            if (gs.LookedAtObjects.Contains(gs.EditorUser.User))
            {
                await repoService.Save(gs.EditorUser.User);
            }
            if (gs.EditorUser.Characters != null)
            {
                foreach (EditorCharacter ech in gs.EditorUser.Characters)
                {
                    if (gs.LookedAtObjects.Contains(ech.Character))
                    {
                        await repoService.Save(ech.CoreCharacter);
                    }
                    foreach (IUnitData unitData in ech.Character.GetAllData().Values)
                    {
                        if (gs.LookedAtObjects.Contains(unitData))
                        {
                            unitData.Save(repoService, false);
                        }
                    }
                }
            }
        }

        public static async Task DeleteEditorUserData(EditorGameState gs, IRepositoryService repoService)
        {

            await repoService.Delete(gs.EditorUser.User);

            if (gs.EditorUser.Characters != null)
            {
                foreach (EditorCharacter ech in gs.EditorUser.Characters)
                {
                    await repoService.Delete(ech.CoreCharacter);
                    foreach (IUnitData unitData in ech.Character.GetAllData().Values)
                    {
                        unitData.Delete(repoService);
                    }
                }
            }
        }
    }
}
