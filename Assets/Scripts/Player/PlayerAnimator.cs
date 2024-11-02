using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    public Animator PlayerAnims;

    private void Update()
    {
        PlayerAnims.SetBool("Jump", PlayerMovement.Instance.IsJumping || PlayerMovement.Instance.IsPogoJumping);
        PlayerAnims.SetVector("Vel Y", new Vector2(0,PlayerMovement.Instance.RB.linearVelocityY));
        
    }
}
