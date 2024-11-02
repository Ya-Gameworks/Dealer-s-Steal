using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManager : Singleton<PlayerInputManager>
{
 
    //--------------------------------------------------------\\
    //Setting the singleton
    //--------------------------------------------------------\\
    protected override void Awake()
    {
        base.Awake();
        Instance = this;
        SetPlayerInput();
    }

    public static PlayerInputManager Instance { get; private set; }
    //--------------------------------------------------------\\
    //Singleton has been set
    //--------------------------------------------------------\\
    
    private PlayerInputs playerInputs;

    private void SetPlayerInput()
    {
        playerInputs = new PlayerInputs();
    }
    
    private void OnEnable()
    {
        playerInputs.Enable();
    
        //Enable Computer Controlls
        playerInputs.Player.WallGrab.started += OnWallGrab;
        playerInputs.Player.WallGrab.canceled += OnWallGrabReleased;
        
        playerInputs.Player.Walk.started += OnPlayerWalkPressed;
        playerInputs.Player.Walk.canceled += OnPlayerWalkPressed;
        
        playerInputs.Player.Look.started += OnPlayerLookPressed;
        playerInputs.Player.Look.canceled += OnPlayerLookPressed;
        
        playerInputs.Player.Jump.performed += OnJumpPressed;
        playerInputs.Player.Jump.canceled += OnJumpReleased;
        
        playerInputs.Player.Dash.started += OnDashPressed;
        
        playerInputs.Player.Combat.started += OnCombatPressed;

        playerInputs.Player.ChipThrow.started += OnChipHoldPressed;
        playerInputs.Player.ChipThrow.canceled += OnChipHoldRelased;
    }
    
    private void OnDisable()
    {
        playerInputs.Disable();
        
        //Disable Computer Controlls
        playerInputs.Player.WallGrab.started -= OnWallGrab;
        playerInputs.Player.WallGrab.canceled -= OnWallGrabReleased;
        
        playerInputs.Player.Walk.started -= OnPlayerWalkPressed;
        playerInputs.Player.Walk.canceled -= OnPlayerWalkPressed;
        
        playerInputs.Player.Look.started -= OnPlayerLookPressed;
        playerInputs.Player.Look.canceled -= OnPlayerLookPressed;
        
        playerInputs.Player.Jump.performed -= OnJumpPressed;
        playerInputs.Player.Jump.canceled -= OnJumpReleased;
        
        playerInputs.Player.Dash.started -= OnDashPressed;
        
        playerInputs.Player.Combat.started -= OnCombatPressed;
        
        playerInputs.Player.ChipThrow.started -= OnChipHoldPressed;
        playerInputs.Player.ChipThrow.canceled -= OnChipHoldRelased;
    }

    private void OnPlayerWalkPressed(InputAction.CallbackContext ctx)
    {
        PlayerMovement.Instance.OnMoveInput(ctx.ReadValue<float>());
    }
    
    private void OnPlayerLookPressed(InputAction.CallbackContext ctx)
    {
        PlayerMovement.Instance.OnLookInput(ctx.ReadValue<float>());
    }

    private void OnDashPressed(InputAction.CallbackContext ctx)
    {
        PlayerMovement.Instance.OnDashInput();
    }

    private void OnCombatPressed(InputAction.CallbackContext ctx)
    {
        PlayerMovement.Instance.OnCombatInput();
    }

    private void OnJumpPressed(InputAction.CallbackContext ctx)
    {
        PlayerMovement.Instance.OnJumpInput();
    }
    
    private void OnJumpReleased(InputAction.CallbackContext ctx)
    {
        PlayerMovement.Instance.OnJumpInputRelease();
    }

    private void OnWallGrab(InputAction.CallbackContext ctx)
    {
     PlayerMovement.Instance.OnWallGrabInput();   
    }
    
    private void OnWallGrabReleased(InputAction.CallbackContext ctx)
    {
        PlayerMovement.Instance.OnWallGrabInputRelease();   
    }

    private void OnChipHoldPressed(InputAction.CallbackContext ctx)
    {
        PlayerMovement.Instance.OnChipHoldInput();
    }
    private void OnChipHoldRelased(InputAction.CallbackContext ctx)
    {
        PlayerMovement.Instance.OnChipHoldReleaseInput();
    }
}

