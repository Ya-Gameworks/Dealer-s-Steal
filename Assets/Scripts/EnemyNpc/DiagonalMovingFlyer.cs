using UnityEngine;

public class DiagonalMovingFlyer : MonoBehaviour
{
    [SerializeField] float moveSpeed;
    [SerializeField] Vector2 moveDirection = new Vector2(1f, 0.25f);
    [SerializeField] GameObject rightCheck, roofCheck, groundCheck;
    [SerializeField] Vector2 rightCheckSize, roofCheckSize, groundCheckSize;
    [SerializeField] LayerMask groundLayer, wallLayer;
    [SerializeField] bool goingUp = true;

    private bool touchedGround, touchedRoof, touchedRight;
    private Rigidbody2D rb;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        HitLogic();
    }

    void FixedUpdate()
    {
        rb.linearVelocity = moveDirection * moveSpeed;
    }

    void HitLogic()
    {
        touchedRight = HitDetector(rightCheck, rightCheckSize, (groundLayer | wallLayer));
        touchedRoof = HitDetector(roofCheck, roofCheckSize, (groundLayer | wallLayer));
        touchedGround = HitDetector(groundCheck, groundCheckSize, (groundLayer | wallLayer));

        if (touchedRight)
        {
            Flip();
        }
        if (touchedRoof && goingUp)
        {
            ChangeYDirection();
        }
        if (touchedGround && !goingUp)
        {
            ChangeYDirection();
        }
    }

    bool HitDetector(GameObject gameObject, Vector2 size, LayerMask layer)
    {
        return Physics2D.OverlapBox(gameObject.transform.position, size, 0f, layer);
    }

    void ChangeYDirection()
    {
        moveDirection.y = -moveDirection.y;
        goingUp = !goingUp;
    }

    void Flip()
    {
        transform.Rotate(new Vector2(0, 180));
        moveDirection.x = -moveDirection.x;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(groundCheck.transform.position, groundCheckSize);
        Gizmos.DrawWireCube(roofCheck.transform.position, roofCheckSize);
        Gizmos.DrawWireCube(rightCheck.transform.position, rightCheckSize);
    }

    //Rigidbody2D rb;

    //[SerializeField] float horizontalMoveSpeed;
    //[SerializeField] float verticalMoveSpeed;

    //[SerializeField] bool willMoveRight;
    //[SerializeField] bool willMoveUp;

    //Vector2 direction;

    //void Start()
    //{
    //    rb = GetComponent<Rigidbody2D>();

    //    direction.x = willMoveRight ? 1 : -1;
    //    direction.y = willMoveUp ? 1 : -1;
    //}

    //void OnCollisionEnter2D(Collision2D collision)
    //{
    //    if (collision.gameObject.layer == 3) //if hit ground change dir
    //    {
    //        direction.y *= -1;
    //    }
    //    else if (collision.gameObject.layer == 9) //if hit wall change dir
    //    {
    //        direction.x *= -1;
    //    }
    //}

    //void FixedUpdate()
    //{
    //    PatrolInAir(direction);
    //}

    //void PatrolInAir(Vector2 _dir)
    //{
    //    rb.linearVelocity = new Vector2(_dir.x * horizontalMoveSpeed, _dir.y * verticalMoveSpeed);
    //}
}
