using UnityEngine;

public class FallTrigger : MonoBehaviour
{
    [SerializeField] private Vector2 respawnLocation;
    private Transform playerTransform;

    private void Awake()
    {
        playerTransform = GameObject.FindWithTag("Player").transform;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerTransform.position = new Vector3(respawnLocation.x, respawnLocation.y);
        }
    }

}
