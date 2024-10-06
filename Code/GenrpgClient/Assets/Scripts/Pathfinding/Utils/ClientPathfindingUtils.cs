
using Genrpg.Shared.Client.Assets.Constants;
using Genrpg.Shared.Client.Assets.Services;
using Genrpg.Shared.Client.Tokens;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Pathfinding.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

        private bool _showPath = true;
        private IMapTerrainManager _terrainManager;
        private IAssetService _assetService;
        private CancellationToken _token;
        private ILogService _logService;

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
            if (_showPath)
            {

                StringBuilder sb = new StringBuilder();
                sb.Append("ShowPath: " + list.RetvalType + " Size: " + list.Waypoints.Count + "\n");
                List<Waypoint> dupeList = new List<Waypoint>(list.Waypoints);
                List<GameObject> pathObjects = new List<GameObject>();

                GameObject basePathSphere = (GameObject)(await _assetService.LoadAssetAsync(AssetCategoryNames.Prefabs, "PathSphere", null, token));

                foreach (Waypoint wp in dupeList)
                {
                    GameObject sph = GameObject.Instantiate<GameObject>(basePathSphere);
                    float height = _terrainManager.SampleHeight(wp.X, wp.Z);
                    sph.transform.position = new Vector3(wp.X, height + 0.5f, wp.Z);
                    sb.Append("WP: " + wp.X + " " + wp.Z + "\n");
                    pathObjects.Add(sph);
                    await Awaitable.NextFrameAsync();
                }

                _logService.Info("Path:\n" + sb.ToString());
                TaskUtils.ForgetAwaitable(DelayDestroyObjects(pathObjects));
            }
        }




        private async Awaitable DelayDestroyObjects(List<GameObject> objs)
        {
            await Awaitable.WaitForSecondsAsync(4.0f);
            foreach (GameObject obj in objs)
            {
                GameObject.Destroy(obj);
            }
        }
    }
}
