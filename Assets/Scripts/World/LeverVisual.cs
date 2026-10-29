using System.Collections;
using UnityEngine;

public class LeverVisual : MonoBehaviour, IRoomResettable
{
    [SerializeField] private Transform leverPivot;
    [SerializeField] private Collider2D leverCollider;

    [SerializeField] private float unpulledAngle = -25f;
    [SerializeField] private float pulledAngle = 25f;

    [SerializeField] private float rotationDuration = 0.25f;

    private bool isPulled = false;
    private bool savedIsPulled;

    private bool isLeverUsable = true;
    private bool savedIsLeverUsable;

    private Coroutine rotationRoutine;

    private Quaternion initialLocalRotation;
    private float currentAngle;

    private void Awake()
    {
        initialLocalRotation = leverPivot.localRotation;
        currentAngle = unpulledAngle;
    }
    
    public void Pull()
    {
        if (!isLeverUsable)
            return;

        RotateTo(pulledAngle);
        isPulled = true;
    }

    public void ResetLever()
    {
        if (!isLeverUsable)
            return;

        RotateTo(unpulledAngle);
        isPulled = false;
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
        float startAngle = currentAngle;
        float timer = 0f;

        while (timer < rotationDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / rotationDuration);

            currentAngle = Mathf.LerpAngle(startAngle, targetAngle, t);

            ApplyRotation();

            yield return null;
        }

        currentAngle = targetAngle;
        ApplyRotation();

        rotationRoutine = null;
    }

    private void ApplyRotation()
    {
        float relativeAngle = currentAngle - unpulledAngle;

        leverPivot.localRotation = initialLocalRotation * Quaternion.Euler(1f, 1f, relativeAngle);
    }
    public void SetLeverUsability(bool isUsable)
    {
        isLeverUsable = isUsable;
        leverCollider.enabled = isUsable;
    }

    public void SaveState()
    {
        savedIsPulled = isPulled;
        savedIsLeverUsable = isLeverUsable;
    }

    public void RestoreState()
    {
        if (rotationRoutine != null)
        {
            StopCoroutine(rotationRoutine);
            rotationRoutine = null;
        }
        
        isPulled = savedIsPulled;
        SetLeverUsability(savedIsLeverUsable);

        currentAngle = isPulled ? pulledAngle : unpulledAngle;

        ApplyRotation();
    }
}