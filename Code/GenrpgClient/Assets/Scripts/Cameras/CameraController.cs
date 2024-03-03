using System;
using System.Collections.Generic;
using GEntity = UnityEngine.GameObject;
using Genrpg.Shared.Constants;
using Genrpg.Shared.Utils;
using UnityEngine; // Needed
using Genrpg.Shared.Interfaces;

[Serializable]
public class CullDistanceOverride
{
    public string LayerName;
    public float CullDistance;
}

public interface ICameraController : IService
{
    Camera GetMainCamera();
    GEntity GetCameraParent();
    void BeforeMoveUpdate();
    void AfterMoveUpdate();
    List<Camera> GetAllCameras();
}

public class CameraController : BaseBehaviour, ICameraController
{

    private IInputService _inputService;
    private IMapTerrainManager _terrainManager;

    public List<CullDistanceOverride> LayerCullDistanceOverrides;

	private const float CameraDistScale = 1.2f; // 1.5f?
	private static GVector3 StartCameraOffset = new GVector3(0,2.0f,-10.0f)*CameraDistScale;
	private const float StartCameraDistance = 4.125f*CameraDistScale;

	protected static float CameraHeightAboveGroundTarget = 2.0f;
	
	private static GVector3 CameraOffset = new GVector3 (0,0.95f,-4.0f)*CameraDistScale;
    private static GVector3 OldCameraOffset = CameraOffset;
	private static GVector3 CamPos = GVector3.zero;
	private static GVector3 LookAtPos = GVector3.zero;

	private bool _lockCameraPosition = false;
	private float LockedCameraAngle { get; set; }
	
	private static float CameraDistance = 0.0f;
	
	private const float minHeightAboveTerrain = 2.0f;
	
	private const float MinCameraDistance = 10f;
	private const float MaxCameraDistance = 20;
	private const float CameraZoomSpeed = 0.2f;
	private const float CameraDistanceDelta = 5.0f;
	private const float CameraZoomScale = 1.1f;
	private const float CameraMoveScale = 1.5f;

    private int _onStairsTicks = 0;


    public Camera MainCam = null;
    public GEntity CameraParent = null;

    public List<Camera> Cameras;

    GEntity _dragParent = null;

    public List<Camera> GetAllCameras() { return Cameras; }

    public override void Initialize(UnityGameState gs)
    {
        base.Initialize(gs);
        gs.loc.Set<ICameraController>(this);
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

    public GEntity GetCameraParent()
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
            MainCam.layerCullSpherical = true;
        }

	}

	protected bool button0 = false;
	protected bool button1 = false;
	protected GVector3 prevPos = GVector3.zero;


	protected IScreenService screenService = null;

	protected static Camera cam = null;

	
	
	protected float moveScale = 0.0f;
	protected GVector3 currPos = GVector3.zero;
	protected GVector3 diffPos = GVector3.zero;
    protected int mouseDownTicks = 0;

	protected GEntity player = null;

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
	private const float MaxPanMagnitude = 15.0f;



    GVector3 beforeCampos = GVector3.zero;
    GVector3 beforeLookat = GVector3.zero;
    GVector3 beforeUp = GVector3.zero;
    GVector3 beforeRight = GVector3.zero;

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
    GVector3 beforeDiffPos = GVector3.zero;
    float beforeAngle = 0;
    float beforeCurrAngle = 0;
    float beforeDiffAngle = 0;
    Quaternion beforeRot = GQuaternion.identity;

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


        List<ActiveScreen> screens = _screenService.GetAllScreens(_gs);

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


        player = PlayerObject.Get();

        if (CheckSwitchPlayerObject())
        {
            return;
        }

        if (player == null)
        {
            return;
        }

        if (_lockCameraPosition)
        {
            return;
        }

        if (mouseDownTicks > 0)
        {
            diffPos = currPos - prevPos;

            diffPos = diffPos * 0.12f;
            currPos = prevPos + diffPos;


            beforeCampos = CameraOffset;
            beforeUp = GVector3.up;
            beforeLookat = beforeCampos;
            beforeLookat /= beforeLookat.magnitude;
            beforeRight = GVector3.Cross(beforeLookat, beforeUp);

            beforeUp = GVector3.Cross(beforeRight, beforeLookat);
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

                beforeCampos = new GVector3(beforePosX, beforePosY, beforePosZ);

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

                    beforeDiffPos = GVector3.Create(player.transform().position -entity.transform().position);

                    beforeAngle = (float)(Math.Atan2(beforeDiffPos.z, -beforeDiffPos.x) * 180f / Math.PI-90);

                    beforeCurrAngle = player.transform().localRotation.eulerAngles.y;


                    beforeDiffAngle = beforeAngle - beforeCurrAngle;

                    beforeRot = Quaternion.Euler(GVector3.Create(0, beforeDiffAngle, 0));

                    player.transform().localRotation *= beforeRot;

                    CameraOffset = GQuaternion.MultVector(Quaternion.Inverse(beforeRot), CameraOffset);
                   
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

    GVector3 lookAtPos = GVector3.zero;
    GVector3 camPos2 = GVector3.zero;
    GVector3 lookPos3 = GVector3.zero;
    float desiredLookAngle = 0;
    Quaternion lookAtRotation = GQuaternion.identity;
    GVector3 cameraOffset = GVector3.zero;

    GVector3 playerPosition = GVector3.zero;
    float terrainHeightAtPlayer = 0;

    float camDist = 0;
    float lookRatio = 0;
    float terrainHeightAtCamera = 0;
    float camYPos = 0;
    float currMinHeightAboveTerrain = 0;

    GEntity objHit;

    int camCollideLayerMask = 0;

    bool didHit = false;

    public void UpdateCamera()
	{
	    player = PlayerObject.Get();
		if (player == null || _gs.md == null || _gs.md.GeneratingMap || MainCam == null)
		{
			return;
		}

        lookAtPos = GVector3.Create(player.transform().position)+ new GVector3 (0,CameraHeightAboveGroundTarget,0);

		if (_lockCameraPosition)
		{
			lookAtPos = LookAtPos;
		}

        desiredLookAngle = player.transform().eulerAngles.y;

		if (_lockCameraPosition)
		{
			desiredLookAngle = LockedCameraAngle;
		}
		else
		{
			LockedCameraAngle = desiredLookAngle;
		}

		lookAtRotation = Quaternion.Euler (0, desiredLookAngle, 0);
			
		cameraOffset = GVector3.zero;

        CameraDistance = MathUtils.Clamp(MinCameraDistance, CameraDistance, MaxCameraDistance);
			
		if (CameraDistance != 0)
		{
			lookRatio = CameraDistance/CameraOffset.magnitude;
			CameraOffset = CameraOffset*lookRatio;
		}
		cameraOffset = CameraOffset;
        MoveCamera(lookAtPos + GQuaternion.MultVector(lookAtRotation,cameraOffset), lookAtPos);

        camPos2 = GVector3.Create(MainCam.transform().position);
		terrainHeightAtCamera = _terrainManager.SampleHeight(_gs, camPos2.x, camPos2.z);

        camDist = GVector3.Distance(lookAtPos, GVector3.Create(MainCam.transform().position))+1.0f;
        camYPos = MainCam.transform().position.y;

        didHit = GPhysics.Raycast(lookAtPos, GVector3.Create(MainCam.transform().position) - lookAtPos,
                    out objHit, camDist, camCollideLayerMask);

        playerPosition = GVector3.Create(player.transform().position);
        terrainHeightAtPlayer = _terrainManager.SampleHeight(_gs, playerPosition.x, playerPosition.z);

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
                CameraOffset += new GVector3(0, lowDiff, 0);
            }
        }
        else if (didHit && objHit != null)
        {
            float newDist = GVector3.Distance(LookAtPos, GVector3.Create(objHit.transform().position)) - 1.0f;
            if (newDist < MinCameraDistance)
            {
                newDist = MinCameraDistance;
            }

            CameraOffset *= newDist / CameraOffset.magnitude;
            CameraDistance = CameraOffset.magnitude;
            targetCameraDistance = CameraDistance;
        }
        CamPos = GVector3.Create(MainCam.transform().position);
        LookAtPos = lookAtPos;

    }

    Camera lookCam = null;
    public void MoveCamera(GVector3 camPos, GVector3 lookAtPos)
    {
        for (int c =0; c < Cameras.Count; c++)
        {
            lookCam = Cameras[c];
            lookCam.transform().position = GVector3.Create(camPos);
            lookCam.transform().LookAt(GVector3.Create(lookAtPos));
        }
    }

    protected bool AreDragging()
    {
        if (_dragParent == null)
        {
            _dragParent = _screenService.GetDragParent() as GEntity;
        }

        if (_dragParent == null)
        {
            return false;
        }

        return _dragParent.transform().childCount > 0;
    }


}

