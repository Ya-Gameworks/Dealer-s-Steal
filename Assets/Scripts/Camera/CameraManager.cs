using System;
using System.Collections;
using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : Singleton<CameraManager>
{
    [Header("Flip Rotation Settings")]
    [SerializeField] private float FlipRotationTime = 0.5f;
    [SerializeField] private Transform CameraFlipTargetTransform;
    
    [Header("Cameras")]
    [SerializeField] private CinemachineCamera[] CinemachineCameras;
    private CinemachineCamera CurrentCinemachineCamera;
    
    [Header("Y Damping Settings")]
    [SerializeField] [Range(0f,1f)] private float FallPanAmount = .25f;
    [SerializeField] [Range(0f,1f)] private float FallPanTime = .35f;
    private float NormalYPanAmount;
    public float FallSpeedYDampingChangeTreshold = -15f;
    
    public bool IsLerpingYDamping {get; private set;}
    public bool LerpedFromPlayerFalling { get; set; }
    
    private Coroutine LerpingCoroutine;
    private Coroutine PanCameraCoroutine;
    private CinemachinePositionComposer PositionComposer;
    private Vector3 StartingTrackedObjectOffset;
    
    //--------------------------------------------------------\\
    //Setting the singleton
    //--------------------------------------------------------\\
    protected override void Awake()
    {
        base.Awake();
        Instance = this;
    }
    public static CameraManager Instance { get; private set; }
    //--------------------------------------------------------\\
    //Singleton has been set
    //--------------------------------------------------------\\

    private void Start()
    {
        Setup();
    }

    private bool NullHandler()
    {
        if (CinemachineCameras == null || CinemachineCameras.Length == 0) //Check if there are cameras in the list
        {
            Debug.LogError("The camera list is empty. Make sure you have at least one Cinemachine Camera In the list");
            return true;
        }
        if (CameraFlipTargetTransform == null)
        {
            Debug.LogError("There is nothing to follow as a camera. Please set the camera flip target transform");
            return true;
        }
        if (PlayerMovement.Instance == null)
        {
            Debug.LogError("Make sure there is player in the scene");
            return true;
        }
        if (GameManager.Instance == null)
        {
            Debug.LogError("Make sure there is Game Manager in the scene");
            return true;
        }
        return false;
    }
    private void Setup()
    {
        if (NullHandler()) return;
        
        for (int i = 0; i < CinemachineCameras.Length; i++)
        {
            if (CinemachineCameras[i].enabled)
            {
                CurrentCinemachineCamera = CinemachineCameras[i]; //Set the current camera
                PositionComposer = CurrentCinemachineCamera.GetComponent<CinemachinePositionComposer>(); //Set the position composer
                break;
            }
        }
        //Old Y Damping amount.
        NormalYPanAmount = PositionComposer.Damping.y;
        
        //Set the starting position of the tracked object offset
        StartingTrackedObjectOffset = PositionComposer.TargetOffset;
    }
    
    private void Update()
    {
        if (NullHandler()) return;
        if (PlayerMovement.Instance.RB.linearVelocity.y < FallSpeedYDampingChangeTreshold && !IsLerpingYDamping &&
            !LerpedFromPlayerFalling)
        {
            LerpYDamping(true);
        }
        
        if (PlayerMovement.Instance.RB.linearVelocity.y >= FallSpeedYDampingChangeTreshold && !IsLerpingYDamping && LerpedFromPlayerFalling)
        {
            LerpedFromPlayerFalling = false;
            LerpYDamping(false);
        }
        CameraFlipTargetTransform.position = PlayerMovement.Instance.transform.position;
    }
    
    #region SwapCameras

    public void SwapCamera(CinemachineCamera cameraFromLeft, CinemachineCamera cameraFromRight,
        Vector2 triggerExitDirection)
    {
        if (CurrentCinemachineCamera == cameraFromLeft && triggerExitDirection.x > 0f)
        {
            //Activate the new camera
            cameraFromRight.enabled = true;
            
            //Deactivate the old camera
            cameraFromLeft.enabled = false;
            
            //set the new camera as the current camera
            CurrentCinemachineCamera = cameraFromRight;
            
            //Update positioncomposer
            PositionComposer = CurrentCinemachineCamera.GetComponent<CinemachinePositionComposer>();
        }
        else if (CurrentCinemachineCamera == cameraFromRight && triggerExitDirection.x < 0f)
        {
            //Activate the new camera
            cameraFromLeft.enabled = true;
            
            //Deactivate the old camera
            cameraFromRight.enabled = false;
            
            //set the new camera as the current camera
            CurrentCinemachineCamera = cameraFromLeft;
            
            //Update positioncomposer
            PositionComposer = CurrentCinemachineCamera.GetComponent<CinemachinePositionComposer>();
        }
    }
    #endregion
    
    #region PanCamera
    public void PanCamera(float PanDistance, float PanTime, PanDirection panDirection, bool PanToStartingPos)
    {
        Vector3 EndPos = Vector3.zero;

        if (!PanToStartingPos)
        {
            switch (panDirection)
            {
                case PanDirection.UP:
                    EndPos = Vector3.up;
                    break;
                case PanDirection.DOWN:
                    EndPos = Vector3.down;
                    break;
                case PanDirection.LEFT:
                    EndPos = Vector3.left;
                    break;
                case PanDirection.RIGHT:
                    EndPos = Vector3.right;
                    break;
            }
            EndPos *= PanDistance;
            EndPos += PositionComposer.TargetOffset;
        }
        else
        {
            EndPos = StartingTrackedObjectOffset;
        }
        DOTween.To(() => PositionComposer.TargetOffset, Var => PositionComposer.TargetOffset = Var, EndPos, PanTime).SetEase(Ease.Linear);
    }
#endregion

    #region LerpCamera

    public void LerpYDamping(bool IsPlayerFalling)
    {
        IsLerpingYDamping = true;
        float EndDampAmount = 0f;
  
        if (IsPlayerFalling)
        {
            EndDampAmount = FallPanAmount;
            LerpedFromPlayerFalling = true;
        }
        else
        {
            EndDampAmount = NormalYPanAmount;
        }
        
        DOTween.To(() => PositionComposer.Damping.y, Var =>  PositionComposer.Damping.y = Var, EndDampAmount, FallPanTime).SetEase(Ease.Linear);
        IsLerpingYDamping = false;
    }
   
      #endregion
    
    #region TurnPlayerCamera
      public void TurnPlayerCamera()
      {
          CameraFlipTargetTransform.DORotateQuaternion(PlayerMovement.Instance.transform.localRotation, FlipRotationTime).SetEase(Ease.InOutSine);
      }
      #endregion
}
