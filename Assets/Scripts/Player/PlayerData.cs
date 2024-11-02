using UnityEngine;

[CreateAssetMenu(menuName = "Player Data")] //Create a new playerData object by right clicking in the Project Menu then Create/Player/Player Data and drag onto the player
public class PlayerData : ScriptableObject
{
	[HideInInspector] public float GravityStrenght; //Downwards force (gravity) needed for the desired jumpHeight and jumpTimeToApex.
	[HideInInspector] public float GravityScale; //Strength of the player's gravity as a multiplier of gravity (set in ProjectSettings/Physics2D).//Also the value the player's rigidbody2D.gravityScale is set to.
	[Header("Gravitational Data")]
	[Space(5)]
	public float FallGravityMultiplier = 2; //Multiplier to the player's gravityScale when falling.
	public float MaxFallSpeed = 20; //Maximum fall speed (terminal velocity) of the player when falling.
	[Space(10)]
	public float ForceFallSpeed = 1; //Larger multiplier to the player's gravityScale when they are falling and a downwards input is pressed.
	public float MaxForceFallSpeed = 25; //Maximum fall speed of the player when performing a force fall.

	[Space(10)] [Header("Movement Data")] 
	[Space(5)]
	public bool CanMove = true;
	public float MaxMoveSpeed = 9.5f; //Players maximum move speed
	public float MoveAcceleration = 9.5f; //The speed at which our player accelerates to max speed, can be set to runMaxSpeed for instant acceleration down to 0 for none at all
	[HideInInspector] public float MoveAccelerationAmount; //The actual force (multiplied with speedDiff) applied to the player.
	public float MoveDecceleration = 9.5f; //The speed at which our player decelerates from their current speed, can be set to runMaxSpeed for instant deceleration down to 0 for none at all
	[HideInInspector] public float MoveDeccelerationAmount; //Actual force (multiplied with speedDiff) applied to the player .
	[Space(5)]
	[Range(0f, 1)] public float AirbornAcceleration = 1; //Multipliers applied to acceleration rate when airborne.
	[Range(0f, 1)] public float AirbornDecceleration = 1;
	[Space(20)]

	[Header("Jump Data")]
	public float MaxJumpHeight = 6.5f; //Height of the player's jump
	public float JumpTimeToApex = 0.5f; //Time between applying the jump force and reaching the desired jump height. These values also control the player's gravity and jump force.
	public int JumpAmount = 1;
	public float PogoJumpForce = 20f;
	public float PogoJumpDuration = .5f;
	[HideInInspector] public float JumpForce; //The actual force applied (upwards) to the player when they jump.

	[Header("Both Jumps")]
	public float JumpCutGravityMultiplier = 3.5f; //Multiplier to increase gravity if the player releases thje jump button while still jumping
	[Range(0f, 1)] public float JumpApexGravityMultiplier = 1; //Reduces gravity while close to the apex (desired max height) of the jump
	public float JumpApexDuration = 0; //Speeds (close to 0) where the player will experience extra "jump hang". The player's velocity.y is closest to 0 at the jump's apex (think of the gradient of a parabola or quadratic function)
	[Space(0.5f)]
	public float JumpApexAccelerationMultiplier = 1f; 
	public float MaxJumpApexSpeedMultiplier = 1f; 				

	[Header("Wall Jump")]
	[Space(5)]
	[Range(0f, 1f)] public float WallJumpToMoveLerp = 0.075f; //Reduces the effect of player's movement while wall jumping.
	[Range(0f, 1.5f)] public float WallJumpDuration = 0.2f; //Time after wall jumping the player's movement is slowed for.
	public Vector2 WallJumpForce = new Vector2(6f,20); //The actual force (this time set by us) applied to the player when wall jumping.
	public float WallJumpAssistTime = 0.1f; //Time it will start assisting the player to the jump destination.
	public float WallJumpCooldown = .3f;
	public bool CanWallJump = true;
	public bool CanWallGrab = true;

	[Space(20)]

	[Header("Wall Slide Data")]
	public float WallClimbSpeed = 3;
	public bool  CanWallClimb = true;
	public float WallSlideSpeed = -12;
	public float WallSlideAcceleration = 12;
	public float WallGrabDuration = 5f;
	public float WallLedgeClimbSpeed = 6;
	public float WallLedgeClimbTime = 0.15f;

    [Header("Assists")]
	[Range(0.01f, 0.5f)] public float LedgeOffJumpDuration = 0.2f; //Grace period after falling off a platform, where you can still jump
	[Range(0.01f, 0.5f)] public float JumpInputBufferDuration = 0.2f; //Grace period after pressing jump where a jump will be automatically performed once the requirements (eg. being grounded) are met.
	[Range(0.01f, 0.5f)] public float AttackInputBufferDuration = 0.2f; //Grace period after pressing attack where attack will be automatically executed
	[Space(20)]

	[Header("Dash")]
	public int MaximumDashAmount = 1;
	public float DashSpeed = 20;
	public float DashFreezeDuration = 0.05f; //Duration for which the game freezes when we press dash but before we read directional input and apply a force
	public float DashApexReachTime = 0.15f;
	[Space(5)]
	public float DashEndTime = 0.15f; //Time after you finish the inital drag phase, smoothing the transition back to idle (or any standard state)
	public Vector2 DashEndSpeed = new Vector2(15,15); //Slows down player, makes dash feel more responsive (used in Celeste)
	[Range(0f, 1f)] public float DashEndToMoveLerp = 0.5f; //Slows the affect of player movement while dashing
	[Space(5)]
	public float DashRefillDuration = 0.1f;
	[Space(5)]
	[Range(0.01f, 0.5f)] public float DashInputBufferDuration = 0.1f;
	
	[Space(20)]
	
	[Header("Combat")]
	public bool CanEnterCombat = true;
	public int PlayerHealth = 100;
	public int PlayerDamage = 2;
	public float AttackCooldown = 0.2f;
	
	//Unity Callback, called when the inspector updates
    private void OnValidate()
    {
		//Calculate gravity strength using the formula (gravity = 2 * jumpHeight / timeToJumpApex^2) 
		GravityStrenght = -(2 * MaxJumpHeight) / (JumpTimeToApex * JumpTimeToApex);
		
		//Calculate the rigidbody's gravity scale (ie: gravity strength relative to unity's gravity value, see project settings/Physics2D)
		GravityScale = GravityStrenght / Physics2D.gravity.y;

		//Calculate are run acceleration & deceleration forces using formula: amount = ((1 / Time.fixedDeltaTime) * acceleration) / runMaxSpeed
		MoveAccelerationAmount = (50 * MoveAcceleration) / MaxMoveSpeed;
		MoveDeccelerationAmount = (50 * MoveDecceleration) / MaxMoveSpeed;

		//Calculate jumpForce using the formula (initialJumpVelocity = gravity * timeToJumpApex)
		JumpForce = Mathf.Abs(GravityStrenght) * JumpTimeToApex;

		#region Variable Ranges
		MoveAcceleration = Mathf.Clamp(MoveAcceleration, 0.01f, MaxMoveSpeed);
		MoveDecceleration = Mathf.Clamp(MoveDecceleration, 0.01f, MaxMoveSpeed);
		#endregion
	}
}