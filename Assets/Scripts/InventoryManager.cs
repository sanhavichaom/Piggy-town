using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    // Constants

    const string SAVE_KEY = "PiggyTown.InventoryCrafting.v2";
    const string LEGACY_INVENTORY_PREFIX = "PiggyTown.Inventory.";
    const string LEGACY_CRAFT_END_KEY = "PiggyTown.Crafting.EndTime";
    const string LEGACY_CRAFT_RECIPE_KEY = "PiggyTown.Crafting.Recipe";

    // Local classes / structs

    [Serializable]
    public class StartingItem
    {
        [SerializeField] ItemData _item;
        public ItemData Item { get { return _item; } }
        [SerializeField] int _amount;
        public int Amount { get { return _amount; } }
    }

    [Serializable]
    class SavedItem
    {
        public string id;
        public int amount;
    }

    [Serializable]
    class InventorySaveData
    {
        public int version = 2;
        public List<SavedItem> items = new List<SavedItem>();
        public string activeRecipeId = "";
        public string craftEndUtcTicks = "";
    }

    // Variables

    [Header("Starting Items")]
    [SerializeField] StartingItem[] _startingItems;

    readonly Dictionary<ItemData, int> _items = new Dictionary<ItemData, int>();

    string _activeRecipeId = "";
    public string ActiveRecipeId { get { return _activeRecipeId; } }
    string _craftEndUtcTicks = "";
    bool _isReady;

    public event Action InventoryChanged;

    public long CraftEndUtcTicks
    {
        get
        {
            long ticks;

            if (long.TryParse(_craftEndUtcTicks, NumberStyles.Integer, CultureInfo.InvariantCulture, out ticks))
                return ticks;

            return 0;
        }
    }

    // Methods

    void Awake()
    {
        if (_startingItems == null || _startingItems.Length == 0)
            return;

        HashSet<string> registeredIds = new HashSet<string>();

        foreach (StartingItem startingItem in _startingItems)
        {
            if (startingItem == null || startingItem.Item == null ||
                string.IsNullOrEmpty(startingItem.Item.Id) || !registeredIds.Add(startingItem.Item.Id))
                return;

            _items.Add(startingItem.Item, startingItem.Amount);
        }

        if (PlayerPrefs.HasKey(SAVE_KEY))
        {
            if (!LoadProgress())
            {
                Debug.LogError("Inventory save is invalid. Data was not overwritten.", this);
                return;
            }
        }
        else
            LoadLegacyProgress();

        _isReady = true;

        if (!PlayerPrefs.HasKey(SAVE_KEY))
            SaveProgress();
    }

    bool LoadProgress()
    {
        try
        {
            string json = PlayerPrefs.GetString(SAVE_KEY);

            InventorySaveData saveData = JsonUtility.FromJson<InventorySaveData>(json);

            if (saveData == null || saveData.version != 2 || saveData.items == null)
                return false;

            HashSet<string> savedIds = new HashSet<string>();

            foreach (SavedItem savedItem in saveData.items)
            {
                if (savedItem == null || string.IsNullOrEmpty(savedItem.id) || savedItem.amount < 0 || !savedIds.Add(savedItem.id))
                    return false;
            }

            foreach (StartingItem startingItem in _startingItems)
            {
                ItemData item = startingItem.Item;

                foreach (SavedItem savedItem in saveData.items)
                {
                    if (savedItem.id == item.Id)
                    {
                        _items[item] = savedItem.amount;
                        break;
                    }
                }
            }

            _activeRecipeId = saveData.activeRecipeId ?? "";
            _craftEndUtcTicks = saveData.craftEndUtcTicks ?? "";

            if (!string.IsNullOrEmpty(_activeRecipeId) && CraftEndUtcTicks <= 0)
                return false;

            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError("Inventory load failed: " + exception.Message);
            return false;
        }
    }

    void LoadLegacyProgress()
    {
        foreach (StartingItem startingItem in _startingItems)
        {
            ItemData item = startingItem.Item;

            _items[item] = PlayerPrefs.GetInt(LEGACY_INVENTORY_PREFIX + item.Id, startingItem.Amount);
        }

        _activeRecipeId = PlayerPrefs.GetString(LEGACY_CRAFT_RECIPE_KEY, "");

        _craftEndUtcTicks = PlayerPrefs.GetString(LEGACY_CRAFT_END_KEY, "");

        if (string.IsNullOrEmpty(_activeRecipeId) ||
            CraftEndUtcTicks <= 0)
        {
            _activeRecipeId = "";
            _craftEndUtcTicks = "";
        }
    }

    void SaveProgress()
    {
        if (!_isReady)
            return;

        InventorySaveData saveData = new InventorySaveData();

        foreach (KeyValuePair<ItemData, int> entry in _items)
        {
            SavedItem savedItem = new SavedItem();

            savedItem.id = entry.Key.Id;
            savedItem.amount = entry.Value;

            saveData.items.Add(savedItem);
        }

        saveData.activeRecipeId = _activeRecipeId;
        saveData.craftEndUtcTicks = _craftEndUtcTicks;

        try
        {
            string json = JsonUtility.ToJson(saveData);

            PlayerPrefs.SetString(SAVE_KEY, json);
            PlayerPrefs.Save();

            InventoryChanged?.Invoke();
        }
        catch (Exception exception)
        {
            Debug.LogError("Inventory save failed: " + exception.Message);
        }
    }

    public int GetAmount(ItemData item)
    {
        if (item == null)
            return 0;

        int amount;

        if (_items.TryGetValue(item, out amount))
            return amount;

        return 0;
    }

    public void AddItem(ItemData item, int amount)
    {
        if (!_isReady || item == null || amount <= 0)
            return;

        _items[item] = GetAmount(item) + amount;

        SaveProgress();
    }

    public bool HasEnough(ItemData item, int amount)
    {
        if (!_isReady || item == null || amount <= 0)
            return false;

        return GetAmount(item) >= amount;
    }

    public bool TryRemoveItem(ItemData item, int amount)
    {
        if (!HasEnough(item, amount))
            return false;

        _items[item] -= amount;

        SaveProgress();

        return true;
    }

    public bool HasIngredients(CraftingRecipeData recipe)
    {
        if (!_isReady || recipe == null ||
            recipe.InputItem1 == null || recipe.InputItem2 == null ||
            recipe.OutputItem == null || recipe.InputAmount1 <= 0 ||
            recipe.InputAmount2 <= 0 || recipe.OutputAmount <= 0)
        {
            return false;
        }

        if (recipe.InputItem1 == recipe.InputItem2)
        {
            return GetAmount(recipe.InputItem1) >=
                recipe.InputAmount1 + recipe.InputAmount2;
        }

        return HasEnough(recipe.InputItem1, recipe.InputAmount1) &&
               HasEnough(recipe.InputItem2, recipe.InputAmount2);
    }

    public bool TryConsumeIngredients(CraftingRecipeData recipe)
    {
        if (!HasIngredients(recipe))
            return false;

        _items[recipe.InputItem1] -= recipe.InputAmount1;
        _items[recipe.InputItem2] -= recipe.InputAmount2;

        SaveProgress();

        return true;
    }

    public bool IsCraftActive(string recipeId)
    {
        return _isReady && !string.IsNullOrEmpty(recipeId) && _activeRecipeId == recipeId;
    }

    public bool TryBeginCraft(CraftingRecipeData recipe)
    {
        if (!_isReady || recipe == null ||
            string.IsNullOrEmpty(recipe.Id) || recipe.DurationSeconds <= 0 ||
            !string.IsNullOrEmpty(_activeRecipeId) || !HasIngredients(recipe))
        {
            return false;
        }

        _items[recipe.InputItem1] -= recipe.InputAmount1;
        _items[recipe.InputItem2] -= recipe.InputAmount2;

        DateTime endTime = DateTime.UtcNow.AddSeconds(recipe.DurationSeconds);

        _activeRecipeId = recipe.Id;
        _craftEndUtcTicks = endTime.Ticks.ToString(CultureInfo.InvariantCulture);

        SaveProgress();

        return true;
    }

    public bool TryCompleteCraft(CraftingRecipeData recipe)
    {
        if (!_isReady || recipe == null ||
            recipe.OutputItem == null || recipe.OutputAmount <= 0 || 
            !IsCraftActive(recipe.Id))
        {
            return false;
        }

        if (DateTime.UtcNow.Ticks < CraftEndUtcTicks)
            return false;

        _items[recipe.OutputItem] =GetAmount(recipe.OutputItem) + recipe.OutputAmount;

        _activeRecipeId = "";
        _craftEndUtcTicks = "";

        SaveProgress();

        return true;
    }
}