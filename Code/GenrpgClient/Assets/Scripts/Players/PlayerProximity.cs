using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public abstract class PlayerProximity : BaseBehaviour
{
    public float CheckTime = 1.0f;
    public float TriggerDistance = 20;

    abstract protected void TriggerOnPlayerProximity(bool isClose);

    private void Start()
    {
        StartCoroutine(CheckForPlayerNearby());
    }

    private IEnumerator CheckForPlayerNearby()
    {
        if (CheckTime < 0.1f)
        {
            CheckTime = 0.1f;
        }

        bool playerIsClose = false;

        while (true)
        {
            yield return new WaitForSeconds(CheckTime);

            GameObject go = PlayerObject.Get();

            if (go == null)
            {
                continue;
            }

            float dist = Vector3.Distance(go.transform.position, transform.position);

            bool isClose = dist < TriggerDistance;

            if (isClose != playerIsClose)
            {
                TriggerOnPlayerProximity(isClose);
                playerIsClose = isClose;
            }
                
        }
    }
}