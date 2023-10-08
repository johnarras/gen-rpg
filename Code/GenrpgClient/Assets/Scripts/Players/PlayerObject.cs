using GEntity = UnityEngine.GameObject;
using Genrpg.Shared.Units.Entities;
using System.Collections.Generic;

public class PlayerObject
{
	private static GEntity _player = null;
    private static Unit _playerUnit = null;
    private static PlayerController _playerController = null;

	public static GEntity Get ()
	{
		return _player;
	}

    public static Unit GetUnit()
    {
        return _playerUnit;
    }

    public static void Destroy()
    {
        GEntity currObject = _player;
        Set(null);
        if (currObject != null)
        {
            GEntityUtils.Destroy(currObject);
        }
    }

	public static void Set (GEntity go)
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
        GEntity obj = Get();
        if (obj == null)
        {
            return;
        }

        List<GEntity> hits = GPhysics.RaycastAll(GVector3.Create(obj.transform().position) + GVector3.up * 500, GVector3.down);

        foreach (GEntity hit in hits)
        {
            if (hit.name.IndexOf("Water") >= 0)
            {
                if (hit.transform().position.y > obj.transform().position.y)
                {
                    obj.transform().position = GVector3.Create(obj.transform().position.x, hit.transform().position.y + 1,
                        obj.transform().position.z);
                }
            }
        }
        obj.transform().position += GVector3.Create(0, 10, 0);
    }
}