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
    [SerializeField] WorldSaveManager _worldSaveManager;

    [Header("UI")]
    [SerializeField] RectTransform _placementPanel;
    [SerializeField] ObjectPlacementControlsUI _placementControls;

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
    bool _isPlacing;
    public bool IsPlacing { get { return _isPlacing; } }
    bool _isFlipped;

    PlaceableObject _editingObject;

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

        if (_placementPanel == null)
            return;

        if (_placementControls == null)
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
        if (!_isReady || !_isPlacing)
            return;

        Pointer pointer = Pointer.current;

        if (pointer == null)
        {
            HidePreview();
            return;
        }

        if (pointer is Touchscreen touchscreen && !touchscreen.primaryTouch.press.isPressed)
            return;

        Vector2 screenPosition = pointer.position.ReadValue();

        if (_placementPanel.gameObject.activeInHierarchy && RectTransformUtility.RectangleContainsScreenPoint(_placementPanel, screenPosition))
            return;

        if (_placementControls.IsPointerOverButtons(screenPosition))
            return;

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

        RefreshPreviewAtCell(originCell);
    }

    void RefreshPreviewAtCell(Vector2Int originCell)
    {
        _previewCell = originCell;
        _hasPreviewCell = true;

        Vector2Int footprint = _selectedData.GetFootprint(_isFlipped);

        bool isValidZone = _zoneManager.CanPlaceInZone(_selectedData.Category, originCell, footprint);

        bool isAreaFree;

        if (_editingObject != null)
            isAreaFree = _gridOccupancy.IsAreaFreeExcept(originCell, footprint, _editingObject);
        else
            isAreaFree = _gridOccupancy.IsAreaFree(originCell, footprint);

        bool isStationAvailable = true;

        if (_editingObject == null && _selectedData.Category == ZoneManager.PlaceableCategory.Station)
            isStationAvailable = !_gridOccupancy.HasPlacedStation(_selectedData.Id);

        _canPlaceHere = isValidZone && isAreaFree && isStationAvailable;

        if (_canPlaceHere)
            _previewRenderer.color = _validPreviewColor;
        else
            _previewRenderer.color = _invalidPreviewColor;

        UpdateFootprintPreview(originCell, footprint);

        Vector3 firstCellCenter = _floorTilemap.GetCellCenterWorld(new Vector3Int(originCell.x, originCell.y, 0));

        Vector3 lastCellCenter = _floorTilemap.GetCellCenterWorld(new Vector3Int(originCell.x + footprint.x - 1, originCell.y + footprint.y - 1, 0));

        Vector3 previewPosition = (firstCellCenter + lastCellCenter) * 0.5f;

        Vector3 visualPosition = previewPosition + new Vector3(_selectedData.VisualOffset.x, _selectedData.VisualOffset.y, 0f);

        _previewRenderer.transform.position = visualPosition;
        _previewRenderer.transform.localScale = Vector3.one * _selectedData.VisualScale;

        _previewRenderer.sortingOrder = PlaceableObject.CalculateSortingOrder(previewPosition.y);

        _previewRenderer.enabled = true;

        _placementControls.ShowAtWorldPosition(previewPosition, _gameCamera);
    }

    void CreatePreview()
    {
        GameObject previewObject = new GameObject("PlacementPreview");

        previewObject.transform.SetParent(transform, false);

        _previewRenderer = previewObject.AddComponent<SpriteRenderer>();

        SpriteRenderer prefabRenderer = _placeablePrefab.GetComponentInChildren<SpriteRenderer>();

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

    public void SelectObject(PlaceableObjectData data)
    {
        if (data == null || data.Sprite == null)
            return;

        if (_editingObject != null)
            CancelPlacement();

        HidePreview();

        _selectedData = data;
        _isFlipped = false;
        _isPlacing = true;

        if (_previewRenderer != null)
        {
            _previewRenderer.sprite = _selectedData.Sprite;
            _previewRenderer.flipX = false;

            _previewRenderer.transform.localScale = Vector3.one * _selectedData.VisualScale;

            SetPlacementPanelVisible(false);
        }
    }

    public void BeginEdit(PlaceableObject placedObject)
    {
        if (!_isReady || _isPlacing || placedObject == null)
            return;

        PlaceableObjectData data = placedObject.Data;

        if (data == null || data.Sprite == null)
            return;

        HidePreview();

        _editingObject = placedObject;
        _selectedData = data;
        _isFlipped = placedObject.IsFlipped;
        _isPlacing = true;

        _previewRenderer.sprite = data.Sprite;
        _previewRenderer.flipX = _isFlipped;

        _previewRenderer.transform.localScale = Vector3.one * data.VisualScale;

        placedObject.SetVisualVisible(false);

        RefreshPreviewAtCell(placedObject.OriginCell);

        SetPlacementPanelVisible(false);
    }

    public void CancelPlacement()
    {
        if (_editingObject != null)
        {
            _editingObject.SetVisualVisible(true);
            _editingObject = null;
        }

        _isPlacing = false;

        HidePreview();

        SetPlacementPanelVisible(true);
    }

    public void ConfirmPlacement()
    {
        if (!_isReady || !_isPlacing)
            return;

        if (!_hasPreviewCell || !_canPlaceHere)
            return;

        Vector2Int originCell = _previewCell;
        Vector2Int footprint = _selectedData.GetFootprint(_isFlipped);

        bool isValidZone = _zoneManager.CanPlaceInZone(_selectedData.Category, originCell, footprint);

        bool isAreaFree;

        if (_editingObject != null)
            isAreaFree = _gridOccupancy.IsAreaFreeExcept(originCell, footprint, _editingObject);
        else
            isAreaFree = _gridOccupancy.IsAreaFree(originCell, footprint);

        bool isStationAvailable = true;

        if (_editingObject == null && _selectedData.Category == ZoneManager.PlaceableCategory.Station)
            isStationAvailable = !_gridOccupancy.HasPlacedStation(_selectedData.Id);

        if (!isValidZone || !isAreaFree || !isStationAvailable)
        {
            RefreshPreviewAtCell(originCell);
            return;
        }

        Vector3 firstCellCenter = _floorTilemap.GetCellCenterWorld(new Vector3Int(originCell.x, originCell.y, 0));

        Vector3 lastCellCenter = _floorTilemap.GetCellCenterWorld(new Vector3Int(originCell.x + footprint.x - 1, originCell.y + footprint.y - 1, 0));

        Vector3 spawnPosition = (firstCellCenter + lastCellCenter) * 0.5f;

        if (_editingObject != null)
        {
            ConfirmEdit(originCell, footprint, spawnPosition);
            return;
        }


        PlaceableObject placedObject = Instantiate(_placeablePrefab, spawnPosition, Quaternion.identity);

        placedObject.Initialize(_selectedData, originCell);
        placedObject.SetFlipped(_isFlipped);

        if (!_gridOccupancy.TryOccupy(placedObject, originCell, footprint))
        {
            Destroy(placedObject.gameObject);

            _hasPreviewCell = false;
            return;
        }

        _isPlacing = false;

        HidePreview();

        SetPlacementPanelVisible(true);

        if (_worldSaveManager != null)
            _worldSaveManager.SaveWorld();
    }


    void ConfirmEdit(Vector2Int newOriginCell, Vector2Int newFootprint, Vector3 newWorldPosition)
    {
        PlaceableObject placedObject = _editingObject;

        Vector2Int oldOriginCell = placedObject.OriginCell;
        Vector2Int oldFootprint = placedObject.Footprint;
        Vector3 oldWorldPosition = placedObject.transform.position;
        bool oldFlipped = placedObject.IsFlipped;

        _gridOccupancy.Release(placedObject);

        placedObject.SetOriginCell(newOriginCell);
        placedObject.SetFlipped(_isFlipped);
        placedObject.transform.position = newWorldPosition;
        placedObject.RefreshSortingOrder();

        bool success = _gridOccupancy.TryOccupy(placedObject, newOriginCell, newFootprint);

        if (!success)
        {
            placedObject.SetOriginCell(oldOriginCell);
            placedObject.SetFlipped(oldFlipped);
            placedObject.transform.position = oldWorldPosition;
            placedObject.RefreshSortingOrder();

            bool restored = _gridOccupancy.TryOccupy(placedObject, oldOriginCell, oldFootprint);

            if (!restored)
                Debug.LogError("Failed to restore object occupancy.", placedObject);
        }

        placedObject.SetVisualVisible(true);

        _editingObject = null;
        _isPlacing = false;

        HidePreview();

        SetPlacementPanelVisible(true);

        if (success && _worldSaveManager != null)
            _worldSaveManager.SaveWorld();
    }

    void HidePreview()
    {
        if (_previewRenderer != null)
            _previewRenderer.enabled = false;

        if (_hasPreviewCell && _footprintTilemap != null)
            _footprintTilemap.ClearAllTiles();

        _hasPreviewCell = false;
        _canPlaceHere = false;

        if (_placementControls != null)
            _placementControls.Hide();
    }

    public void FlipPlacement()
    {
        if (!_isReady || !_isPlacing)
            return;

        _isFlipped = !_isFlipped;

        if (_previewRenderer != null)
            _previewRenderer.flipX = _isFlipped;

        if (_hasPreviewCell)
            RefreshPreviewAtCell(_previewCell);
    }
    void OnDisable()
    {
        if (_editingObject != null)
        {
            _editingObject.SetVisualVisible(true);
            _editingObject = null;
        }

        _isPlacing = false;

        HidePreview();
    }
    void SetPlacementPanelVisible(bool visible)
    {
        if (_placementPanel != null)
            _placementPanel.gameObject.SetActive(visible);
    }

    public void TogglePlacementPanel()
    {
        if (_isPlacing || _placementPanel == null)
            return;

        bool isVisible = _placementPanel.gameObject.activeSelf;

        SetPlacementPanelVisible(!isVisible);
    }
}