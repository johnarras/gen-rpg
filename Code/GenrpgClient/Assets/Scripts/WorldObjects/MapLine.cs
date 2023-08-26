using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Genrpg.Shared.Core.Entities;

using Services;
using UnityEngine;
using Entities;
using Genrpg.Shared.Spells.Entities;
using System.Threading;

public class MapLine : BaseBehaviour
{
    public LineRenderer Renderer;

    public List<GameObject> Anchors;

    public Vector3 Offset = Vector3.zero;


    DateTime endTime;

    private CancellationToken _token;
    public void Init(UnityGameState gs, LineRenderer rend, List<GameObject> anchors, float duration, Vector3 offset,
        CancellationToken token)
    {
        _token = token;
        Renderer = rend;
        Anchors = anchors;
        endTime = DateTime.UtcNow.AddSeconds(duration);
        Offset = offset;
        AddUpdate(LineUpdate, UpdateType.Regular);
    }

    private void LineUpdate()
    {
        if (Renderer == null || Anchors == null || Anchors.Count < 2)
        {
            GameObject.Destroy(gameObject);
            return;
        }

        if (DateTime.UtcNow >= endTime)
        {
            GameObject.Destroy(gameObject);
            return;
        }

        for (int i = 0; i < Anchors.Count; i++)
        {
            if (Anchors[i] == null)
            {
                GameObject.Destroy(gameObject);
                return;
            }
            Renderer.SetPosition(i, Anchors[i].transform.position + Offset);
        }

    }
}
