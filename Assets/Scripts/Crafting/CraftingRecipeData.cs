using UnityEngine;

[CreateAssetMenu(fileName = "CraftingRecipeData", menuName = "PiggyTown/Crafting Recipe")]
public class CraftingRecipeData : ScriptableObject
{
    // Constants

    // Local classes / structs

    // Enums

    // Variables

    [Header("Recipe Information")]
    [SerializeField] string _id;
    public string Id { get { return _id; } }
    [SerializeField] string _displayName;
    public string DisplayName { get { return _displayName; } }

    [Header("Input Items")]
    [SerializeField] ItemData _inputItem1;
    public ItemData InputItem1 { get { return _inputItem1; } }
    [SerializeField] int _inputAmount1 = 1;
    public int InputAmount1 { get { return _inputAmount1; } }

    [SerializeField] ItemData _inputItem2;
    public ItemData InputItem2 { get { return _inputItem2; } }
    [SerializeField] int _inputAmount2 = 1;
    public int InputAmount2 { get { return _inputAmount2; } }

    [Header("Output Item")]
    [SerializeField] ItemData _outputItem;
    public ItemData OutputItem { get { return _outputItem; } }
    [SerializeField] int _outputAmount = 1;
    public int OutputAmount { get { return _outputAmount; } }

    [Header("Crafting")]
    [SerializeField] int _durationSeconds = 180;
    public int DurationSeconds { get { return _durationSeconds; } }

    // Methods
}