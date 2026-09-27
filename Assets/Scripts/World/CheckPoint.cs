using System;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    [SerializeField] private GameRoom checkpointOwnerRoom;
    [SerializeField] private int initialLightGroup = -1;
    public Vector2 PlayerRespawnLocation { get; private set; }
    public int InitialLightGroup { get; private set; }

    void Awake()
    {
        PlayerRespawnLocation = transform.position;
    }

    void Start()
    {
        if (initialLightGroup <= 0)
        {
            InitialLightGroup = checkpointOwnerRoom.MainLightGroup;
        }
        else
        {
            InitialLightGroup = initialLightGroup;
        }
    }
}
