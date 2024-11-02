using Unity.VisualScripting;
using UnityEngine;

public class PlayerKiller : MonoBehaviour
{
   public void OnTriggerEnter2D(Collider2D collision)
   {
      if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
      {
         PlayerMovement.Instance.KillPlayer();
      }
   }
}
