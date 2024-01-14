using Cysharp.Threading.Tasks;
using Genrpg.Shared.Pathfinding.Constants;
using Genrpg.Shared.Pathfinding.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Pathfinding.Utils
{
    public static class ClientPathfindingUtils
    {
        public static async void ShowPath(UnityGameState _gs, WaypointList list)
        {

            IMapTerrainManager _terrainManager = _gs.loc.Get<IMapTerrainManager>();
            StringBuilder sb = new StringBuilder();
            if (true || list.Waypoints.Count > 2)
            {
                List<Waypoint> dupeList = new List<Waypoint>(list.Waypoints);
                List<GameObject> pathObjects = new List<GameObject>();

                GameObject basePathSphere = Resources.Load<GameObject>("Prefabs/PathSphere");

                foreach (Waypoint wp in dupeList)
                {
                    GameObject sph = GameObject.Instantiate<GameObject>(basePathSphere);
                    float height = _terrainManager.SampleHeight(_gs, wp.X, wp.Z);
                    sph.transform.position = new Vector3(wp.X, height + 0.5f, wp.Z);
                    sb.Append("WP: " + wp.X + " " + wp.Z + "\n");
                    pathObjects.Add(sph);
                    await UniTask.NextFrame();
                }

                _gs.logger.Info("Path:\n" + sb.ToString());
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
