using UnityEngine;

public class Patroller : MonoBehaviour
{
    Rigidbody2D rb;

    //patrol
    [SerializeField] Transform patrolCenter;
    [SerializeField] float patrolDistance;
    [SerializeField] bool facingRight;
    [SerializeField] bool canTurn = true;
    [SerializeField] float walkSpeed;
    private int direction;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        transform.position = patrolCenter.position;

        direction = facingRight ? 1 : -1;
    }

    void FixedUpdate()
    {
        Patrol(direction);
    }

    void Patrol(int dir)
    {
        //walk
        rb.linearVelocity = new Vector2(dir * walkSpeed, rb.linearVelocity.y);

        if (transform.position.x > patrolCenter.position.x - 1f && transform.position.x < patrolCenter.position.x + 1f)
        {
            canTurn = true;
        }

        //turn
        float distanceFromPatrolCenter = Vector2.Distance(patrolCenter.position, transform.position);

        if (distanceFromPatrolCenter >= patrolDistance && canTurn)
        {
            canTurn = false;
            direction *= -1;
            transform.Rotate(0, 180, 0);
        }
    }
}
