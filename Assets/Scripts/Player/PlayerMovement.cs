using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class PlayerMovement : Singleton<PlayerMovement>
{
    public Animator animator;

    //Scriptable object which holds all the player's movement parameters.
    public PlayerData PlayerData;

    #region EDITOR METHODS
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(GroundCheckPoint.position, GroundCheckSize);
        Gizmos.DrawWireCube(FrontWallCheckPoint.position, WallCheckSize);
        Gizmos.DrawWireCube(BackWallCheckPoint.position, WallCheckSize);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(TopCombatCheck.position, CombatCheckSize);
        Gizmos.DrawWireCube(FrontCombatCheck.position, CombatCheckSize);
        Gizmos.DrawWireCube(BottomCombatCheck.position, CombatCheckSize);
    }
    #endregion

    #region COMPONENTS
    public Rigidbody2D RB { get; private set; }
    //Script to handle all player animations, all references can be safely removed if you're importing into your own project.
    public PlayerAnimator AnimHandler { get; private set; }

    public GameObject PlayerFirePoint;
    public GameObject ChipPrefab;
    public GameObject PlayerCorpsePrefab;
    #endregion
    
    #region STATE PARAMETERS
    //Variables control the various actions the player can perform at any time.
    //These are fields which can are public allowing for other sctipts to read them
    //but can only be privately written to.
    public bool IsFacingRight { get; private set; }
    public bool IsJumping { get; private set; }
    public bool IsWallJumping { get; private set; }
    public bool IsDashing { get; private set; }
    public bool IsSliding { get; private set; }
    public bool IsWallJumpAssisting { get; private set; }
    public bool IsGrabbingWall { get; private set; }
    public bool IsWallLedgeClimbing { get; private set; }
    public bool IsPogoJumping { get; private set; }
    public bool IsAttacking { get; private set; }

    //Timers (also all fields, could be private and a method returning a bool could be used)
    public float LastOnGroundTime { get; private set; }
    public float LastOnWallTime { get; private set; }
    public float LastOnRightWallTime { get; private set; }
    public float LastOnLeftWallTime { get; private set; }
    public float LastAttackTime { get; private set; }
    public float AttackCooldown { get; private set; }
    public float WallHangTime { get; private set; }
    public float WallJumpAssistTimer { get; private set; }
    public float WallJumpCooldownTimer { get; private set; }
    private float WallJumpStartTime;
    private float ChipPressStartTime;
    
    //Jump
    private bool IsJumpCut;
    private bool IsJumpFalling;
    private int JumpCount;

    //Wall Jump
    public int LastCollidedWallIndex { get; private set; }
    private int LastWallJumpAssistDirectionIndex;

    //Dash
    private int LeftDashCount;
    private bool IsDashRefilling;
    private bool IsDashApex;

    #endregion

    #region INPUT PARAMETERS
    private Vector2 ActionInput;

    public float LastJumpPressedTime { get; private set; }
    public float LastDashPressedTime { get; private set; }
    #endregion

    #region CHECK PARAMETERS
    //Set all of these up in the inspector
    [Header("Combat Checks")] [SerializeField]
    private Transform TopCombatCheck;

    [SerializeField] private Transform FrontCombatCheck;
    [SerializeField] private Transform BottomCombatCheck;
    [SerializeField] private Vector2 CombatCheckSize = new(5f, 5f);

    [Space(5)] [Header("Ground Checks")] [SerializeField]
    private Transform GroundCheckPoint;

    [SerializeField] private Vector2 GroundCheckSize = new(0.49f, 0.03f);

    [Space(5)]
    [Header("Wall Checks")]
    //Size of groundCheck depends on the size of your character generally you want them slightly small than width (for ground) and height (for the wall check)
    [SerializeField] private Transform FrontWallCheckPoint;
    [SerializeField] private Transform BackWallCheckPoint;
    [SerializeField] private Vector2 WallCheckSize = new(0.5f, 1f);
    public Vector2 SpawnPoint;
    #endregion

    #region LAYERS & TAGS
    [Header("Layers & Tags")] 
    [SerializeField] private LayerMask GroundLayer;
    [SerializeField] private LayerMask WallLayer;
    [SerializeField] private LayerMask DamageableLayer;
    [SerializeField] private LayerMask PogoableLayer;

    #endregion
    
    #region INPUT CALLBACKS
    //Methods which whandle input detected in Update()
    public void OnMoveInput(float IncomingMoveInput)
    {
        if (!PlayerData.CanMove) return;
        ActionInput.x = IncomingMoveInput; //Horizontal
    }

    public void OnLookInput(float IncomingLookInput)
    {
        if (!PlayerData.CanMove) return;
        ActionInput.y = IncomingLookInput; //Vertical
    }

    public void OnJumpInput()
    {
        if (!PlayerData.CanMove) return;
        LastJumpPressedTime = PlayerData.JumpInputBufferDuration;
    }

    public void OnJumpInputRelease()
    {
        if (!PlayerData.CanMove) return;
        if (CanJumpCut() || CanWallJumpCut())
            IsJumpCut = true;
    }

    public void OnDashInput()
    {
        if (!PlayerData.CanMove) return;
        LastDashPressedTime = PlayerData.DashInputBufferDuration;
    }

    public void OnCombatInput()
    {
        if (!PlayerData.CanMove) return;
        LastAttackTime = PlayerData.AttackInputBufferDuration;
        IsAttacking = true;
    }

    public void OnWallGrabInput()
    {
        if (!PlayerData.CanMove) return;
        IsGrabbingWall = true;
    }

    public void OnChipHoldInput()
    {
        ChipPressStartTime = Time.time;
    }

    public void OnChipHoldReleaseInput()
    {
       var Chip = Instantiate(ChipPrefab, PlayerFirePoint.transform.position, Quaternion.identity);
       Chip.GetComponent<ChipScript>().MoveChip(Mathf.Clamp(Time.time - ChipPressStartTime,0,3) * GetPlayerFacingIndex());
    }

    public void OnWallGrabInputRelease()
    {
        if (!PlayerData.CanMove) return;
        IsGrabbingWall = false;
    }

    #endregion

    #region UNITY CALLS
    //--------------------------------------------------------\\
    //Setting the singleton
    //--------------------------------------------------------\\
    protected override void Awake()
    {
        base.Awake();
        Instance = this;
        SetupPlayer();
    }

    public static PlayerMovement Instance { get; private set; }
    //--------------------------------------------------------\\
    //Singleton has been set
    //--------------------------------------------------------\\

    private void SetupPlayer()
    {
        RB = GetComponent<Rigidbody2D>();
        AnimHandler = GetComponent<PlayerAnimator>();
    }

    private void Start()
    {
        SetGravityScale(PlayerData.GravityScale);
        LeftDashCount = PlayerData.MaximumDashAmount;
        IsFacingRight = true;
    }

    private void Update()
    {
        animator.SetFloat("Speed", PlayerMovement.Instance.ActionInput.x);
        animator.SetBool("isJumping", IsJumping);

        if (!PlayerData.CanMove) return;
        RunTimers(); //Run timers
        if (ActionInput.x != 0) CheckDirectionToFace(ActionInput.x > 0); //Set players facing direction
        
        if (!IsDashing && !IsJumping && !IsWallJumping && !IsWallJumpAssisting && !IsWallLedgeClimbing) //Check for player collisions
        {
            //Ground Check
            UpdateGrounded();
            
            //Wall Check
            UpdateWallCollision();
        }
        
        //Update jump and wall jump states
        UpdateWallVriables();
        UpdateJumpVariables();
        if (!IsDashing && !IsWallLedgeClimbing)
        {
            if (CanAttack() && LastAttackTime > 0) PlayerAttack();
            if (CanJump() && LastJumpPressedTime > 0) PlayerJump(); //If player can jump and the input is pressed jump
            else if (CanWallJump() && LastJumpPressedTime > 0) PlayerWallJump(-GetPlayerFacingIndex()); //Checks if the player can wall jump and makes player wall jump
           if (IsWallJumpAssisting)
           {
               if ( ActionInput.x != 0 && LastWallJumpAssistDirectionIndex == (ActionInput.x > 0 ? 1 : -1)) WallJumpAssist();
               else CancelWallJumpAssist();
           }
           if (CanLedgeClimb()){ WallLedgeClimb(); }
        }

        if (CanDash() && LastDashPressedTime > 0) PlayerDash(); //Checks if the player can dash, dash!
        PlayerDashRefill(); //Refills the players dash

        IsSliding = CanWallSlide(); //Let player wall slide if player can
        
        UpdateGravity(); //Update players gravity
    }

    private void FixedUpdate()
    {
        if (!PlayerData.CanMove) return;
        //Handle player move
        if (!IsDashing && !IsWallLedgeClimbing)
        {
            if (IsWallJumping || (IsWallJumpAssisting && !IsGrabbingWall))
                PlayerMove(PlayerData.WallJumpToMoveLerp);
            else if (CanPlayerMove())
                PlayerMove(1);
        }
        else if (IsDashApex)
        {
            PlayerMove(PlayerData.DashEndToMoveLerp);
        }

        //Handle wall slide
        if (IsSliding)
            PlayerWallClimbAndSlide();
    }

    #endregion

    #region COLLISION FUNCTIONS

    private void UpdateGrounded() // Check if the player is colliding with the ground
    {
        if (CheckPlayerGroundCollision()) //checks if set box overlaps with ground
        {
            WallHangTime = PlayerData.WallGrabDuration;
            LastOnGroundTime = PlayerData.LedgeOffJumpDuration; //Sets players last grounded time to cayote time letting them have some more time after being grounded
            JumpCount = PlayerData.JumpAmount - 1;
        }
        else if (JumpCount > 0 && LastJumpPressedTime > 0) //Additional jump (For double Jump)
        {
            LastOnGroundTime = PlayerData.LedgeOffJumpDuration;
            JumpCount--;
        }
        else if (CanWallJump() || CanWallJumpAssist()) LastOnGroundTime = 0; //Sets player grounded immidiately to not have any problem while wall jumping
    }

    private void UpdateWallCollision() // Checks if the player is colliding with any walls
    {
        //Check if the player is colliding with the right wall
        if (CheckPlayerRightWallCollision())
        {
            LastCollidedWallIndex = 1;
            LastOnRightWallTime = PlayerData.LedgeOffJumpDuration;
        }

        //Check if the player is colliding with the left wall
        if (CheckPlayerLeftWallCollision())
        {
            LastCollidedWallIndex = -1;
            LastOnLeftWallTime = PlayerData.LedgeOffJumpDuration;
        }

        //Sets the last wall colliding time to the closest collision
        LastOnWallTime = Mathf.Max(LastOnLeftWallTime, LastOnRightWallTime);
    }

    #endregion

    #region OTHER FUNCTIONS

    private void RunTimers() //Runs the timers to calculate player movements (If statements stops useless calculation)
    {
        if (WallHangTime >= 0) WallHangTime -= Time.deltaTime;
        if (WallJumpAssistTimer >= 0) WallJumpAssistTimer -= Time.deltaTime;
        if (WallJumpCooldownTimer >= 0) WallJumpCooldownTimer -= Time.deltaTime;
        
        if (LastOnWallTime >= 0) LastOnWallTime -= Time.deltaTime;
        if (LastOnRightWallTime >= 0) LastOnRightWallTime -= Time.deltaTime;
        if (LastOnLeftWallTime >= 0)  LastOnLeftWallTime -= Time.deltaTime;
        if (LastOnGroundTime >= 0) LastOnGroundTime -= Time.deltaTime;
        
        if (LastJumpPressedTime >= 0) LastJumpPressedTime -= Time.deltaTime;
        if (LastDashPressedTime >= 0) LastDashPressedTime -= Time.deltaTime;
         
        if (AttackCooldown >= 0) AttackCooldown -= Time.deltaTime;
        if (LastAttackTime >= 0) LastAttackTime -= Time.deltaTime;
    }

    private void Sleep(float duration) //Pauses the game for a time
    {
        StartCoroutine(PerformSleep(duration));
    }

    private IEnumerator PerformSleep(float duration) //Seperate sleep to another thread to have timers
    {
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(duration); //Must be Realtime since timeScale will be 0 
        Time.timeScale = 1;
    }

    #endregion

    #region GRAVITY FUNCTIONS

    public void SetGravityScale(float scale) //Sets the current gravity scale to the desired scale
    {
        RB.gravityScale = scale;
    }

    private void UpdateGravity() // Updates players gravity based on the current state
    {
        if (ZeroGravityConditions()) //Sets gravity to 0 for player to move in one axis easier
        {
            SetGravityScale(0);
        }
        else if (RB.linearVelocity.y < 0 && ActionInput.y < 0) // Increase gravity if the player is looking down
        {
            SetGravityScale(PlayerData.GravityScale * PlayerData.ForceFallSpeed);
            RB.linearVelocity = new Vector2(RB.linearVelocity.x, Mathf.Max(RB.linearVelocity.y, -PlayerData.MaxForceFallSpeed));
        }
        else if (IsJumpCut) //Higher gravity if jump button released
        {
            SetGravityScale(PlayerData.GravityScale * PlayerData.JumpCutGravityMultiplier);
            RB.linearVelocity = new Vector2(RB.linearVelocity.x, Mathf.Max(RB.linearVelocity.y, -PlayerData.MaxFallSpeed));
        }
        else if ((IsJumping || IsPogoJumping || IsWallJumping || IsJumpFalling) && Mathf.Abs(RB.linearVelocity.y) < PlayerData.JumpApexDuration) // Set gravity low at the apex of the jump
        {
            SetGravityScale(PlayerData.GravityScale * PlayerData.JumpApexGravityMultiplier);
        }
        else if (RB.linearVelocity.y < 0) // Increase gravity if player is falling
        {
            SetGravityScale(PlayerData.GravityScale * PlayerData.FallGravityMultiplier);
            RB.linearVelocity = new Vector2(RB.linearVelocity.x, Mathf.Max(RB.linearVelocity.y, -PlayerData.MaxFallSpeed));
        }
        else // Default gravity
        {
            SetGravityScale(PlayerData.GravityScale);
        }
    }

    #endregion

    #region MOVEMENT FUNCTIONS
    private void PlayerMove(float lerpAmount) // Move the player to the direction that they are facing
    {
        var targetSpeed = ActionInput.x * PlayerData.MaxMoveSpeed;
        
        targetSpeed = Mathf.Lerp(RB.linearVelocity.x, targetSpeed, lerpAmount);

        //Calculate player accelaration
        float accelRate;
        
        if (IsGrounded())
            accelRate = Mathf.Abs(targetSpeed) > 0.01f ? PlayerData.MoveAccelerationAmount : PlayerData.MoveDeccelerationAmount;
        else
            accelRate = Mathf.Abs(targetSpeed) > 0.01f ? PlayerData.MoveAccelerationAmount * PlayerData.AirbornAcceleration : PlayerData.MoveDeccelerationAmount * PlayerData.AirbornDecceleration;


        //Add bonus jump apex accelaration
        if ((IsJumping || IsWallJumping || IsJumpFalling) &&
            Mathf.Abs(RB.linearVelocity.y) < PlayerData.JumpApexDuration)
        {
            accelRate *= PlayerData.JumpApexAccelerationMultiplier;
            targetSpeed *= PlayerData.MaxJumpApexSpeedMultiplier;
        }

        //Conserve the players current momentum
        if (Mathf.Abs(RB.linearVelocity.x) > Mathf.Abs(targetSpeed) &&
            Mathf.Sign(RB.linearVelocity.x) == Mathf.Sign(targetSpeed) && Mathf.Abs(targetSpeed) > 0.01f &&
            LastOnGroundTime < 0)
            accelRate = 0;
        
        var speedDif = targetSpeed - RB.linearVelocity.x;
        var movement = speedDif * accelRate;
        
        RB.AddForce(movement * Vector2.right, ForceMode2D.Force);
    }

    private void PlayerTurn() // Turns the player to the other side
    {
        IsFacingRight = !IsFacingRight;
        
        var Rot = transform.localRotation;
        if (IsFacingRight)
            Rot.y = 0;
        else
            Rot.y = 180f;
        
        transform.localRotation = Rot;
        CameraManager.Instance.TurnPlayerCamera();
    }
    
    private Vector2Int GetPlayerFacingDirection()
    {
        return IsFacingRight ? Vector2Int.right: Vector2Int.left;
    }
        
    private int GetPlayerFacingIndex()
    {
        return GetPlayerFacingDirection().x;
    }

    #endregion

    #region JUMP FUNCTIONS
    private void UpdateJumpVariables() //Updates all the state variables required for jump and walljump
    {
        if (IsJumping && RB.linearVelocity.y < 0 && (IsJumping||!IsJumpFalling))
        {
            IsJumping = false;
            IsJumpFalling = true;
        }

        if (WallJumpAssistTimer <= 0 && IsWallJumpAssisting) IsWallJumpAssisting = false;
        if (IsWallJumping && Time.time - WallJumpStartTime > PlayerData.WallJumpDuration) IsWallJumping = false;

        if (IsGrounded() && !IsJumping && !IsWallJumping && !IsWallJumpAssisting && (IsJumpCut || IsJumpFalling))
        {
            IsJumpCut = false;
            IsJumpFalling = false;
        }
    }

    private void PlayerJump() // Makes the player jump
    {
        IsJumping = true;
        IsWallJumping = false;
        IsJumpCut = false;
        IsJumpFalling = false;
        LastJumpPressedTime = 0;
        LastOnGroundTime = 0;
        
        RB.linearVelocity = new Vector2(RB.linearVelocity.x, 0);
        RB.AddForce(Vector2.up * PlayerData.JumpForce, ForceMode2D.Impulse);
    }
    
    private void PlayerPogoJump(Transform AttackTransform, Vector2 AttackArea)
    {
        var GroundObjects = Physics2D.OverlapBoxAll(AttackTransform.position,AttackArea,0,PogoableLayer);
        if (GroundObjects.Length > 0 && !IsPogoJumping && !IsDashing)
        {
            StartCoroutine(StartPogoJump());
        }
    }

    IEnumerator StartPogoJump()
    {
        IsJumping = false;
        IsJumpCut = false;
        IsJumpFalling = false;
        IsGrabbingWall = false;
        LastJumpPressedTime = 0;
        LastOnGroundTime = 0;
        LastOnRightWallTime = 0;
        LastOnLeftWallTime = 0;
        IsPogoJumping = true;
        
        RB.linearVelocity = Vector2.zero;
        RB.AddForce(new Vector2(0,-ActionInput.y * PlayerData.PogoJumpForce),ForceMode2D.Impulse);
        yield return new WaitForSeconds(PlayerData.PogoJumpDuration);
        IsPogoJumping = false;
        yield return null;
    }
    #endregion

    #region DASH FUNCTIONS

    private void PlayerDash() //Starts the player's dash
    {
        Sleep(PlayerData.DashFreezeDuration);
        
        IsDashing = true;
        IsJumping = false;
        IsWallJumping = false;
        IsJumpCut = false;

        StartCoroutine(StartDash(ActionInput.x != 0 && ActionInput.y == 0? ActionInput : GetPlayerFacingDirection()));
    }
    
    private void PlayerDashRefill() //Refill the dash cooldown
    {
        if (!IsDashing && (LeftDashCount < PlayerData.MaximumDashAmount) && (IsGrounded() || LastOnWallTime > 0) && !IsDashRefilling)
            StartCoroutine(RefillDash());
    }
    
    private IEnumerator StartDash(Vector2 dir) //Seperate player dash to another thread to have timers
    {
        LastOnGroundTime = 0;
        LastDashPressedTime = 0;

        var startTime = Time.time;

        LeftDashCount--;
        IsDashApex = true;

        SetGravityScale(0);
            
        while (Time.time - startTime <= PlayerData.DashApexReachTime)
        {
            RB.linearVelocity = dir.normalized * PlayerData.DashSpeed;
            yield return null;
        }

        startTime = Time.time;

        IsDashApex = false; 
        
        SetGravityScale(PlayerData.GravityScale);
        RB.linearVelocity = PlayerData.DashEndSpeed * dir.normalized;

        while (Time.time - startTime <= PlayerData.DashEndTime) yield return null;
        
        IsDashing = false;
    }
    
    private IEnumerator RefillDash() //Seperate dash refill to another thread to have timers
    {
        IsDashRefilling = true;
        yield return new WaitForSeconds(PlayerData.DashRefillDuration);
        IsDashRefilling = false;
        LeftDashCount = Mathf.Min(PlayerData.MaximumDashAmount, LeftDashCount + 1);
    }

    #endregion

    #region COMBAT FUNCTIONS

    public void KillPlayer()
    {
        Instantiate(PlayerCorpsePrefab, transform.position, Quaternion.identity);
        transform.position = SpawnPoint;
    }
    
    private void PlayerAttack() //Makes the player attack to the direction that the player looks
    {
            AttackCooldown = PlayerData.AttackCooldown;
            if (ActionInput.y == 0 || (ActionInput.y < 0 && IsGrounded())) //Attack Left Or Right
            {
                HitEnemiesInRange(FrontCombatCheck, CombatCheckSize);
                Debug.Log("Attacking Front");
            }
            else if (ActionInput.y > 0) // Attack Up
            {
                HitEnemiesInRange(TopCombatCheck, CombatCheckSize);
                Debug.Log("Attacking Top");
            }
            else if (ActionInput.y < 0 && !IsGrounded()) //Attack down
            {
                HitEnemiesInRange(BottomCombatCheck, CombatCheckSize, true);
                PlayerPogoJump(BottomCombatCheck, CombatCheckSize);
                Debug.Log("Attacking Bottom");
            }
            else AttackCooldown = 0;
    }
    private void HitEnemiesInRange(Transform AttackTransform, Vector2 AttackArea, bool HasKnockback = false) //Checks if the hitting range has enimies and hits them
    {
        var DamageableObjects = Physics2D.OverlapBoxAll(AttackTransform.position, AttackArea, 0, DamageableLayer);
        if (DamageableObjects.Length > 0)
        {
            Debug.Log("HIT!");
            for (var i = 0; i < DamageableObjects.Length; i++)
                if (DamageableObjects[i].GetComponent<Enemy>() != null)
                    DamageableObjects[i].GetComponent<Enemy>().EnemyHit(PlayerData.PlayerDamage,
                        (transform.position - DamageableObjects[i].transform.position).normalized * -1,
                        PlayerData.PlayerDamage * 10);
            Sleep(.1f); // Hit Freeze
        }
    }

    #endregion

    #region WALL FUNCTIONS

    private void UpdateWallVriables() // Sets the variables about wall functions
    {
        if (WallHangTime <= 0) IsGrabbingWall = false;
    }

    private void PlayerWallJump(int DirectionIndex) //Starts the wall jump
    {
        IsWallJumping = true;
        IsJumping = false;
        IsJumpCut = false;
        IsJumpFalling = false;
        IsGrabbingWall = false;
        LastJumpPressedTime = 0;
        LastOnGroundTime = 0;
        LastOnRightWallTime = 0;
        LastOnLeftWallTime = 0;
        WallJumpStartTime = Time.time;
        RB.linearVelocity = Vector2.zero;
        
        RB.AddForce(new Vector2(PlayerData.WallJumpForce.x * DirectionIndex,PlayerData.WallJumpForce.y), ForceMode2D.Impulse);
        PlayerTurn();
        if (CanWallJumpAssist())
        {
            IsWallJumpAssisting = true;
            LastWallJumpAssistDirectionIndex = DirectionIndex > 0 ? -1 : 1;
            WallJumpAssistTimer = PlayerData.WallJumpAssistTime;
        }
        
    }

    private void CancelWallJumpAssist()
    {
        IsWallJumpAssisting = false;
        RB.linearVelocity = Vector2.zero;
        RB.AddForce(new Vector2(PlayerData.WallJumpForce.x * -LastWallJumpAssistDirectionIndex / 1.25f, PlayerData.WallJumpForce.y / 1.25f), ForceMode2D.Impulse);
        LastWallJumpAssistDirectionIndex = 0;
        WallJumpCooldownTimer = PlayerData.WallJumpCooldown;
    }
    
    private void WallJumpAssist() //This assists the player to continue to the next wall jump without falling too much
    {
        RB.linearVelocity = Vector2.zero;
        RB.AddForce(new Vector2(PlayerData.WallJumpForce.x * -LastCollidedWallIndex, PlayerData.WallJumpForce.y), ForceMode2D.Impulse);
        WallJumpCooldownTimer = PlayerData.WallJumpCooldown;
    }

    private void WallLedgeClimb()
    {
        LastOnWallTime = 0;
        IsGrabbingWall = false;
        RB.linearVelocity = Vector2.zero;
        StartCoroutine(StartWallLedgeClimb(new Vector2(GetPlayerFacingDirection().x,2)));
    }
    
    private IEnumerator StartWallLedgeClimb(Vector2 dir) //Seperate Wall climb assist to another thread to have timers
    {
        Sleep(PlayerData.DashFreezeDuration);
        
        IsWallLedgeClimbing = true;
        IsJumping = false;
        IsWallJumping = false;
        IsJumpCut = false;
        
        var startTime = Time.time;
        SetGravityScale(0);
        while (Time.time - startTime <= PlayerData.WallLedgeClimbTime)
        {
            RB.linearVelocity = dir.normalized * PlayerData.WallLedgeClimbSpeed;
            yield return null;
        }
        SetGravityScale(PlayerData.GravityScale);
        IsWallLedgeClimbing = false;
    }

    private void PlayerWallClimbAndSlide() //This makes the player slide with an accelarating speed when they are colliding to the walls
    {
        if (CanWallHang()) //This lets player to fully grab the wall and hang without falling and lets player move up and down while holding
        {
            RB.linearVelocity = Vector2.zero;
            RB.AddForce(new Vector2(0, PlayerData.CanWallClimb ? ActionInput.y > 0 ? 1 * PlayerData.WallClimbSpeed : (ActionInput.y < 0 ? -2f : 0) * PlayerData.WallClimbSpeed : 0), ForceMode2D.Impulse);
            return;
        }
        
        var speedDif = PlayerData.WallSlideSpeed - RB.linearVelocity.y;
        var movement = speedDif * PlayerData.WallSlideAcceleration;
        
        movement = Mathf.Clamp(movement, -Mathf.Abs(speedDif) * (1 / Time.fixedDeltaTime),
            Mathf.Abs(speedDif) * (1 / Time.fixedDeltaTime));

        RB.AddForce(movement * Vector2.up);
    }
    #endregion

    #region CHECK METHODS

    public void CheckDirectionToFace(bool isMovingRight) //Checks where player should face
    {
        if (isMovingRight != IsFacingRight && CanPlayerMove())
            PlayerTurn();
    }

    public bool CheckPlayerLeftWallCollision()
    {
        return ((Physics2D.OverlapBox(FrontWallCheckPoint.position, WallCheckSize, 0, GroundLayer) && !IsFacingRight)
                || (Physics2D.OverlapBox(BackWallCheckPoint.position, WallCheckSize, 0, GroundLayer) &&
                    IsFacingRight)) && !IsWallJumping;
    }

    public bool CheckPlayerRightWallCollision()
    {
       return ((Physics2D.OverlapBox(FrontWallCheckPoint.position, WallCheckSize, 0, GroundLayer) && IsFacingRight)
         || (Physics2D.OverlapBox(BackWallCheckPoint.position, WallCheckSize, 0, GroundLayer) &&
             !IsFacingRight)) && !IsWallJumping;
    }

    public bool CheckPlayerGroundCollision()
    {
        return (Physics2D.OverlapBox(GroundCheckPoint.position, GroundCheckSize, 0, GroundLayer) ||
                Physics2D.OverlapBox(GroundCheckPoint.position, GroundCheckSize, 0, WallLayer))  && PlayerData.JumpAmount > 0;
    }
    
    private bool CanWallJumpAssist() //Checks if the player can have an assist in wall jump
    {
        return !IsDashing && ActionInput.x != 0;
    }

    private bool CanWallJump() //Chceks if the player can wall jump
    {
        return LastJumpPressedTime > 0 && ActionInput.x != 0 && PlayerData.CanWallJump &&  WallJumpCooldownTimer <= 0 && LastOnWallTime > 0 && !IsJumping && (!IsWallJumping || LastOnLeftWallTime > 0 || LastOnRightWallTime > 0);
    }
    
    private bool CanWallJumpCut() //Checks if the player can stop the wall jump mid air
    {
        return IsWallJumping && RB.linearVelocity.y > 0;
    }
    
    public bool CanWallHang() //Checks if the player can hang at the wall
    {
        return WallHangTime > 0 && !IsGrounded() && CanWallSlide() && !IsDashing && IsGrabbingWall && PlayerData.CanWallGrab && (!IsWallJumping || LastOnLeftWallTime > 0 || LastOnRightWallTime > 0);
    }

    public bool CanLedgeClimb()
    {
        return LastOnWallTime > 0 && !IsGrounded() && IsGrabbingWall && ActionInput.y > 0 && !(CheckPlayerLeftWallCollision() || CheckPlayerRightWallCollision());
    }
    
    public bool CanWallSlide() //Checks if the player should slide slowly from the wall
    {
        return LastOnWallTime > 0 && !IsJumping && !IsWallJumping && !IsDashing && LastOnGroundTime <= 0 &&
               (LastOnLeftWallTime > 0 || LastOnRightWallTime > 0);
    }

    private bool CanJumpCut() //Checks if the player can stop the jumping mid air
    {
        return IsJumping && RB.linearVelocity.y > 0;
    }

    private bool CanDash() //Checks if the player can dash
    {
        return LeftDashCount > 0;
    }

    public bool CanAttack() //Checks if the player can attack
    {
        return PlayerData.CanEnterCombat && IsAttacking && AttackCooldown <= 0 && !IsGrabbingWall;
    }

    public bool CanJump() //Checks if the player can jump
    {
        return IsGrounded() && !IsWallJumping && !IsWallJumpAssisting && !IsDashing && !IsGrabbingWall;
    }

    public bool CanPlayerMove()
    {
        return !IsGrabbingWall && !IsDashing && !IsWallLedgeClimbing;
    }

    public bool IsGrounded() //Checks if the player is on the ground
    {
        return LastOnGroundTime > 0;
    }

    public bool ZeroGravityConditions()
    {
        return IsSliding || IsWallJumpAssisting || IsDashApex || IsWallLedgeClimbing;
    }

    #endregion
}