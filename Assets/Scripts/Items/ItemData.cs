using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "PiggyTown/Item Data")]
public class ItemData : ScriptableObject
{
    // Constants

    // Local classes / structs

    // Enums

    // Variables

    [Header("Item Information")]
    [SerializeField] string _id;
    public string Id { get { return _id; } }

    [SerializeField] string _displayName;
    public string DisplayName { get { return _displayName; } }
    [SerializeField] Sprite _icon;
    public Sprite Icon { get { return _icon; } }

    // Methods
}
