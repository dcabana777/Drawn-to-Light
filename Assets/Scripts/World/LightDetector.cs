using System.Collections.Generic;
using UnityEngine;

public class LightDetector : MonoBehaviour
{
    private LightExistencePiece detectorSource;
    private HashSet<Collider2D> overlappingLights = new();
    private bool detectorActive => (overlappingLights.Count > 0);

    private void Start()
    {
        detectorSource = GetComponentInParent<LightExistencePiece>();

        if (detectorSource == null)
        {
            Debug.LogWarning("LightExistencePiece could not be found anywhere in parent hierarchy!");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        overlappingLights.Add(collision);

        if (detectorActive)
        {
            detectorSource.SetDetectorStatus(gameObject.GetComponent<LightDetector>(), detectorActive);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        overlappingLights.Remove(collision);

        if (!detectorActive)
        {
            detectorSource.SetDetectorStatus(gameObject.GetComponent<LightDetector>(), detectorActive);
        }
    }
}
