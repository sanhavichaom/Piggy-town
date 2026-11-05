using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    // Constants

    // Local classes / structs

    [System.Serializable]
    public class StartingItem
    {
        [SerializeField] ItemData _item;
        public ItemData Item { get { return _item; } }
        [SerializeField] int _amount;
        public int Amount { get { return _amount; } }


    }

    // Enums

    // Variables

    [Header("Starting Inventory")]
    [SerializeField] StartingItem[] _startingItems;

    Dictionary<ItemData, int> _items = new Dictionary<ItemData, int>();

    // Methods

    void Awake()
    {
        for (int i = 0; i < _startingItems.Length; i++)
        {
            StartingItem startingItem = _startingItems[i];

            if (startingItem == null || startingItem.Item == null)
                continue;

            ItemData item = startingItem.Item;

            string saveKey = "PiggyTown.Inventory." + item.Id;

            int amount = PlayerPrefs.GetInt(saveKey, startingItem.Amount);

            _items[item] = amount;
        }
    }

    public int GetAmount(ItemData item)
    {
        if (item == null)
            return 0;

        if (_items.TryGetValue(item, out int amount))
            return amount;

        return 0;
    }

    public void AddItem(ItemData item, int amount)
    {
        if (item == null || amount <= 0)
            return;

        int currentAmount = GetAmount(item);

        _items[item] = currentAmount + amount;

        SaveItem(item);
    }

    public bool HasEnough(ItemData item, int amount)
    {
        if (item == null || amount <= 0)
            return false;

        return GetAmount(item) >= amount;
    }

    public bool TryRemoveItem(ItemData item, int amount)
    {
        if (!HasEnough(item, amount))
            return false;

        _items[item] -= amount;

        SaveItem(item);

        return true;
    }

    void SaveItem(ItemData item)
    {
        string saveKey = "PiggyTown.Inventory." + item.Id;

        PlayerPrefs.SetInt(saveKey, GetAmount(item));
        PlayerPrefs.Save();
    }

    public bool HasIngredients(CraftingRecipeData recipe)
    {
        if (recipe == null)
            return false;

        if (recipe.InputItem1 == null || recipe.InputItem2 == null)
            return false;

        if (recipe.OutputItem == null)
            return false;

        if (recipe.InputAmount1 <= 0 || recipe.InputAmount2 <= 0)
            return false;

        if (recipe.InputItem1 == recipe.InputItem2)
        {
            int totalAmount =
                recipe.InputAmount1 + recipe.InputAmount2;

            return HasEnough(recipe.InputItem1, totalAmount);
        }

        bool hasFirstItem = HasEnough(recipe.InputItem1, recipe.InputAmount1);

        bool hasSecondItem = HasEnough(recipe.InputItem2, recipe.InputAmount2);

        return hasFirstItem && hasSecondItem;
    }

    public bool TryConsumeIngredients(CraftingRecipeData recipe)
    {
        if (!HasIngredients(recipe))
            return false;

        if (recipe.InputItem1 == recipe.InputItem2)
        {
            int totalAmount =
                recipe.InputAmount1 + recipe.InputAmount2;

            TryRemoveItem(recipe.InputItem1, totalAmount);

            return true;
        }

        TryRemoveItem(recipe.InputItem1, recipe.InputAmount1);
        TryRemoveItem(recipe.InputItem2, recipe.InputAmount2);

        return true;
    }
}