using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StationCraftingPanelUI : MonoBehaviour
{
    // Constants

    // Local classes / structs

    // Enums

    // Variables

    [Header("References")]
    [SerializeField] PlaceableObjectData _stationData;
    [SerializeField] CraftingManager _craftingManager;
    [SerializeField] InventoryManager _inventoryManager;

    [Header("UI")]
    [SerializeField] TMP_Text _recipeText;
    [SerializeField] TMP_Text _inventoryText;
    [SerializeField] TMP_Text _statusText;
    [SerializeField] TMP_Text _timerText;

    [SerializeField] Button _startButton;
    [SerializeField] Button _closeButton;

    float _nextRefreshTime;

    public bool IsOpen
    {
        get { return gameObject.activeSelf; }
    }

    // Methods

    void Awake()
    {
        if (_stationData == null ||
            _craftingManager == null ||
            _inventoryManager == null ||
            _recipeText == null ||
            _inventoryText == null ||
            _statusText == null ||
            _timerText == null ||
            _startButton == null ||
            _closeButton == null)
        {
            Debug.LogError("Station Crafting Panel references are missing.", this);
            enabled = false;
            gameObject.SetActive(false);
            return;
        }

        _startButton.onClick.AddListener(StartCraft);
        _closeButton.onClick.AddListener(Close);

        gameObject.SetActive(false);
    }

    void Update()
    {
        if (Time.unscaledTime < _nextRefreshTime)
            return;

        _nextRefreshTime = Time.unscaledTime + 0.5f;

        RefreshUI();
    }

    public bool OpenFor(PlaceableObject placedObject)
    {
        if (!enabled || placedObject == null)
            return false;

        if (placedObject.Data != _stationData)
            return false;

        gameObject.SetActive(true);

        _nextRefreshTime = 0f;
        RefreshUI();

        return true;
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    void StartCraft()
    {
        _craftingManager.TryStartCraft();

        RefreshUI();
    }

    void RefreshUI()
    {
        CraftingRecipeData recipe = _craftingManager.Recipe;

        if (recipe == null ||
            recipe.InputItem1 == null ||
            recipe.InputItem2 == null ||
            recipe.OutputItem == null)
        {
            _statusText.text = "Invalid Recipe";
            _startButton.interactable = false;
            return;
        }

        _recipeText.text = recipe.InputItem1.DisplayName + " x" + recipe.InputAmount1 + " + " + recipe.InputItem2.DisplayName + " x" + recipe.InputAmount2 + " = " + recipe.OutputItem.DisplayName + " x" + recipe.OutputAmount;

        _inventoryText.text = recipe.InputItem1.DisplayName + ": " + _inventoryManager.GetAmount(recipe.InputItem1) + "\n" + recipe.InputItem2.DisplayName + ": " + _inventoryManager.GetAmount(recipe.InputItem2) + "\n" + recipe.OutputItem.DisplayName + ": " + _inventoryManager.GetAmount(recipe.OutputItem);

        int seconds;

        if (_craftingManager.IsCrafting)
        {
            _statusText.text = "Crafting...";
            _startButton.interactable = false;

            seconds = _craftingManager.RemainingSeconds;
        }
        else
        {
            bool hasIngredients = _inventoryManager.HasIngredients(recipe);

            if (hasIngredients)
                _statusText.text = "Ready to Craft";
            else
                _statusText.text = "Not Enough Materials";

            _startButton.interactable = hasIngredients;

            seconds = recipe.DurationSeconds;
        }

        int minutes = seconds / 60;
        int remainingSeconds = seconds % 60;

        _timerText.text = minutes.ToString("00") + ":" + remainingSeconds.ToString("00");
    }

    void OnDestroy()
    {
        if (_startButton != null)
            _startButton.onClick.RemoveListener(StartCraft);

        if (_closeButton != null)
            _closeButton.onClick.RemoveListener(Close);
    }
}