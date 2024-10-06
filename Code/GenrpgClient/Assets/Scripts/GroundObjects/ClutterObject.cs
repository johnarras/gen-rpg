using Genrpg.Shared.MapServer.Services;
using System.Threading;
using UnityEngine; // Needed

public class ClutterObject : BaseBehaviour
{
    IMapTerrainManager _terrainManager;
    protected IMapProvider _mapProvider;

    public override void Init()
    {
        base.Init();
        AddDelayedUpdate(FullTurnOff, 0.7f);
    }

    private void FullTurnOff(CancellationToken token)
    {
        FinalSetPos();
    }

    bool collidedNonTerrain = false;
    bool collidedTerrain = false;
    private void OnCollisionEnter(Collision collision)
    {
        if (collidedTerrain)
        {
            return;
        }

        TerrainCollider terrainCollider = collision.gameObject.GetComponent<TerrainCollider>();

        if (terrainCollider != null)
        {
            collidedTerrain = true;
            FinalSetPos();
            return;
        }
        collidedNonTerrain = true;
        AddDelayedUpdate(DelayTurnOffPhysics, 2.0f);
    }

    private void DelayTurnOffPhysics(CancellationToken token)
    {
        FinalSetPos();
    }


    bool didFinalSetPos = false;
    private void FinalSetPos()
    {
        if (didFinalSetPos)
        {
            return;
        }

        didFinalSetPos = true;

        Vector3 normal = _terrainManager.GetInterpolatedNormal(_mapProvider.GetMap(),entity.transform.position.x,entity.transform.position.z);

        Quaternion groundTilt = Quaternion.FromToRotation(Vector3.up, normal);

        int index = (int)(entity.transform.position.x * 131 +entity.transform.position.y * 139 +entity.transform.position.z * 511);

        int newAngle = (index * 413) % 360;

        if (collidedNonTerrain)
        {
            entity.transform.Rotate(normal, newAngle);
        }
       entity.transform.position -= normal * (2 + (index * 13) % 8) * 0.05f;
    }
}
