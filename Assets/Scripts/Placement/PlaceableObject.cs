using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(SpriteRenderer))]
public class PlaceableObject : MonoBehaviour
{
    // Constants

    // Local classes / structs

    // Enums

    // Variables

    PlaceableObjectData _data;

    public PlaceableObjectData Data { get { return _data; } }

    SpriteRenderer _spriteRenderer;

    Vector2Int _originCell;
    public Vector2Int OriginCell { get { return _originCell; } }

    bool _isFlipped;

    public bool IsFlipped { get { return _isFlipped; } }

    public Vector2Int Footprint
    {
        get
        {
            if (_data == null)
                return Vector2Int.zero;

            return _data.GetFootprint(_isFlipped);
        }
    }

    // Methods
    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(PlaceableObjectData data, Vector2Int originCell)
    {
        if (data == null)
            return;

        _data = data;
        _originCell = originCell;
        _isFlipped = false;

        _spriteRenderer.sprite = _data.Sprite;
    }

    public void SetOriginCell(Vector2Int originCell)
    {
        _originCell = originCell;
    }
    public void SetFlipped(bool isFlipped)
    {
        _isFlipped = isFlipped;

        if (_spriteRenderer != null)
            _spriteRenderer.flipX = _isFlipped;
    }
}
