using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class ObjectInteractionManager : MonoBehaviour
{
    // Constants

    // Local classes / structs

    // Enums

    // Variables

    [Header("References")]
    [SerializeField] Camera _gameCamera;
    [SerializeField] Tilemap _floorTilemap;
    [SerializeField] GridOccupancy _gridOccupancy;
    [SerializeField] PlacementManager _placementManager;
    [SerializeField] StationCraftingPanelUI _craftingPanel;
    [SerializeField] StationQuizPanelUI _quizPanel;

    [Header("Long Press Settings")]
    [SerializeField] float _longPressDuration = 0.5f;
    [SerializeField] float _maxMovementPixels = 40f;

    PlaceableObject _candidateObject;

    PlaceableObject _selectedObject;
    public PlaceableObject SelectedObject { get { return _selectedObject; } }

    Vector2 _pressStartPosition;
    float _pressStartTime;
    bool _wasPressed;

    readonly List<RaycastResult> _uiRaycastResults = new List<RaycastResult>();

    // Methods

    void Update()
    {
        Pointer pointer = Pointer.current;

        if (pointer == null)
        {
            ResetPress();
            return;
        }

        bool isPressed = pointer.press.isPressed;
        Vector2 screenPosition = pointer.position.ReadValue();

        if (pointer is Touchscreen touchscreen)
        {
            isPressed = touchscreen.primaryTouch.press.isPressed;
            screenPosition = touchscreen.primaryTouch.position.ReadValue();
        }

        bool isCraftingPanelOpen = _craftingPanel != null && _craftingPanel.IsOpen;

        bool isQuizPanelOpen = _quizPanel != null && _quizPanel.IsOpen;

        if (isCraftingPanelOpen || isQuizPanelOpen)
        {
            ResetPress();
            return;
        }

        if (!isPressed)
        {
            if (_wasPressed && _candidateObject != null)
            {
                float tapDuration = Time.unscaledTime - _pressStartTime;

                float tapMovement = Vector2.Distance(_pressStartPosition, screenPosition);

                if (tapDuration < _longPressDuration && tapMovement <= _maxMovementPixels)
                {
                    bool opened = false;

                    if (_craftingPanel != null)
                        opened = _craftingPanel.OpenFor(_candidateObject);

                    if (!opened && _quizPanel != null)
                        _quizPanel.OpenFor(_candidateObject);
                }
            }

            ResetPress();
            return;
        }

        if (_placementManager.IsPlacing)
        {
            _candidateObject = null;
            _wasPressed = true;
            return;
        }

        if (!_wasPressed)
        {
            _wasPressed = true;
            BeginPress(screenPosition);
            return;
        }

        if (_candidateObject == null)
            return;

        float movement = Vector2.Distance(_pressStartPosition, screenPosition);

        if (movement > _maxMovementPixels)
        {
            _candidateObject = null;
            return;
        }

        float holdTime = Time.unscaledTime - _pressStartTime;

        if (holdTime >= _longPressDuration)
        {
            _selectedObject = _candidateObject;
            _candidateObject = null;

            _placementManager.BeginEdit(_selectedObject);
        }
    }

    void BeginPress(Vector2 screenPosition)
    {
        _candidateObject = null;

        if (IsPointerOverUI(screenPosition))
            return;

        if (!_gameCamera.pixelRect.Contains(screenPosition))
            return;

        float distanceToFloor = _floorTilemap.transform.position.z - _gameCamera.transform.position.z;

        Vector3 worldPosition = _gameCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, distanceToFloor));

        Vector3Int tileCell = _floorTilemap.WorldToCell(worldPosition);

        if (!_floorTilemap.HasTile(tileCell))
            return;

        Vector2Int cell = new Vector2Int(tileCell.x, tileCell.y);

        PlaceableObject placedObject = _gridOccupancy.GetObjectAtCell(cell);

        if (placedObject == null)
            return;

        _candidateObject = placedObject;
        _pressStartPosition = screenPosition;
        _pressStartTime = Time.unscaledTime;
    }

    bool IsPointerOverUI(Vector2 screenPosition)
    {
        if (EventSystem.current == null)
            return false;

        PointerEventData pointerData = new PointerEventData(EventSystem.current);

        pointerData.position = screenPosition;

        _uiRaycastResults.Clear();

        EventSystem.current.RaycastAll(pointerData, _uiRaycastResults);

        return _uiRaycastResults.Count > 0;
    }

    void ResetPress()
    {
        _wasPressed = false;
        _candidateObject = null;
    }

    void OnDisable()
    {
        ResetPress();
    }
}