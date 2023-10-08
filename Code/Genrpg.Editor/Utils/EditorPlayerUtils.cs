using Genrpg.Editor.Entities.Core;
using Genrpg.ServerShared.PlayerData;
using Genrpg.Shared.Characters.Entities;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Users.Entities;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.Editor.Utils
{
    public static class EditorPlayerUtils
    {
        public static void ClearEditorUserData(EditorGameState gs)
        {
            gs.EditorUser = new EditorUser();
        }

        public static async Task LoadEditorUserData(EditorGameState gs, long userId)
        {

            gs.EditorUser.User = await gs.repo.Load<User>(userId.ToString());

            List<CharacterStub> charStubs = await PlayerDataUtils.LoadCharacterStubs(gs, userId.ToString());

            foreach (CharacterStub stub in charStubs)
            {
                Character ch = await gs.repo.Load<Character>(stub.Id);
                if (ch != null)
                {
                    EditorCharacter ech = new EditorCharacter() { Character = ch };
                    gs.EditorUser.Characters.Add(ech);
                    await PlayerDataUtils.LoadPlayerData(gs, ch);
                    foreach (IUnitData dataCont in ch.GetAllData().Values)
                    {
                        ech.Data.Add(new EditorUnitData() { Data = dataCont });
                    }
                }
            }
        }

        public static async Task SaveEditorUserData(EditorGameState gs)
        {
            if (true || gs.LookedAtObjects.Contains(gs.EditorUser.User))
            {
                await gs.repo.Save(gs.EditorUser.User);
            }
            if (true || gs.EditorUser.Characters != null)
            {
                foreach (EditorCharacter ech in gs.EditorUser.Characters)
                {
                    if (true ||gs.LookedAtObjects.Contains(ech.Character))
                    {
                        await gs.repo.Save(ech.Character);
                    }
                    foreach (IUnitData unitData in ech.Character.GetAllData().Values)
                    {
                        if (true || gs.LookedAtObjects.Contains(unitData))
                        {
                            unitData.Save(gs.repo, true);
                        }
                    }
                }
            }
        }

        public static async Task DeleteEditorUserData(EditorGameState gs)
        {

            await gs.repo.Delete(gs.EditorUser.User);

            if (gs.EditorUser.Characters != null)
            {
                foreach (EditorCharacter ech in gs.EditorUser.Characters)
                {
                    await gs.repo.Delete(ech.Character);
                    foreach (IUnitData unitData in ech.Character.GetAllData().Values)
                    {
                        unitData.Delete(gs.repo);
                    }
                }
            }
        }
    }
}
