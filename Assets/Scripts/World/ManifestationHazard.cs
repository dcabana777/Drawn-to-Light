using Unity.VisualScripting;
using UnityEditor.U2D.Aseprite;
using UnityEngine;

public class ManifestationHazard : MonoBehaviour
{
    private LightExistencePiece existencePiece;
    private GameManager gm;

    private bool playerInside;

    private void Awake()
    {
        existencePiece = GetComponentInParent<LightExistencePiece>();

        if (existencePiece == null)
        {
            Debug.LogError("ManifestationHazard requires a LightExistencePiece parent.");
        }

        gm = FindAnyObjectByType<GameManager>();

        if (gm == null)
        {
            Debug.LogError("ManifestationHazard could not find Gamemanager!");
        }
    }

    private void OnEnable()
    {
        if (existencePiece != null)
        {
            existencePiece.ExistenceChanged += OnExistenceChanged;
        }
    }

    private void OnDisable()
    {
        if (existencePiece != null)
        {
            existencePiece.ExistenceChanged -= OnExistenceChanged;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = true;

        // also protects against player somehow entering an already-existing hazardous piece
        if (existencePiece.Exists)
            gm.RespawnPlayer();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = false;
    }

    private void OnExistenceChanged(bool nowExists)
    {
        if (nowExists && playerInside)
        {
            gm.RespawnPlayer();
        }
    }
}
