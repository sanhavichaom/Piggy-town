using System;
using UnityEngine;

public class CraftingManager : MonoBehaviour
{
    // Constants

    // Local classes / structs

    // Enums

    // Variables

    [Header("References")]
    [SerializeField] InventoryManager _inventoryManager;
    [SerializeField] CraftingRecipeData _recipe;
    public CraftingRecipeData Recipe { get { return _recipe; } }

    float _nextCheckTime;


    public bool IsCrafting
    {
        get
        {
            if (_inventoryManager == null || _recipe == null)
                return false;

            return _inventoryManager.IsCraftActive(_recipe.Id);
        }
    }

    public int RemainingSeconds
    {
        get
        {
            if (!IsCrafting)
                return 0;

            long remainingTicks = _inventoryManager.CraftEndUtcTicks - DateTime.UtcNow.Ticks;

            if (remainingTicks <= 0)
                return 0;

            return (int)Math.Ceiling(remainingTicks / (double)TimeSpan.TicksPerSecond);
        }
    }

    // Methods

    void Start()
    {
        if (_inventoryManager == null || _recipe == null || string.IsNullOrEmpty(_recipe.Id))
        {
            enabled = false;
            return;
        }

        if (!string.IsNullOrEmpty(_inventoryManager.ActiveRecipeId) &&
            !IsCrafting)
        {
            Debug.LogError("Saved crafting recipe ID does not match.", this);

            enabled = false;
            return;
        }

        CheckCraftCompletion();
    }

    void Update()
    {
        if (Time.unscaledTime < _nextCheckTime)
            return;

        _nextCheckTime = Time.unscaledTime + 0.5f;

        CheckCraftCompletion();
    }

    public bool TryStartCraft()
    {
        if (!enabled || _inventoryManager == null || _recipe == null)
            return false;

        return _inventoryManager.TryBeginCraft(_recipe);
    }

    void CheckCraftCompletion()
    {
        if (!IsCrafting)
            return;

        if (RemainingSeconds > 0)
            return;

        _inventoryManager.TryCompleteCraft(_recipe);
    }

#if UNITY_EDITOR
    [ContextMenu("Start Craft (Test)")]
    void TestStartCraft()
    {
        Debug.Log("Start Craft: " + TryStartCraft());
    }

    [ContextMenu("Show Craft Status (Test)")]
    void TestShowStatus()
    {
        Debug.Log("Crafting: " + IsCrafting + " | Remaining: " + RemainingSeconds);
    }
#endif
}