using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;


public class AnimatorEnabledChecker
{
	//[MenuItem("Scratchers/DisabledAnimatorsCheck")]
	public static void CheckForDisabledAnimators()
	{
        object extraData = new System.Object();
        StringBuilder logTxt = new StringBuilder();
		PrefabChecker.CheckData(extraData,logTxt,CheckAnimators);
	}



	public static void CheckAnimators (GameObject go, string path, System.Object extraData, StringBuilder logTxt)
	{

		if (go == null || logTxt == null)
		{
			return;
		}

        Animator[] animators = go.GetComponents<Animator>();
		if (animators != null)
		{
			foreach (Animator anim in animators)
			{
				if (!anim.enabled)
				{
					logTxt.Append("Disabled animator on " + path + "\n");
				}
			}
		}
	}

}

