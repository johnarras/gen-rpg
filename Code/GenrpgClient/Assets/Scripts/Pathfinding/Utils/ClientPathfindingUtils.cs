using Cysharp.Threading.Tasks;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Pathfinding.Constants;
using Genrpg.Shared.Pathfinding.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Pathfinding.Utils
{
    public static class ClientPathfindingUtils
    {

        public static async UniTask ShowPath(UnityGameState _gs, WaypointList list, CancellationToken token)
        {

            IMapTerrainManager _terrainManager = _gs.loc.Get<IMapTerrainManager>();
            IAssetService _assetService = _gs.loc.Get<IAssetService>();
            ILogService logService = _gs.loc.Get<ILogService>();
            StringBuilder sb = new StringBuilder();
            if (true || list.Waypoints.Count > 2)
            {
                List<Waypoint> dupeList = new List<Waypoint>(list.Waypoints);
                List<GameObject> pathObjects = new List<GameObject>();

                GameObject basePathSphere = await _assetService.LoadAssetAsync(_gs, AssetCategoryNames.Prefabs, "PathSphere", null, token);

                foreach (Waypoint wp in dupeList)
                {
                    GameObject sph = GameObject.Instantiate<GameObject>(basePathSphere);
                    float height = _terrainManager.SampleHeight(_gs, wp.X, wp.Z);
                    sph.transform.position = new Vector3(wp.X, height + 0.5f, wp.Z);
                    sb.Append("WP: " + wp.X + " " + wp.Z + "\n");
                    pathObjects.Add(sph);
                    await UniTask.NextFrame();
                }

                logService.Info("Path:\n" + sb.ToString());
                DelayDestroyObjects(pathObjects).Forget();
            }
        }




        private static async UniTask DelayDestroyObjects(List<GameObject> objs)
        {
            await UniTask.Delay(3000);
            foreach (GameObject obj in objs)
            {
                GameObject.Destroy(obj);
            }
        }
    }
}
