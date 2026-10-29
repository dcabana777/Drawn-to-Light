using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class DelayEvent : MonoBehaviour
{
    [SerializeField] private UnityEvent delayedEvent;

    private Coroutine delayedCoroutine;

    public void CallDelayedEvent(float delay)
    {
        delayedCoroutine = StartCoroutine(DelayEventForSeconds(delay));
    }

    private IEnumerator DelayEventForSeconds(float delay)
    {
        yield return new WaitForSeconds(delay);

        delayedEvent.Invoke();

        delayedCoroutine = null;
    }

    private void OnDisable()
    {
        StopCoroutine(delayedCoroutine);
        delayedCoroutine = null;
    }
}
