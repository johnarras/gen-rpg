using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Genrpg.Shared.Core.Entities;

using UnityEditor;
using Genrpg.Shared.Constants;
using Genrpg.Shared.Client.Core;

namespace GameAssets.Editor
{
	public class FullAssetBuild
	{

        [MenuItem("Tools/Dev Full Asset Build")]
        static void ExecuteDev()
        {
            ExecuteEnv(EnvNames.Dev);
        }

        [MenuItem("Tools/Prod Full Asset Build")]
        static void ExecuteProd()
        {
            ExecuteEnv(EnvNames.Prod);
        }


        static void ExecuteEnv (string env)
        {
           IClientGameState gs = SetupEditorUnityGameState.Setup(null).GetAwaiter().GetResult();
            
            SetupBundles.SetupAll(gs);
			CreateAssetBundle.BuildAssetBundles(gs);
			UploadAssetBundle.UploadAssetBundles(env);
            
		}
	}
}
