using System;
using UnityEngine;

public class Enemy : MonoBehaviour
{
   [SerializeField] private float health = 100f;
   [SerializeField] private float RecoilLenght;
   [SerializeField] private float RecoilFactor;
   [SerializeField] private bool IsRecoiling = false; 
   float RecoilTimer = 0f;
   Rigidbody2D rb2d;

   private void Awake()
   {
      rb2d = GetComponent<Rigidbody2D>();
   }
   
   private void Update()
   {
      if (health <= 0)
      {
         Destroy(gameObject);
      }

      if (IsRecoiling)
      {
         if (RecoilTimer < RecoilLenght)
         {
            RecoilTimer+= Time.deltaTime;
         }
         else
         {
            IsRecoiling = false;
            RecoilTimer = 0;
         }
      }
   }

   public void EnemyHit(float Damage,Vector2 HitDirection, float HitForce)
   {
      health -= Damage;
      if (!IsRecoiling)
      {
         rb2d.AddForce(HitForce * RecoilFactor * HitDirection);
      }
   }
}
