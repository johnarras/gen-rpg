
using System;
using UnityEngine;

using Genrpg.Shared.Core.Entities;


using Services;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Units.Entities;

public class PlayerObject
{
	private static GameObject _player = null;
    private static Unit _playerUnit = null;
    private static PlayerController _playerController = null;



	public static GameObject Get ()
	{
		return _player;
	}

    public static Unit GetUnit()
    {
        return _playerUnit;
    }

    public static void Destroy()
    {
        GameObject currObject = _player;
        Set(null);
        if (currObject != null)
        {
            GameObject.Destroy(currObject);
        }
    }

	public static void Set (GameObject go)
	{
		_player = go;
        _playerController = null;
        _playerUnit = null;
        if (go != null)
        {
            _playerController = go.GetComponent<PlayerController>();
            if (_playerController != null)
            {
                _playerUnit = _playerController.GetUnit();
                ClientUnitUtils.UpdateMapPosition(_playerController,_playerController.GetUnit());
            }
        }
	}


    public static void MoveAboveObstacles()
    {
        GameObject obj = Get();
        if (obj == null)
        {
            return;
        }

        RaycastHit[] hits = Physics.RaycastAll(obj.transform.position + Vector3.up * 500, Vector3.down);

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject.name.IndexOf("Water") >= 0)
            {
                if (hit.transform.position.y > obj.transform.position.y)
                {
                    obj.transform.position = new Vector3(obj.transform.position.x, hit.transform.position.y + 1,
                        obj.transform.position.z);
                }
            }
        }
        obj.transform.position += new Vector3(0, 10, 0);
    }
}