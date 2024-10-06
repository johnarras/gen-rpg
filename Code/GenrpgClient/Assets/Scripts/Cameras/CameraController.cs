using System;
using System.Collections.Generic;
using Genrpg.Shared.Constants;
using Genrpg.Shared.Utils;
using UnityEngine; // Needed
using Genrpg.Shared.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using Assets.Scripts.Core.Interfaces;
using Genrpg.Shared.UI.Services;
using Genrpg.Shared.UI.Entities;

[Serializable]
public class CullDistanceOverride
{
    public string LayerName;
    public float CullDistance;
}

public interface ICameraController : IInitializable
{
    Camera GetMainCamera();
    GameObject GetCameraParent();
    void BeforeMoveUpdate();
    void AfterMoveUpdate();
    List<Camera> GetAllCameras();
    void SetupForBoardGame();
}

public class CameraController : BaseBehaviour, ICameraController, IInjectOnLoad<ICameraController>, IInitOnResolve
{

    private IInputService _inputService;
    private IMapTerrainManager _terrainManager;
    private IPlayerManager _playerManager;
    protected IMapGenData _md;


    public async Task Initialize(CancellationToken token)
    {
        await Task.CompletedTask;
    }


    public List<CullDistanceOverride> LayerCullDistanceOverrides;

	private const float CameraDistScale = 1.2f; // 1.5f?
	private Vector3 StartCameraOffset = new Vector3(0,2.0f,-10.0f)*CameraDistScale;
	private const float StartCameraDistance = 4.125f*CameraDistScale;

	protected float CameraHeightAboveGroundTarget = 2.0f;
	
	private Vector3 CameraOffset = new Vector3 (0,0.95f,-4.0f)*CameraDistScale;
    private Vector3 OldCameraOffset = new Vector3(0, 0.95f, -4.0f) * CameraDistScale;
    private Vector3 CamPos = Vector3.zero;
	private Vector3 LookAtPos = Vector3.zero;

	private bool _lockCameraPosition = false;
    private bool _lockOffset = false;
	private float LockedCameraAngle { get; set; }
	
	private float CameraDistance = 0.0f;
	
	private const float minHeightAboveTerrain = 2.0f;
	
	private  float MinCameraDistance = 10f;
	private float MaxCameraDistance = 20;
	private float CameraZoomSpeed = 0.2f;
	private float CameraZoomScale = 1.1f;
	private float CameraMoveScale = 1.5f;

    private int _onStairsTicks = 0;


    public Camera MainCam = null;
    public GameObject CameraParent = null;

    public List<Camera> Cameras;

    GameObject _dragParent = null;

    public List<Camera> GetAllCameras() { return Cameras; }

    public override void Init()
    {
        CameraDistance = StartCameraOffset.magnitude;
        SetupCullDistances();
        int layerMask = LayerUtils.GetMask(new string[] { LayerNames.Default, LayerNames.Water, LayerNames.ObjectLayer, LayerNames.UnitLayer, LayerNames.SpellLayer });

        camCollideLayerMask = LayerUtils.GetMask(new string[] { LayerNames.Default, LayerNames.ObjectLayer });

        foreach (Camera cam in Cameras)
        {
            cam.enabled = true;
            cam.cullingMask = layerMask;
        }
    }


    public Camera GetMainCamera()
    {
        return MainCam;
    }

    public GameObject GetCameraParent()
    {
        return CameraParent;
    }

    int _maxDistance = 100;

    

    public void SetupCullDistances(int maxDistance = 0)
    {

        if (_maxDistance == maxDistance)
        {
            return;
        }

        _maxDistance = maxDistance;

        if (MainCam != null && LayerCullDistanceOverrides != null)
        {

            float[] cullDistances = new float[32];

            // Camera is set up to render objects on the map at the proper distances, but the water and terrain need more distance.
            foreach (CullDistanceOverride dist in LayerCullDistanceOverrides)
            {
                int layerId = LayerUtils.NameToLayer(dist.LayerName);
                if (layerId >= 0 && layerId < 32 && dist.CullDistance > 0)
                {
                    cullDistances[layerId] = (maxDistance > 0 ? maxDistance : dist.CullDistance);
                }
               
            }

            MainCam.layerCullDistances = cullDistances;
        }

	}

    private Vector3 FixedOffset = Vector3.zero;

    public void SetupForBoardGame()
    {
        MainCam.layerCullDistances = new float[32];
        CameraDistance = 25;
        CameraOffset = new Vector3(-CameraDistance, CameraDistance, -CameraDistance);
        FixedOffset = CameraOffset;
        MinCameraDistance = CameraDistance;
        MaxCameraDistance = CameraDistance;
        _lockOffset = true;
    }

	protected bool button0 = false;
	protected bool button1 = false;
	protected Vector3 prevPos = Vector3.zero;


	protected IScreenService screenService = null;

	protected Camera cam = null;

	
	
	protected float moveScale = 0.0f;
	protected Vector3 currPos = Vector3.zero;
	protected Vector3 diffPos = Vector3.zero;
    protected int mouseDownTicks = 0;

	protected GameObject player = null;

	protected float targetCameraDistance = 0.0f;

    float scrollWheel = 0;
    float oldDist = 0;
    float newDist = 0;
    float dz = 0;
    float newSpeed = 0;
    
	public virtual bool CheckSwitchPlayerObject()
	{
		return false;
	}

    public virtual void SetIsOnStairsNow(bool value)
    {
        if (value)
        {
            _onStairsTicks = 10;
        }
        else
        {
            _onStairsTicks = 0;
        }
    }

    Vector3 beforeCampos = Vector3.zero;
    Vector3 beforeLookat = Vector3.zero;
    Vector3 beforeUp = Vector3.zero;
    Vector3 beforeRight = Vector3.zero;

    float beforeDiffX = 0;
    float beforeDiffY = 0;
    float beforeXmult = 1;
    float beforePosX = 0;
    float beforePosY = 0;
    float beforePosZ = 0;
    float beforeGroundPlaneDist = 0;
    float beforeNorm = 0;
    float beforeDistScale = 0;

    float beforeMaxHeightMult = 10.0f;
    Vector3 beforeDiffPos = Vector3.zero;
    float beforeAngle = 0;
    float beforeCurrAngle = 0;
    float beforeDiffAngle = 0;
    Quaternion beforeRot = Quaternion.identity;

    public void BeforeMoveUpdate()
    {
        if (AreDragging())
        {
            return;
        }

        if (_onStairsTicks > 0)
        {
            _onStairsTicks--;
        }

        if (_gs.user == null)
        {
            return;
        }

        CameraHeightAboveGroundTarget = MathUtils.Clamp(-1, CameraHeightAboveGroundTarget, 10);

        List<ActiveScreen> screens = _screenService.GetAllScreens();

        foreach (ActiveScreen scrn in screens)
        {
            if (scrn.Screen != null && scrn.Screen.BlockMouse())
            {
                UpdateCamera();
                return;
            }
        }

        moveScale = CameraMoveScale * _inputService.GetDeltaTime();
        currPos = _inputService.MousePosition();
        diffPos = currPos - prevPos;
        scrollWheel = -Input.GetAxis("Mouse ScrollWheel");

        if (targetCameraDistance <= 0.0f)
        {
            targetCameraDistance = CameraDistance;
        }

        if (scrollWheel != 0)
        {
            if (scrollWheel < 0 && targetCameraDistance > CameraDistance)
            {
                targetCameraDistance = CameraDistance;
            }
            if (scrollWheel > 0 && targetCameraDistance < CameraDistance)
            {
                targetCameraDistance = CameraDistance;
            }

            if (scrollWheel > 0)
            {
                targetCameraDistance *= CameraZoomScale;
            }
            else if (scrollWheel < 0)
            {
                targetCameraDistance /= CameraZoomScale;
            }


            targetCameraDistance = MathUtils.Clamp(MinCameraDistance, targetCameraDistance,
                                                    MaxCameraDistance);
        }


        oldDist = CameraDistance;
        newDist = oldDist;
        dz = targetCameraDistance - oldDist;

        newSpeed = Math.Min(CameraZoomSpeed, Math.Abs(dz / 8));

        if (oldDist < targetCameraDistance)
        {
            newDist += newSpeed;
        }
        else
        {
            newDist -= newSpeed;
        }

        if (scrollWheel != 0 || button0 || button1)
        {
            CameraDistance = newDist;
        }

        if (button0 || button1)
        {
            mouseDownTicks++;
        }
        else
        {
            mouseDownTicks = 0;
        }
        button0 = _inputService.MouseIsDown(0);
        button1 = _inputService.MouseIsDown(1);

        player = _playerManager.GetPlayerGameObject();

        if (CheckSwitchPlayerObject())
        {
            return;
        }

        if (player == null)
        {
            return;
        }
        if (_lockOffset)
        {
            CamPos = player.transform.position - FixedOffset;
            MainCam.transform.position = CamPos;
            MainCam.transform.LookAt(player.transform.position);
            return;
        }

        if (_lockCameraPosition)
        {
            return;
        }

        if (mouseDownTicks > 0 && !_lockOffset)
        {
            diffPos = currPos - prevPos;

            diffPos = diffPos * 0.12f;
            currPos = prevPos + diffPos;


            beforeCampos = CameraOffset;
            beforeUp = Vector3.up;
            beforeLookat = beforeCampos;
            beforeLookat /= beforeLookat.magnitude;
            beforeRight = Vector3.Cross(beforeLookat, beforeUp);

            beforeUp = Vector3.Cross(beforeRight, beforeLookat);
            beforeUp /= beforeUp.magnitude;

            beforeDiffX = RescaleMouseMoveDelta(diffPos.x);
            beforeDiffY = RescaleMouseMoveDelta(diffPos.y);

            if (button1 || button0)
            {
                beforeXmult = 1;

                beforeCampos += (beforeUp * -beforeDiffY + beforeRight * -beforeDiffX * beforeXmult) * moveScale;
                beforePosX = beforeCampos.x;
                beforePosY = beforeCampos.y;
                beforePosZ = beforeCampos.z;
                beforeGroundPlaneDist = (float)Math.Sqrt(beforePosX * beforePosX + beforePosZ * beforePosZ);

                beforeMaxHeightMult = 10.0f;
                if (beforeGroundPlaneDist < Math.Abs(beforePosY) / beforeMaxHeightMult)
                {
                    beforePosY = beforeMaxHeightMult * beforeGroundPlaneDist * Math.Sign(beforePosY);
                }

                beforeCampos = new Vector3(beforePosX, beforePosY, beforePosZ);

                if (beforeGroundPlaneDist < Math.Abs(beforePosY) * 0.05)
                {
                    beforePosY = beforeGroundPlaneDist * 0.1f * Math.Sign(beforePosY);
                }

                beforeNorm = beforeCampos.magnitude;
                if (beforeNorm < 0.01)
                {
                    beforeNorm = 0.01f;
                }
                if (CameraDistance <= 0.0f)
                {
                    CameraDistance = beforeNorm;
                }
                beforeDistScale = CameraDistance / beforeNorm;
                beforeCampos *= beforeDistScale;
                OldCameraOffset = CameraOffset;
                CameraOffset = beforeCampos;
                

                // Rorate the player to face where the camera is facing and undo the
                // rotation so the camera keeps facing the same direction
                if (button1)
                {

                    beforeDiffPos = player.transform.position -entity.transform.position;

                    beforeAngle = (float)(Math.Atan2(beforeDiffPos.z, -beforeDiffPos.x) * 180f / Math.PI-90);

                    beforeCurrAngle = player.transform.localRotation.eulerAngles.y;


                    beforeDiffAngle = beforeAngle - beforeCurrAngle;

                    beforeRot = Quaternion.Euler(new Vector3(0, beforeDiffAngle, 0));

                    player.transform.localRotation *= beforeRot;

                    CameraOffset = Quaternion.Inverse(beforeRot) * CameraOffset;
                   
                }

            }
        }
        prevPos = currPos;
    }

    public void AfterMoveUpdate()
    {
        if (AreDragging())
        {
            return;
        }

        UpdateCamera();
	}

	public float RescaleMouseMoveDelta(float delta)
	{

        return delta*Math.Max(0.5f, CameraDistance)*0.50f;
	}

    Vector3 lookAtPos = Vector3.zero;
    Vector3 camPos2 = Vector3.zero;
    Vector3 lookPos3 = Vector3.zero;
    float desiredLookAngle = 0;
    Quaternion lookAtRotation = Quaternion.identity;
    Vector3 cameraOffset = Vector3.zero;

    Vector3 playerPosition = Vector3.zero;
    float terrainHeightAtPlayer = 0;

    float camDist = 0;
    float lookRatio = 0;
    float terrainHeightAtCamera = 0;
    float camYPos = 0;
    float currMinHeightAboveTerrain = 0;

    RaycastHit objHit;

    int camCollideLayerMask = 0;

    bool didHit = false;

    public void UpdateCamera()
	{
	    player = _playerManager.GetPlayerGameObject();
		if (player == null || _md.GeneratingMap || MainCam == null)
		{
			return;
		}

        lookAtPos = player.transform.position + new Vector3 (0,CameraHeightAboveGroundTarget,0);

		if (_lockCameraPosition)
		{
			lookAtPos = LookAtPos;
		}

        desiredLookAngle = player.transform.eulerAngles.y;

		if (_lockCameraPosition)
		{
			desiredLookAngle = LockedCameraAngle;
		}
		else
		{
			LockedCameraAngle = desiredLookAngle;
		}

		lookAtRotation = Quaternion.Euler (0, desiredLookAngle, 0);
			
		cameraOffset = Vector3.zero;

        CameraDistance = MathUtils.Clamp(MinCameraDistance, CameraDistance, MaxCameraDistance);
			
		if (CameraDistance != 0)
		{
			lookRatio = CameraDistance/CameraOffset.magnitude;
			CameraOffset = CameraOffset*lookRatio;
		}
		cameraOffset = CameraOffset;
        MoveCamera(lookAtPos + lookAtRotation*cameraOffset, lookAtPos);

        camPos2 = MainCam.transform.position;
		terrainHeightAtCamera = _terrainManager.SampleHeight(camPos2.x, camPos2.z);

        camDist = Vector3.Distance(lookAtPos, MainCam.transform.position)+1.0f;
        camYPos = MainCam.transform.position.y;

        didHit = Physics.Raycast(lookAtPos, MainCam.transform.position - lookAtPos,
                    out objHit, camDist, camCollideLayerMask);

        playerPosition = player.transform.position;
        terrainHeightAtPlayer = _terrainManager.SampleHeight(playerPosition.x, playerPosition.z);

        currMinHeightAboveTerrain = minHeightAboveTerrain;


        if (camDist < currMinHeightAboveTerrain)
        {
            currMinHeightAboveTerrain = camDist;
        }

        if (playerPosition.y >= terrainHeightAtPlayer &&
            camYPos < terrainHeightAtCamera + currMinHeightAboveTerrain)
        {

            float lowDiff = (terrainHeightAtCamera + currMinHeightAboveTerrain - camYPos);
            if (lowDiff > 0 && _onStairsTicks < 1)
            {
                CameraOffset += new Vector3(0, lowDiff, 0);
            }
        }
        else if (didHit && objHit.collider != null)
        {
            float newDist = Vector3.Distance(LookAtPos, objHit.transform.position) - 1.0f;
            if (newDist < MinCameraDistance)
            {
                newDist = MinCameraDistance;
            }

            CameraOffset *= newDist / CameraOffset.magnitude;
            CameraDistance = CameraOffset.magnitude;
            targetCameraDistance = CameraDistance;
        }
        CamPos = MainCam.transform.position;
        LookAtPos = lookAtPos;

    }

    Camera lookCam = null;
    public void MoveCamera(Vector3 camPos, Vector3 lookAtPos)
    {
        for (int c =0; c < Cameras.Count; c++)
        {
            lookCam = Cameras[c];
            lookCam.transform.position = camPos;
            lookCam.transform.LookAt(lookAtPos);
        }
    }

    protected bool AreDragging()
    {
        if (_dragParent == null)
        {
            _dragParent = _screenService.GetDragParent() as GameObject;
        }

        if (_dragParent == null)
        {
            return false;
        }

        return _dragParent.transform.childCount > 0;
    }


}

