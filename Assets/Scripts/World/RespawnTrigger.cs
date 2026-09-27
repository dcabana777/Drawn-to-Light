using UnityEngine;

public class RespawnTrigger : MonoBehaviour
{
    private GameManager gm;

    private void Awake()
    {
        gm = FindAnyObjectByType<GameManager>();

        if (gm == null)
        {
            Debug.LogError("FallZone could not find GameManager!");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            gm.RespawnPlayer();
        }
    }

}
