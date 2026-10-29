using System.Collections;
using UnityEngine;

public class LeverVisual : MonoBehaviour
{
    [SerializeField] private Transform leverPivot;

    [SerializeField] private float unpulledAngle = -25f;
    [SerializeField] private float pulledAngle = 25f;

    [SerializeField] private float rotationDuration = 0.25f;

    private Coroutine rotationRoutine;

    public void Pull()
    {
        RotateTo(pulledAngle);
    }

    public void ResetLever()
    {
        RotateTo(unpulledAngle);
    }

    private void RotateTo(float targetAngle)
    {
        if (rotationRoutine != null)
        {
            StopCoroutine(rotationRoutine);
        }

        rotationRoutine = StartCoroutine(RotateRoutine(targetAngle));
    }

    private IEnumerator RotateRoutine(float targetAngle)
    {
        float startAngle = leverPivot.localEulerAngles.z;

        // Convert 0–360 into a useful signed angle.
        startAngle = Mathf.DeltaAngle(0f, startAngle);

        float timer = 0f;

        while (timer < rotationDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / rotationDuration);

            float angle = Mathf.LerpAngle(startAngle, targetAngle, t);

            leverPivot.localRotation = Quaternion.Euler(0f, 0f, angle);

            yield return null;
        }

        leverPivot.localRotation = Quaternion.Euler(0f, 0f, targetAngle);

        rotationRoutine = null;
    }
}