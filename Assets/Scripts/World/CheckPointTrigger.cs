using UnityEngine;

public class CheckPointTrigger : MonoBehaviour
{
    private GameManager gm;
    
    [SerializeField] private CheckPoint newCheckPoint;

    private void Awake()
    {
        gm = FindAnyObjectByType<GameManager>();

        if (gm == null)
        {
            Debug.LogError("CheckPointTrigger could not find GameManager!");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        gm.CurrentRoom.ChangeCheckPoint(newCheckPoint);
    }
}
