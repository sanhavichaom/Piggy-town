using UnityEngine;
using UnityEngine.Tilemaps;

public class ZoneManager : MonoBehaviour
{
    // Constants

    // Local classes / structs

    // Enums

    public enum ZoneType
    {
        None,
        Furniture,
        Mixed
    }

    public enum PlaceableCategory
    {
        Furniture,
        Station
    }

    // Variables

    [Header("Reference")]
    [SerializeField] Tilemap _floorTilemap;

    [Header("Zone Setting")]
    [SerializeField] RectInt _furnitureZone;
    [SerializeField] RectInt _mixedZone;

    [Header("Zone Colors")]
    [SerializeField]
    private Color _furnitureColor = Color.blueViolet;

    [SerializeField]
    private Color _mixedColor = Color.greenYellow;

    bool _isReady;

    // Methods

    void Start()
    {
        if (_floorTilemap == null)
            return;

        BoundsInt floorBounds = _floorTilemap.cellBounds;

        if (!IsZoneInsideFloor(_furnitureZone, floorBounds) || !IsZoneInsideFloor(_mixedZone, floorBounds))
            return;

        if (_furnitureZone.Overlaps(_mixedZone))
            return;

        _isReady = true;

        PaintZone(_furnitureZone, _furnitureColor);
        PaintZone(_mixedZone, _mixedColor);
    }

    public ZoneType GetZoneAtCell(Vector2Int cell)
    {
        if (!_isReady)
            return ZoneType.None;

        if (_furnitureZone.Contains(cell))
            return ZoneType.Furniture;

        if (_mixedZone.Contains(cell))
            return ZoneType.Mixed;

        return ZoneType.None;
    }

    public bool CanPlaceInZone(PlaceableCategory category, Vector2Int originCell, Vector2Int footprint)
    {
        if (!_isReady)
            return false;

        if (footprint.x <= 0 || footprint.y <= 0)
            return false;

        for (int x = 0; x < footprint.x; x++)
        {
            for (int y = 0; y < footprint.y; y++)
            {
                Vector2Int cell = originCell + new Vector2Int(x, y);

                ZoneType zone = GetZoneAtCell(cell);

                if (zone == ZoneType.None)
                    return false;

                if (category == PlaceableCategory.Station && zone != ZoneType.Mixed)
                    return false;
            }
        }
        return true;
    }

    bool IsZoneInsideFloor(RectInt zone, BoundsInt floorBounds)
    {
        return zone.width > 0 && zone.height > 0 &&
               zone.xMin >= floorBounds.xMin &&
               zone.yMin >= floorBounds.yMin &&
               zone.xMax <= floorBounds.xMax &&
               zone.yMax <= floorBounds.yMax;
    }

    void PaintZone(RectInt zone, Color color)
    {
        for (int x = zone.xMin; x < zone.xMax; x++)
        {
            for (int y = zone.yMin; y < zone.yMax; y++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);

                bool isWhite = (x + y) % 2 == 0;

                Color tileColor = isWhite ? Color.Lerp(color, Color.white, 0.2f) : color;

                _floorTilemap.SetColor(cell, tileColor);
            }
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (_floorTilemap == null)
            return;

        DrawZoneOutline(_furnitureZone, Color.blue);
        DrawZoneOutline(_mixedZone, Color.green);
    }

    void DrawZoneOutline(RectInt zone, Color color)
    {
        if (zone.width <= 0 || zone.height <= 0)
            return;

        Vector3 a = _floorTilemap.CellToWorld(new Vector3Int(zone.xMin, zone.yMin, 0));

        Vector3 b = _floorTilemap.CellToWorld(new Vector3Int(zone.xMax, zone.yMin, 0));

        Vector3 c = _floorTilemap.CellToWorld(new Vector3Int(zone.xMax, zone.yMax, 0));

        Vector3 d = _floorTilemap.CellToWorld(new Vector3Int(zone.xMin, zone.yMax, 0));

        Gizmos.color = color;
        Gizmos.DrawLine(a, b);
        Gizmos.DrawLine(b, c);
        Gizmos.DrawLine(c, d);
        Gizmos.DrawLine(d, a);
    }
#endif
}