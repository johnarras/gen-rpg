using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Assets.Bundles
{
    public class BundleCacheItem : MonoBehaviour
    {

        private BundleCacheData _cacheData;
        public void RegisterCache(BundleCacheData data)
        {
            _cacheData = data;
        }


        private void OnDestroy()
        {
            if (_cacheData != null)
            {
                _cacheData.Instances.Remove(gameObject);
                _cacheData.LastUsed = DateTime.UtcNow;
            }
        }
    }
}
