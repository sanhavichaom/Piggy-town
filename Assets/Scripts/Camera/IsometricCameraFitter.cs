using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Camera))]
public class IsometricCameraFitter : MonoBehaviour
{
    // Constants

    // Local classes / structs

    // Enums

    // Variables

    [Header("References")]
    [SerializeField] Tilemap _floorTilemap;

    [Header("Camera Setting")]
    [SerializeField, Range(0f, 0.5f)]  float _padding = 0.1f;

    // Methods
    void Start()
    {
        if (_floorTilemap == null)
            return;

        Bounds bounds = _floorTilemap.localBounds;

        if (bounds.size.x <= 0 || bounds.size.y <= 0)
            return;

        Camera gameCamera = GetComponent<Camera>();

        Vector3 center = _floorTilemap.transform.TransformPoint(bounds.center);

        transform.position = new Vector3(center.x, center.y, transform.position.z);

        gameCamera.orthographic = true;

        float halfHeight = bounds.extents.y;
        float halfWidth = bounds.extents.x / gameCamera.aspect;

        gameCamera.orthographicSize = Mathf.Max(halfHeight, halfWidth) * (1.0f + _padding);
    }
}