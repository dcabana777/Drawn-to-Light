using UnityEngine;

public class MovingPlatformPassenger : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the colliding object is the player (adjust tag if needed)
        if (collision.gameObject.CompareTag("Player"))
        {
            // Make the player a child of the platform
            collision.transform.SetParent(transform);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Remove the player from the platform's hierarchy
            collision.transform.SetParent(null);
        }
    }
}