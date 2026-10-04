using UnityEngine;
using UnityEngine.InputSystem;

public class DebugOverlay : MonoBehaviour
{
    [Header("Controls")]
    [SerializeField] private Key toggleOverlayKey = Key.F1;
    [SerializeField] private Key toggleInteractableLabelKey = Key.F2;

    [Header("Display")]
    [SerializeField] private bool startsVisible = false;
    [SerializeField] private bool showInteractableLabels = true;

    [Header("Interactable Labels")]
    [SerializeField] private Vector3 interactableLabelOffset = new Vector3(0f, 1f, 0f);
    [SerializeField] private float interactableLabelWidth = 180f;
    [SerializeField] private float interactableLabelHeight = 25f;

    [Header("Refresh")]
    [SerializeField] private float refreshInterval = 0.25f;

    private GameManager gm;
    private Camera mainCamera;

    private bool isVisible;
    private GameRoom cachedRoom;

    private GameCondition[] conditions;
    private ConditionGate[] gates;
    private Interactable[] interactables;

    private float nextRefreshTime;

    private GUIStyle headerStyle;
    private GUIStyle normalStyle;
    private GUIStyle sectionStyle;
    private GUIStyle worldLabelStyle;

    private void Awake()
    {
        gm = FindAnyObjectByType<GameManager>();
        mainCamera = Camera.main;

        isVisible = startsVisible;
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard != null)
        {
            if (keyboard[toggleOverlayKey].wasPressedThisFrame)
            {
                isVisible = !isVisible;

                if (isVisible)
                {
                    RefreshCache();
                }
            }

            if (keyboard[toggleInteractableLabelKey].wasPressedThisFrame)
            {
                showInteractableLabels = !showInteractableLabels;
            }
        }

        if (!isVisible)
            return;

        if (gm == null)
            return;

        if (gm.CurrentRoom != cachedRoom)
        {
            RefreshCache();
            return;
        }

        if (Time.unscaledTime >= nextRefreshTime)
        {
            RefreshCache();
        }
    }

    private void RefreshCache()
    {
        nextRefreshTime = Time.unscaledTime + refreshInterval;

        if (gm == null)
            return;

        cachedRoom = gm.CurrentRoom;

        if (cachedRoom == null)
        {
            conditions = null;
            gates = null;
            interactables = null;
            return;
        }

        conditions = cachedRoom.GetComponentsInChildren<GameCondition>(true);
        gates = cachedRoom.GetComponentsInChildren<ConditionGate>(true);
        interactables = cachedRoom.GetComponentsInChildren<Interactable>(true);
    }

    private void OnGUI()
    {
        if (!isVisible)
            return;

        CreateStylesIfNeeded();

        DrawMainOverlay();

        if (showInteractableLabels)
            DrawInteractableLabels();
    }

    private void DrawMainOverlay()
    {
        const float width = 420f;
        const float margin = 10f;

        float height = CalculateOverlayHeight();

        GUILayout.BeginArea(new Rect(margin, margin, width, height), GUI.skin.box);

        GUILayout.Label("DEBUG OVERLAY", headerStyle);

        GUILayout.Label("F1: Toggle Overlay   F2: Interactable Labels", normalStyle);

        GUILayout.Space(8f);

        DrawRoomInformation();

        GUILayout.Space(10f);

        DrawConditions();

        GUILayout.Space(10f);

        DrawConditionGates();

        GUILayout.EndArea();
    }

    private void DrawRoomInformation()
    {
        GUILayout.Label("ROOM / CHECKPOINT", sectionStyle);

        if (gm == null)
        {
            GUILayout.Label("GameManager: NULL", normalStyle);

            return;
        }

        string roomName = "None";

        if (gm.CurrentRoom != null)
        {
            roomName = gm.CurrentRoom.RoomName;
        }

        GUILayout.Label($"Current Room: {roomName}", normalStyle);

        string majorCheckpoint = "None";

        if (gm.CurrentMajorCheckpoint != null)
        {
            majorCheckpoint = gm.CurrentMajorCheckpoint.CheckPointName;
        }

        GUILayout.Label($"Major Checkpoint: {majorCheckpoint}", normalStyle);

        string roomCheckpoint = "None";

        if (gm.CurrentRoom.CurrentCheckPoint != null)
        {
            roomCheckpoint = gm.CurrentRoom.CurrentCheckPoint.CheckPointName;
        }

        GUILayout.Label($"Room Checkpoint: {roomCheckpoint}", normalStyle);
    }

    private void DrawConditions()
    {
        GUILayout.Label("GAME CONDITIONS", sectionStyle);

        if (conditions == null || conditions.Length == 0)
        {
            GUILayout.Label("No GameConditions found.", normalStyle);

            return;
        }

        foreach (GameCondition condition in conditions)
        {
            if (condition == null)
                continue;

            string state = condition.IsMet ? "[MET]" : "[NOT MET]";

            GUILayout.Label($"{state} {condition.gameObject.name}", normalStyle);
        }
    }

    private void DrawConditionGates()
    {
        GUILayout.Label("CONDITION GATES", sectionStyle);

        if (gates == null || gates.Length == 0)
        {
            GUILayout.Label("No ConditionGates found.", normalStyle);

            return;
        }

        foreach (ConditionGate gate in gates)
        {
            if (gate == null)
                continue;

            string state = gate.CurrentResult ? "[TRUE]" : "[FALSE]";

            string onceText = gate.TriggerOnce ? "Once" : "Repeat";

            string triggeredText = gate.HasTriggered ? "Triggered" : "Not Triggered";

            GUILayout.Label($"{state} {gate.gameObject.name}", normalStyle);

            GUILayout.Label($"    {onceText} | {triggeredText}", normalStyle);
        }
    }

    private void DrawInteractableLabels()
    {
        if (interactables == null || mainCamera == null)
        {
            return;
        }

        foreach (Interactable interactable in interactables)
        {
            if (interactable == null)
                continue;

            if (!interactable.gameObject.activeInHierarchy)
                continue;

            Vector3 worldPosition = interactable.transform.position + interactableLabelOffset;

            Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);

            // Object is behind the camera.
            if (screenPosition.z <= 0f)
                continue;

            float x = screenPosition.x - interactableLabelWidth * 0.5f;

            // GUI coordinates have Y=0 at the top,
            // unlike WorldToScreenPoint.
            float y = Screen.height - screenPosition.y - interactableLabelHeight;

            Rect rect = new Rect(x, y, interactableLabelWidth, interactableLabelHeight);

            if (interactable.LimitUses)
                GUI.Label(rect, $"{interactable.gameObject.name} | Uses: {interactable.CurrentUseCount} / {interactable.MaxUses}", worldLabelStyle);
            else
                GUI.Label(rect, $"{interactable.gameObject.name} | Uses: Unlimited", worldLabelStyle);
        }
    }

    private float CalculateOverlayHeight()
    {
        int conditionCount = conditions?.Length ?? 0;

        int gateCount = gates?.Length ?? 0;

        // Each gate currently uses two text lines.
        float calculatedHeight = 170f + conditionCount * 22f + gateCount * 44f;

        // Prevent the panel from taking over the
        // entire screen in an unusually large room.
        return Mathf.Min(calculatedHeight, Screen.height - 20f);
    }

    private void CreateStylesIfNeeded()
    {
        if (normalStyle != null)
            return;

        normalStyle = new GUIStyle(GUI.skin.label);

        normalStyle.fontSize = 14;
        normalStyle.wordWrap = false;

        headerStyle = new GUIStyle(normalStyle);

        headerStyle.fontSize = 18;
        headerStyle.fontStyle = FontStyle.Bold;

        sectionStyle = new GUIStyle(normalStyle);

        sectionStyle.fontSize = 15;
        sectionStyle.fontStyle = FontStyle.Bold;

        worldLabelStyle = new GUIStyle(GUI.skin.box);

        worldLabelStyle.fontSize = 13;
        worldLabelStyle.alignment = TextAnchor.MiddleCenter;
    }

}