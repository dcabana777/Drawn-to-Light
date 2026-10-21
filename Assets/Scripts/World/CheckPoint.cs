using System;
using System.Linq;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    public enum CheckPointType
    {
        Major,
        Room
    }
    
    
    [SerializeField] private GameRoom checkpointOwnerRoom;
    [SerializeField] private string checkPointName;
    public string CheckPointName => checkPointName;
    [SerializeField] private int initialLightGroup = -1;
    [SerializeField] private CheckPointType checkPointType;
    [SerializeField] private int checkpointPriority = 0;
    public Vector2 PlayerRespawnLocation { get; private set; }
    public int InitialLightGroup { get; private set; }

    public GameRoom CheckPointOwnerRoom => checkpointOwnerRoom;
    public IRoomResettable[] ThingsToReset { get; private set; }
    public bool HasSaved { get; private set; }
    public CheckPointType Type => checkPointType;
    public int CheckPointPriority => checkpointPriority;

    private GameManager gm;

    private void Awake()
    {
        PlayerRespawnLocation = transform.position;

        gm = FindAnyObjectByType<GameManager>();
        if (gm == null)
        {
            Debug.LogError("CheckPoint: " + CheckPointName + " could not find GameManager!");
        }

        ThingsToReset = checkpointOwnerRoom.GetComponentsInChildren<IRoomResettable>(true);

        HasSaved = false;
    }

    private void Start()
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

    public void SaveRoom()
    {
        int i = 1;
        foreach (IRoomResettable thing in ThingsToReset)
        {
            thing.SaveState();
            //Debug.Log("Saving " + thing + " " + i + "\n");
            i++;
        }

        gm.SaveGlobalConditions();

        HasSaved = true;
    }
}
