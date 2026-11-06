using UnityEngine;

public class PlaceableObject : MonoBehaviour
{
    // Constants

    const float SORTING_ORDER_PER_WORLD_UNIT = 100f;

    // Local classes / structs

    // Enums

    // Variables

    PlaceableObjectData _data;

    public PlaceableObjectData Data { get { return _data; } }

    [SerializeField] SpriteRenderer _spriteRenderer;

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

    public void Initialize(PlaceableObjectData data, Vector2Int originCell)
    {
        if (data == null)
            return;

        _data = data;
        _originCell = originCell;
        _isFlipped = false;

        _spriteRenderer.sprite = _data.Sprite;

        ApplyVisualSettings();
        RefreshSortingOrder();
    }

    void ApplyVisualSettings()
    {
        if (_data == null || _spriteRenderer == null)
            return;

        Transform visualTransform = _spriteRenderer.transform;

        visualTransform.localPosition = new Vector3(_data.VisualOffset.x, _data.VisualOffset.y, 0f);

        visualTransform.localScale = Vector3.one * _data.VisualScale;
    }

    public static int CalculateSortingOrder(float worldY)
    {
        int sortingOrder = Mathf.RoundToInt(-worldY * SORTING_ORDER_PER_WORLD_UNIT);

        return sortingOrder;
    }

    public void RefreshSortingOrder()
    {
        if (_spriteRenderer == null)
            return;

        _spriteRenderer.sortingOrder = CalculateSortingOrder(transform.position.y);
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
    public void SetVisualVisible(bool isVisible)
    {
        if (_spriteRenderer == null)
            return;

        _spriteRenderer.enabled = isVisible;
    }
}