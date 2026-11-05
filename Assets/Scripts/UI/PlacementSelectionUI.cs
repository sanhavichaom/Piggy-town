using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlacementSelectionUI : MonoBehaviour
{
    // Constants

    // Local classes / structs

    // Enums

    // Variables

    [Header("References")]
    [SerializeField] PlacementManager _placementManager;

    [Header("Tabs")]
    [SerializeField] Button _furnitureTabButton;
    [SerializeField] Button _stationTabButton;

    [Header("Content")]
    [SerializeField] Transform _furnitureContent;
    [SerializeField] Transform _stationContent;
    [SerializeField] PlaceableItemButtonUI _itemButtonPrefab;

    [Header("Scroll View")]
    [SerializeField] ScrollRect _itemsScrollRect;

    [Header("Items")]
    [SerializeField] PlaceableObjectData[] _furnitureItems;
    [SerializeField] PlaceableObjectData[] _stationItems;

    [Header("Selected Information")]
    [SerializeField] TMP_Text _selectedNameText;
    [SerializeField] TMP_Text _selectedFootprintText;

    // Methods

    void Start()
    {
        if (_placementManager == null || _furnitureTabButton == null ||
            _stationTabButton == null || _furnitureContent == null ||
            _stationContent == null || _itemButtonPrefab == null ||
            _selectedNameText == null || _selectedFootprintText == null ||
            _itemsScrollRect == null)
        {
            enabled = false;
            return;
        }

        _furnitureTabButton.onClick.AddListener(ShowFurniture);
        _stationTabButton.onClick.AddListener(ShowStations);

        CreateButtons(_furnitureItems,_furnitureContent,ZoneManager.PlaceableCategory.Furniture);

        CreateButtons(_stationItems, _stationContent, ZoneManager.PlaceableCategory.Station);

        ShowFurniture();

        _selectedNameText.text = "Selected: None";
        _selectedFootprintText.text = "Footprint: -";
    }

    PlaceableObjectData CreateButtons(PlaceableObjectData[] items, Transform content, ZoneManager.PlaceableCategory requiredCategory)
    {
        PlaceableObjectData firstItem = null;

        if (items == null)
            return null;

        for (int i = 0; i < items.Length; i++)
        {
            PlaceableObjectData data = items[i];

            if (data == null || data.Sprite == null)
                continue;

            if (data.Category != requiredCategory)
                continue;

            PlaceableItemButtonUI button = Instantiate(_itemButtonPrefab, content);

            button.gameObject.name = "Button_" + data.name;

            button.Initialize(data, SelectItem);

            if (firstItem == null)
                firstItem = data;
        }

        return firstItem;
    }

    void SelectItem(PlaceableObjectData data)
    {
        if (data == null)
            return;

        _placementManager.SelectObject(data);

        _selectedNameText.text = data.DisplayName;

        Vector2Int footprint = data.GetFootprint(false);

        _selectedFootprintText.text = "Footprint: " + footprint.x + " x " + footprint.y;
    }

    void ShowFurniture()
    {
        _furnitureContent.gameObject.SetActive(true);
        _stationContent.gameObject.SetActive(false);

        _itemsScrollRect.content = (RectTransform)_furnitureContent;
        _itemsScrollRect.horizontalNormalizedPosition = 0f;
    }

    void ShowStations()
    {
        _furnitureContent.gameObject.SetActive(false);
        _stationContent.gameObject.SetActive(true);

        _itemsScrollRect.content = (RectTransform)_stationContent;
        _itemsScrollRect.horizontalNormalizedPosition = 0f;
    }

    void OnDestroy()
    {
        if (_furnitureTabButton != null)
            _furnitureTabButton.onClick.RemoveListener(ShowFurniture);

        if (_stationTabButton != null)
            _stationTabButton.onClick.RemoveListener(ShowStations);
    }
}