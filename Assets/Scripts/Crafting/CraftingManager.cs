using System;
using UnityEngine;

public class CraftingManager : MonoBehaviour
{
    // Constants

    const string CRAFT_END_KEY = "PiggyTown.Crafting.EndTime";
    const string CRAFT_RECIPE_KEY = "PiggyTown.Crafting.Recipe";

    // Local classes / structs

    // Enums

    // Variables

    [Header("References")]
    [SerializeField] InventoryManager _inventoryManager;
    [SerializeField] CraftingRecipeData _recipe;
    public CraftingRecipeData Recipe { get { return _recipe; } }

    bool _isCrafting;
    public bool IsCrafting { get { return _isCrafting; } }

    DateTime _endTimeUtc;

    float _nextCheckTime;


    public int RemainingSeconds
    {
        get
        {
            if (!_isCrafting)
                return 0;

            double seconds = (_endTimeUtc - DateTime.UtcNow).TotalSeconds;

            return Mathf.Max(0, Mathf.CeilToInt((float)seconds));
        }
    }

    // Methods

    void Start()
    {
        if (_inventoryManager == null || _recipe == null)
        {
            Debug.LogError("Crafting Manager references are missing.", this);
            enabled = false;
            return;
        }

        LoadCraftingProgress();
        CheckCraftCompletion();
    }

    void Update()
    {
        if (!_isCrafting)
            return;

        if (Time.unscaledTime < _nextCheckTime)
            return;

        _nextCheckTime = Time.unscaledTime + 0.5f;

        CheckCraftCompletion();
    }

    public bool TryStartCraft()
    {
        if (!enabled || _inventoryManager == null || _recipe == null)
            return false;

        if (_isCrafting)
            return false;

        if (_recipe.DurationSeconds <= 0 || _recipe.OutputAmount <= 0)
            return false;

        if (string.IsNullOrEmpty(_recipe.Id))
            return false;

        if (!_inventoryManager.TryConsumeIngredients(_recipe))
            return false;

        _endTimeUtc = DateTime.UtcNow.AddSeconds(_recipe.DurationSeconds);

        _isCrafting = true;
        _nextCheckTime = 0f;

        PlayerPrefs.SetString(CRAFT_END_KEY, _endTimeUtc.Ticks.ToString());

        PlayerPrefs.SetString(CRAFT_RECIPE_KEY, _recipe.Id);

        PlayerPrefs.Save();

        Debug.Log("Crafting started: " + _recipe.DisplayName);

        return true;
    }

    void CheckCraftCompletion()
    {
        if (!_isCrafting)
            return;

        if (DateTime.UtcNow < _endTimeUtc)
            return;

        _isCrafting = false;

        _inventoryManager.AddItem(_recipe.OutputItem, _recipe.OutputAmount);

        PlayerPrefs.DeleteKey(CRAFT_END_KEY);
        PlayerPrefs.DeleteKey(CRAFT_RECIPE_KEY);
        PlayerPrefs.Save();

        Debug.Log("Crafting completed: " + _recipe.DisplayName);
    }

    void LoadCraftingProgress()
    {
        string savedTime = PlayerPrefs.GetString(CRAFT_END_KEY, "");

        string savedRecipeId = PlayerPrefs.GetString(CRAFT_RECIPE_KEY, "");

        if (savedRecipeId != _recipe.Id)
            return;

        if (!long.TryParse(savedTime, out long ticks))
            return;

        _endTimeUtc = new DateTime(ticks, DateTimeKind.Utc);
        _isCrafting = true;
    }

    [ContextMenu("Start Craft (Test)")]
    void TestStartCraft()
    {
        bool started = TryStartCraft();

        if (!started)
            Debug.Log("Cannot start crafting. Check inventory or active craft.");
    }

    [ContextMenu("Show Craft Status (Test)")]
    void TestShowStatus()
    {
        if (!_isCrafting)
        {
            Debug.Log("No active crafting.");
            return;
        }

        Debug.Log("Crafting remaining: " + RemainingSeconds + " seconds");
    }
}
