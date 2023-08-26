using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class ProximityController : BaseBehaviour
{
    public GameObject ChildObject;

    public float MaxDistanceToPlayer = 30.0f;
    public float CheckInterval = 1.0f;

    private void Start()
    {
        StartCoroutine(CheckForPlayerNear());
    }

    private IEnumerator CheckForPlayerNear()
    {
        if (CheckInterval < 0.1f)
        {
            CheckInterval = 0.1f;
        }

        while (true)
        {
            GameObject go = PlayerObject.Get();
            if (go != null)
            {
                float dist = Vector3.Distance(go.transform.position, transform.position);
                if (dist < MaxDistanceToPlayer)
                {
                    GameObjectUtils.SetActive(ChildObject, true);
                }
                else
                {
                    GameObjectUtils.SetActive(ChildObject, false);
                }
            }                
            yield return new WaitForSeconds(3.9f);
        }
    }
}