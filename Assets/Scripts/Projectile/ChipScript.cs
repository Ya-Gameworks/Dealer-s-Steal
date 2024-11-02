using UnityEngine;

public class ChipScript : MonoBehaviour
{
    private Rigidbody2D ChipRigidbody;
    public float ChipMoveSpeed;

    private void Awake()
    {
        ChipRigidbody = GetComponent<Rigidbody2D>();
    }
    
    public void MoveChip(float ChipChargeMultiplier)
    {
        Debug.Log("Ananasaldırdım");
        Debug.Log(ChipChargeMultiplier);
        ChipRigidbody.linearVelocity = new Vector2(ChipChargeMultiplier * ChipMoveSpeed, 0);
    }
}
