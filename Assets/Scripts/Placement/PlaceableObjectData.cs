using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "PlaceableObject", menuName = "PiggyTown/Placeable Object")]
public class PlaceableObjectData : ScriptableObject
{
    // Constants

    // Local classes / structs


    // Enums

    // Variables

    [Header("Object Information")]
    [SerializeField] string _id;
    public string Id { get { return _id; } }

    [SerializeField] string _displayName;
    public string DisplayName { get { return _displayName; } }

    [SerializeField] ZoneManager.PlaceableCategory _category;
    public ZoneManager.PlaceableCategory Category { get { return _category; } }

    [SerializeField] Sprite _sprite;
    public Sprite Sprite { get { return _sprite; } }

    [SerializeField] Vector2Int _footprint = Vector2Int.one;
    public Vector2Int Footprint { get { return _footprint; } }

    [Header("Visual Settings")]
    [SerializeField] Vector2 _visualOffset = Vector2.zero;
    public Vector2 VisualOffset { get { return _visualOffset; } }
    [SerializeField] float _visualScale = 1f;
    public float VisualScale { get { return _visualScale; } }

    // Methods

    public Vector2Int GetFootprint(bool isFlipped)
    {
        if (isFlipped)
            return new Vector2Int(_footprint.y, _footprint.x);

        return _footprint;
    }

    void OnValidate()
    {
        _footprint.x = Mathf.Max(1, _footprint.x);
        _footprint.y = Mathf.Max(1, _footprint.y);
        _visualScale = Mathf.Max(0.01f, _visualScale);
    }
}