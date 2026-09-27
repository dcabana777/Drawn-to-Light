using UnityEngine;

public class LightGroupTransition : MonoBehaviour
{
    private GameManager gm;
    [SerializeField] private GameRoom defaultRoom;
    [SerializeField] private int lightGroupOne;
    [SerializeField] private int lightGroupTwo;

    private void Awake()
    {
        gm = FindAnyObjectByType<GameManager>();

        if (gm == null)
        {
            Debug.LogError(
                "RoomTransition could not find GameManager!"
            );
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        LightController playerController = other.GetComponent<LightController>();

        int currentLightGroup = playerController.LightGroup;

        if (currentLightGroup == lightGroupOne)
            playerController.ChangeLightGroup(lightGroupTwo);
        else if (currentLightGroup == lightGroupTwo)
            playerController.ChangeLightGroup(lightGroupOne);
        else
        {
            Debug.LogWarning("Entered LightGroupTransition with a nonmatching LightGroup!");

            if (defaultRoom != null)
                Debug.Log("Assiging default GameRoom LightGroup instead.");
                playerController.ChangeLightGroup(defaultRoom.MainLightGroup);
        }
    }

}
