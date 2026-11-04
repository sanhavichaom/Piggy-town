using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;

public class PlacementManager : MonoBehaviour
{
    // Constants

    // Local classes / structs

    // Enums

    // Variables

    [Header("Reference")]
    [SerializeField] Camera _gameCamera;
    [SerializeField] Tilemap _floorTilemap;
    [SerializeField] ZoneManager _zoneManager;
    [SerializeField] GridOccupancy _gridOccupancy;

    [Header("Placeable Object")]
    [SerializeField] PlaceableObject _placeablePrefab;
    [SerializeField] PlaceableObjectData _selectedData;

    [Header("Preview Colors")]
    [SerializeField] Color _validPreviewColor = new Color(0.4f, 1f, 0.4f, 0.6f);
    [SerializeField] Color _invalidPreviewColor = new Color(1f, 0.4f, 0.4f, 0.6f);

    SpriteRenderer _previewRenderer;
    Tilemap _footprintTilemap;

    [Header("Footprint Colors")]

    [SerializeField] Color _validFootprintColor = new Color(0f, 1f, 0f, 0.4f);

    [SerializeField] Color _invalidFootprintColor = new Color(1f, 0f, 0f, 0.4f);

    Vector2Int _previewCell;

    bool _hasPreviewCell;
    bool _canPlaceHere;
    bool _isReady;

    // Methods

    void Start()
    {
        if (_gameCamera == null)
            return;

        if (_floorTilemap == null)
            return;

        if (_zoneManager == null)
            return;

        if (_gridOccupancy == null)
            return;

        if (_placeablePrefab == null)
            return;

        if (_selectedData == null)
            return;

        if (_selectedData.Sprite == null)
            return;

        CreatePreview();
        CreateFootprintPreview();

        _isReady = true;
    }

    void Update()
    {
        if (!_isReady)
            return;

        Pointer pointer = Pointer.current;

        if(pointer == null)
        {
            HidePreview();
            return;
        }

        if (pointer is Touchscreen touchscreen && !touchscreen.primaryTouch.press.isPressed)
        {
            HidePreview();
            return;
        }

        Vector2 screenPosition = Pointer.current.position.ReadValue();

        if (!_gameCamera.pixelRect.Contains(screenPosition))
        {
            HidePreview();
            return;
        }

        UpdatePreview(screenPosition);
    }

    void UpdatePreview(Vector2 screenPosition)
    {
        float distanceToFloor = _floorTilemap.transform.position.z - _gameCamera.transform.position.z;

        Vector3 worldPosition = _gameCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, distanceToFloor));

        Vector3Int tileCell = _floorTilemap.WorldToCell(worldPosition);

        if (!_floorTilemap.HasTile(tileCell))
        {
            HidePreview();
            return;
        }

        Vector2Int originCell = new Vector2Int(tileCell.x, tileCell.y);

        if (_hasPreviewCell && originCell == _previewCell)
            return;

        _previewCell = originCell;
        _hasPreviewCell = true;

        Vector2Int footprint = _selectedData.GetFootprint(false);

        bool isValidZone = _zoneManager.CanPlaceInZone(_selectedData.Category, originCell, footprint);

        bool isAreaFree = _gridOccupancy.IsAreaFree(originCell, footprint);

        _canPlaceHere = isValidZone && isAreaFree;

        if (_canPlaceHere)
            _previewRenderer.color = _validPreviewColor;
        else
            _previewRenderer.color = _invalidPreviewColor;

        UpdateFootprintPreview(originCell, footprint);

        Vector3 firstCellCenter = _floorTilemap.GetCellCenterWorld(new Vector3Int(originCell.x, originCell.y, 0));

        Vector3 lastCellCenter = _floorTilemap.GetCellCenterWorld(new Vector3Int(originCell.x + footprint.x - 1, originCell.y + footprint.y - 1, 0));

        Vector3 previewPosition = (firstCellCenter + lastCellCenter) * 0.5f;

        _previewRenderer.transform.position = previewPosition;
        _previewRenderer.enabled = true;
    }

    void CreatePreview()
    {
        GameObject previewObject = new GameObject("PlacementPreview");

        previewObject.transform.SetParent(transform, false);

        _previewRenderer = previewObject.AddComponent<SpriteRenderer>();

        SpriteRenderer prefabRenderer = _placeablePrefab.GetComponent<SpriteRenderer>();

        _previewRenderer.sprite = _selectedData.Sprite;

        _previewRenderer.sharedMaterial = prefabRenderer.sharedMaterial;

        _previewRenderer.sortingLayerID = prefabRenderer.sortingLayerID;

        _previewRenderer.sortingOrder = prefabRenderer.sortingOrder;

        _previewRenderer.color = _validPreviewColor;
        _previewRenderer.enabled = false;

    }
    void CreateFootprintPreview()
    {
        GameObject footprintObject = new GameObject("FootprintPreview");

        Transform floorTransform = _floorTilemap.transform;

        footprintObject.transform.SetParent(floorTransform.parent, false);

        footprintObject.transform.localPosition = floorTransform.localPosition;
        footprintObject.transform.localRotation = floorTransform.localRotation;
        footprintObject.transform.localScale = floorTransform.localScale;

        _footprintTilemap = footprintObject.AddComponent<Tilemap>();

        _footprintTilemap.tileAnchor = _floorTilemap.tileAnchor;

        TilemapRenderer footprintRenderer =
            footprintObject.AddComponent<TilemapRenderer>();

        TilemapRenderer floorRenderer =
            _floorTilemap.GetComponent<TilemapRenderer>();

        footprintRenderer.sharedMaterial = floorRenderer.sharedMaterial;
        footprintRenderer.sortingLayerID = floorRenderer.sortingLayerID;
        footprintRenderer.sortingOrder = floorRenderer.sortingOrder + 1;
    }

    void UpdateFootprintPreview(Vector2Int originCell, Vector2Int footprint)
    {
        _footprintTilemap.ClearAllTiles();

        Color footprintColor;

        if (_canPlaceHere)
            footprintColor = _validFootprintColor;
        else
            footprintColor = _invalidFootprintColor;

        TileBase previewTile = _floorTilemap.GetTile(
            new Vector3Int(originCell.x, originCell.y, 0));

        if (previewTile == null)
            return;

        for (int x = 0; x < footprint.x; x++)
        {
            for (int y = 0; y < footprint.y; y++)
            {
                Vector3Int cell = new Vector3Int(
                    originCell.x + x,
                    originCell.y + y,
                    0
                );

                _footprintTilemap.SetTile(cell, previewTile);
                _footprintTilemap.SetTileFlags(cell, TileFlags.None);
                _footprintTilemap.SetColor(cell, footprintColor);
            }
        }
    }

    void HidePreview()
    {
        if (_previewRenderer != null)
            _previewRenderer.enabled = false;

        if (_hasPreviewCell && _footprintTilemap != null)
            _footprintTilemap.ClearAllTiles();

        _hasPreviewCell = false;
        _canPlaceHere = false;
    }

    void OnDisable()
    {
        HidePreview();
    }
}
