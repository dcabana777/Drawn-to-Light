
using System.Collections;
using UnityEngine;

public class GearRotation : MonoBehaviour, IRoomResettable
{
    public enum SpinDirection
    {
        Clockwise = -1,
        CounterClockwise = 1
    }

    [Header("Rotation Settings")]
    [SerializeField, Min(0f)] private float spinSpeed = 90f;

    [SerializeField] private SpinDirection spinDirection = SpinDirection.Clockwise;

    private Coroutine rotationCoroutine;

    private Quaternion savedGearRotation;

    public void RotateGear(float duration)
    {
        // Prevent overlapping rotation coroutines.
        if (rotationCoroutine != null)
        {
            StopCoroutine(rotationCoroutine);
        }

        if (duration <= 0f)
        {
            rotationCoroutine = null;
            return;
        }

        rotationCoroutine = StartCoroutine(RotateForDuration(duration));
    }

    private IEnumerator RotateForDuration(float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float deltaTime = Mathf.Min(Time.deltaTime, duration - elapsed);

            float rotation = spinSpeed * (int)spinDirection * deltaTime;

            transform.Rotate(0f, 0f, rotation);

            elapsed += deltaTime;

            yield return null;
        }

        rotationCoroutine = null;
    }

    public void StopGear()
    {
        if (rotationCoroutine != null)
        {
            StopCoroutine(rotationCoroutine);
            rotationCoroutine = null;
        }
    }

    public void SaveState()
    {
        savedGearRotation = transform.localRotation;
    }

    public void RestoreState()
    {
        if (rotationCoroutine != null)
        {
            StopCoroutine(rotationCoroutine);
            rotationCoroutine= null;
        }

        transform.localRotation = savedGearRotation;
    }
}
