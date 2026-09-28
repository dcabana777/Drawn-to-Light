using UnityEngine;
using UnityEngine.InputSystem;

public class ReturnToCheckpoint : MonoBehaviour
{
    private GameManager gm;


    private void Awake()
    {
        gm = FindAnyObjectByType<GameManager>();

        if (gm == null)
        {
            Debug.LogError("ReturnToCheckpoint could not find GameManager!");
        }
    }

    private void OnResetCheckpoint()
    {
        gm.RespawnPlayer();
    }
}
