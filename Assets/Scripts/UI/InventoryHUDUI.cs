using TMPro;
using UnityEngine;

public class InventoryHUDUI : MonoBehaviour
{
    // Constants

    // Local classes / structs

    // Enums

    // Variables

    [Header("References")]
    [SerializeField] InventoryManager _inventoryManager;
    [SerializeField] TMP_Text _inventoryText;

    [Header("Items")]
    [SerializeField] ItemData _wood;
    [SerializeField] ItemData _stone;
    [SerializeField] ItemData _tool;

    // Methods

    void OnEnable()
    {
        if (_inventoryManager != null)
            _inventoryManager.InventoryChanged += RefreshInventory;
    }

    void Start()
    {
        RefreshInventory();
    }

    void OnDisable()
    {
        if (_inventoryManager != null)
            _inventoryManager.InventoryChanged -= RefreshInventory;
    }

    void RefreshInventory()
    {
        int wood = _inventoryManager.GetAmount(_wood);
        int stone = _inventoryManager.GetAmount(_stone);
        int tool = _inventoryManager.GetAmount(_tool);

        _inventoryText.text = $"Wood: {wood}    |    Stone: {stone}    |    Tool: {tool}";
    }
}