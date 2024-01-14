using Genrpg.Shared.Buildings.Settings;
using Genrpg.Shared.MapObjects.Messages;
using Genrpg.Shared.Spawns.WorldData;

namespace Assets.Scripts.Buildings
{
    public class StoreSign : BaseBehaviour
    {
        public GText SignText;

        private BuildingType _btype;
        private OnSpawn _spawn;
        public void Init(BuildingType btype, OnSpawn spawn)
        {
            _btype = btype;
            _spawn = spawn;

            _uiService.SetText(SignText, _btype.Name);
        }
    }
}
