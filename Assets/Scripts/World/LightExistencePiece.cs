using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightExistencePiece : MonoBehaviour
{
    private bool exists;
    [SerializeField, Min(1)] private int requiredLitDetectors;
    [SerializeField] private float disappearanceDelay = 0.15f;

    private Coroutine disappearanceCoroutine;


    private HashSet<LightDetector> litDetectors = new();
    private Rigidbody2D rb;
    private SpriteRenderer renderer;
    private Collider2D col;

    public event Action<bool> ExistenceChanged;
    public bool Exists => exists;


    private void Awake()
    {
        exists = false;

        rb = GetComponent<Rigidbody2D>();
        renderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();

        bool shouldExist = litDetectors.Count >= requiredLitDetectors;
        SetExistence(shouldExist);
    }

    private void UpdateExistence()
    {        
        bool shouldExist = litDetectors.Count >= requiredLitDetectors;

        if (shouldExist)
        {
            if (disappearanceCoroutine != null)
            {
                StopCoroutine(disappearanceCoroutine);
                disappearanceCoroutine = null;
            }

            SetExistence(true);
        }
        else
        {
            if (exists && disappearanceCoroutine == null)
            {
                disappearanceCoroutine = StartCoroutine(DisappearAfterDelay());
            }
        }
    }

    private IEnumerator DisappearAfterDelay()
    {
        yield return new WaitForSeconds(disappearanceDelay);

        disappearanceCoroutine = null;

        // check again in case something changed
        if (litDetectors.Count < requiredLitDetectors)
        {
            SetExistence(false);
        }
    }

    private void SetExistence(bool shouldExist)
    {
        exists = shouldExist;
        
        if (rb != null) rb.simulated = shouldExist;
        if (renderer != null) renderer.enabled = shouldExist;
        if (col != null) col.enabled = shouldExist;

        ExistenceChanged?.Invoke(Exists);
    }

    public void SetDetectorStatus(LightDetector detector, bool isLit)
    {
        if (isLit)
        {
            litDetectors.Add(detector);
        }
        else
        {
            litDetectors.Remove(detector);
        }
        UpdateExistence();
    }






}
