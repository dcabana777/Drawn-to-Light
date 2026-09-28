using UnityEngine;

public class ObjectToggleInteract : Interactable
{
    [SerializeField] private GameObject[] objectsToActivate;
    [SerializeField] private bool[] setObjectActive;

    protected override void Awake()
    {
        base.Awake();

        if (objectsToActivate.Length != setObjectActive.Length)
        {
            Debug.LogError("ObjectToggleTrigger does not have matching numbers of " +
                "objects to activate and object activation booleans!");
        }
    }


    protected override bool OnInteract()
    {
        int i = 0;
        
        foreach (GameObject gameObject in objectsToActivate)
        {
            if (gameObject != null)
                gameObject.SetActive(setObjectActive[i]);
            i++;
        }

        return true;
    }
}
