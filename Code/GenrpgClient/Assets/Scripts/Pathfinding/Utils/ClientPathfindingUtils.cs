using Assets.Scripts.Tokens;

using Genrpg.Shared.Interfaces;
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
    public interface IClientPathfindingUtils : IMapTokenService, IInjectable
    {
        Awaitable ShowPath(WaypointList list, CancellationToken token);
        CancellationToken GetToken();
    }

    public class ClientPathfindingUtils : IClientPathfindingUtils
    {

        private bool _showPath = false;
        private IMapTerrainManager _terrainManager;
        private IAssetService _assetService;
        private CancellationToken _token;

        public void SetMapToken(CancellationToken token)
        {
            _token = token;
        }

        public CancellationToken GetToken()
        {
            return _token;
        }

        public async Awaitable ShowPath(WaypointList list, CancellationToken token)
        {
            
            StringBuilder sb = new StringBuilder();
            if (_showPath)
            {
                List<Waypoint> dupeList = new List<Waypoint>(list.Waypoints);
                List<GameObject> pathObjects = new List<GameObject>();

                GameObject basePathSphere = await _assetService.LoadAssetAsync(AssetCategoryNames.Prefabs, "PathSphere", null, token);

                foreach (Waypoint wp in dupeList)
                {
                    GameObject sph = GameObject.Instantiate<GameObject>(basePathSphere);
                    float height = _terrainManager.SampleHeight(wp.X, wp.Z);
                    sph.transform.position = new Vector3(wp.X, height + 0.5f, wp.Z);
                    //sb.Append("WP: " + wp.X + " " + wp.Z + "\n");
                    pathObjects.Add(sph);
                    await Awaitable.NextFrameAsync();
                }

                //logService.Info("Path:\n" + sb.ToString());
                AwaitableUtils.ForgetAwaitable(DelayDestroyObjects(pathObjects));
            }
        }




        private async Awaitable DelayDestroyObjects(List<GameObject> objs)
        {
            await Awaitable.WaitForSecondsAsync(1.0f);
            foreach (GameObject obj in objs)
            {
                GameObject.Destroy(obj);
            }
        }
    }
}
