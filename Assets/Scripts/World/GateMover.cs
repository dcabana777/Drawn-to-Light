using System.Collections;
using UnityEngine;

public class GateMover : MonoBehaviour
{
    [SerializeField] private Transform gate;

    [Header("Movement")]
    [SerializeField] private Vector2 openOffset = new Vector2(0f, -5f);
    [SerializeField] private float moveDuration = 1.5f;
    [SerializeField] private AnimationCurve movementCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private Vector3 closedPosition;
    private Vector3 openPosition;

    private Coroutine moveRoutine;

    private void Awake()
    {
        closedPosition = gate.localPosition;

        openPosition = closedPosition + (Vector3)openOffset;
    }

    public void OpenGate()
    {
        MoveGate(openPosition);
    }

    public void CloseGate()
    {
        MoveGate(closedPosition);
    }

    private void MoveGate(Vector3 destination)
    {
        if (moveRoutine != null)
        {
            StopCoroutine(moveRoutine);
        }

        moveRoutine = StartCoroutine(MoveRoutine(destination));
    }

    private IEnumerator MoveRoutine(Vector3 destination)
    {
        Vector3 start = gate.localPosition;

        float timer = 0f;

        while (timer < moveDuration)
        {
            timer += Time.deltaTime;

            float normalizedTime = Mathf.Clamp01(timer / moveDuration);

            float curvedTime = movementCurve.Evaluate(normalizedTime);

            gate.localPosition = Vector3.Lerp(start, destination, curvedTime);

            yield return null;
        }

        gate.localPosition = destination;

        moveRoutine = null;
    }
}