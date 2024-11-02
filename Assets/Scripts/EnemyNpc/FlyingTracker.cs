using UnityEngine;

public class FlyingTracker : MonoBehaviour
{
    [SerializeField] float flyingSpeed;
    [SerializeField] float lineOfSite;
    private Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        float distanceFromPlayer = Vector2.Distance(player.position, transform.position);

        if (distanceFromPlayer < lineOfSite)
            transform.position = Vector2.MoveTowards(transform.position, player.position, flyingSpeed * Time.deltaTime);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, lineOfSite);
    }
}

