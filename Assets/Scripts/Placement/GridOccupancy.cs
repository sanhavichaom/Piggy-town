using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridOccupancy : MonoBehaviour
{
    // Constants

    // Local classes / structs

    // Enums

    // Variables

    [Header("References")]
    [SerializeField] Tilemap _floorTilemap;

    PlaceableObject[,] _occupiedCells;

    Dictionary<PlaceableObject, RectInt> _objectAreas = new Dictionary<PlaceableObject, RectInt>();

    Vector2Int _minCell;
    int _width;
    int _height;
    bool _isReady;

    // Methods

    void Start()
    {
        if (_floorTilemap == null)
            return;

        BoundsInt bounds = _floorTilemap.cellBounds;

        if (bounds.size.x <= 0 || bounds.size.y <= 0)
            return;

        _minCell = new Vector2Int(bounds.xMin, bounds.yMin);
        _width = bounds.size.x;
        _height = bounds.size.y;

        _occupiedCells = new PlaceableObject[_width, _height];
        _isReady = true;
    }

    public bool IsAreaFree(Vector2Int originCell, Vector2Int footprint)
    {
        if (!_isReady)
            return false;

        if (footprint.x <= 0 || footprint.y <= 0)
            return false;

        for (int x = 0; x < footprint.x; x++)
        {
            for (int y = 0; y < footprint.y; y++)
            {
                int gridX = originCell.x + x - _minCell.x;
                int gridY = originCell.y + y - _minCell.y;

                if (gridX < 0 || gridY < 0 || gridX >= _width || gridY >= _height)
                    return false;

                if (_occupiedCells[gridX, gridY] != null)
                    return false;

            }
        }
        return true;
    }
    public bool TryOccupy(PlaceableObject placeableObject, Vector2Int originCell, Vector2Int footprint)
    {
        if (!_isReady || placeableObject == null)
            return false;

        if (_objectAreas.ContainsKey(placeableObject))
            return false;

        if (!IsAreaFree(originCell, footprint))
            return false;

        for (int x = 0; x < footprint.x; x++)
        {
            for (int y = 0; y < footprint.y; y++)
            {
                int gridX = originCell.x + x - _minCell.x;
                int gridY = originCell.y + y - _minCell.y;

                _occupiedCells[gridX, gridY] = placeableObject;
            }
        }

        RectInt area = new RectInt(originCell.x, originCell.y, footprint.x, footprint.y);

        _objectAreas.Add(placeableObject, area);

        return true;
    }

    public void Release(PlaceableObject placeableObject)
    {
        if (!_isReady || placeableObject == null)
            return;

        if (!_objectAreas.TryGetValue(placeableObject, out RectInt area))
            return;

        for (int x = 0; x < area.width; x++)
        {
            for (int y = 0; y < area.height; y++)
            {
                int gridX = area.xMin + x - _minCell.x;
                int gridY = area.yMin + y - _minCell.y;

                if (_occupiedCells[gridX, gridY] == placeableObject)
                    _occupiedCells[gridX, gridY] = null;
            }
        }

        _objectAreas.Remove(placeableObject);
    }
}
