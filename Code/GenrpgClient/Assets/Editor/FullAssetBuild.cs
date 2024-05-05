using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Genrpg.Shared.Core.Entities;

using UnityEditor;
using Genrpg.Shared.Constants;

namespace GameAssets.Editor
{
	public class FullAssetBuild
	{

        [MenuItem("Build/Dev Full Asset Build")]
        static void ExecuteDev()
        {
            ExecuteEnv(EnvNames.Dev);
        }

        [MenuItem("Build/Prod Full Asset Build")]
        static void ExecuteProd()
        {
            ExecuteEnv(EnvNames.Prod);
        }


        static void ExecuteEnv (string env)
        {
           UnityGameState gs = SetupEditorUnityGameState.Setup(null).GetAwaiter().GetResult();
            
            SetupBundles.SetupAll(gs);
			CreateAssetBundle.BuildAssetBundles(gs);
			UploadAssetBundle.UploadAssetBundles(gs, env);
            
		}
	}
}
