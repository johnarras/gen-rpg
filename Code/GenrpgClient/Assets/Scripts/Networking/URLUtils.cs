
using Genrpg.Shared.Core.Entities;
using System;
using UnityEngine;

public class URLUtils
{
	public static void OpenURL (UnityGameState gs, string url, bool forceNewWindow = true)
	{
		if (string.IsNullOrEmpty(url))
        {
            return;
        }

        Application.OpenURL(url);
	}
}

