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

    private void Awake()
    {
        PlayerRespawnLocation = transform.position;

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
        foreach (IRoomResettable thing in ThingsToReset)
        {
            thing.SaveState();
        }

        HasSaved = true;
    }
}
