using UnityEngine;
using System.Collections;

public class ActivateForSeconds : MonoBehaviour
{
    [SerializeField] GameObject targetObject;

    public void TriggerTimedActivation(float duration)
    {
        StartCoroutine(ActivateRoutine(duration));
    }

    private IEnumerator ActivateRoutine(float duration)
    {
        if (targetObject == null)
            yield return null;

        targetObject.SetActive(true);

        yield return new WaitForSeconds(duration);

        targetObject.SetActive(false);
    }
}
