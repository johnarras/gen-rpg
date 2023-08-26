using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Genrpg.Shared.Core.Entities;


public class ClutterObject : BaseBehaviour
{
    private void OnEnable()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.mass = 0f;
            rb.drag = 0;
            rb.angularDrag = 1000;
            rb.useGravity = true;
        }
        StartCoroutine(DelayFullTurnOff());
    }
    
    private IEnumerator DelayFullTurnOff()
    {
        yield return new WaitForSeconds(5.0f);
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
        StartCoroutine(DelayTurnOffPhysics());
    }

    private IEnumerator DelayTurnOffPhysics()
    {

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            yield break;
        }

        
        if (!didFinalSetPos)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        yield return new WaitForSeconds(5.0f);

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

        Rigidbody rb = GetComponent<Rigidbody>();
        GameObject.Destroy(rb);
        Vector3 normal = _gs.md.GetInterpolatedNormal(_gs, _gs.map, transform.position.x, transform.position.z);

        if (_gs.md.GetSteepness(_gs, transform.position.x, transform.position.z) > AddClutter.MaxSteepness)
        {
            GameObject.Destroy(gameObject);
            return;
        }
        Quaternion groundTilt = Quaternion.FromToRotation(Vector3.up, normal);

        //transform.rotation *= groundTilt;

        int index = (int)(transform.position.x * 131 + transform.position.y * 139 + transform.position.z * 511);



        //var nrot = new Vector3(((index * 17) % 4) * 90, ((index * 31) % 4) * 90 , ((index * 67) % 4) * 90);
        //transform.Rotate(nrot);

        int newAngle = (index * 413) % 360;

        if (collidedNonTerrain)
        {
            transform.Rotate(normal, newAngle);
        }
        transform.position -= normal * (2+(index * 13) % 8) * 0.05f;
    }
}
