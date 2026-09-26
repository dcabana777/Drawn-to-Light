using UnityEngine;

public class Spintest : MonoBehaviour
{
    [Tooltip("Rotation speed around the X, Y, and Z axes in degrees per second.")]
    [SerializeField] private Vector3 rotationSpeed = new Vector3(0f, 50f, 0f);

    void Update()
    {
        // Rotates the object every frame, frame-rate independent
        transform.Rotate(rotationSpeed * Time.deltaTime);
    }
}