using Unity.VisualScripting;
using UnityEngine;

public class ChipScript : MonoBehaviour
{
    private Rigidbody2D ChipRigidbody;
    public float ChipMoveSpeed;

    private void Awake()
    {
        ChipRigidbody = GetComponent<Rigidbody2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Pogoable") &&
            !(ChipRigidbody.constraints == RigidbodyConstraints2D.FreezeAll))
        {
            BreakChip();
        }
        
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
        ChipRigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
        }
    }

    public void BreakChip()
    {
        Destroy(gameObject);
    }
    
    public void MoveChip(float ChipChargeMultiplier)
    {
        ChipRigidbody.linearVelocity = new Vector2(ChipChargeMultiplier * ChipMoveSpeed, 0);
    }
}
