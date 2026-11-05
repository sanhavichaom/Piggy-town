using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlaceableItemButtonUI : MonoBehaviour
{
    // Constants

    // Local classes / structs

    // Enums

    // Variables

    [Header("References")]
    [SerializeField] Button _button;
    [SerializeField] Image _iconImage;
    [SerializeField] TMP_Text _nameText;
    [SerializeField] TMP_Text _sizeText;

    PlaceableObjectData _data;
    
    Action<PlaceableObjectData> _onSelected;

    // Methods

    void Awake()
    {
        if (_button == null || _iconImage == null || _nameText == null || _sizeText == null)
        {
            enabled = false;
            return;
        }

        _button.onClick.AddListener(OnClicked);
    }

    public void Initialize(PlaceableObjectData data, Action<PlaceableObjectData> onSelected)
    {
        if (data == null || data.Sprite == null)
            return;

        _data = data;
        _onSelected = onSelected;

        _iconImage.sprite = data.Sprite;
        _iconImage.preserveAspect = true;
        _iconImage.raycastTarget = false;

        _nameText.text = data.DisplayName;

        Vector2Int footprint = data.GetFootprint(false);

        _sizeText.text = footprint.x + " x " + footprint.y;
    }

    void OnClicked()
    {
        if (_data == null || _onSelected == null)
            return;

        _onSelected(_data);
    }

    void OnDestroy()
    {
        if (_button != null)
            _button.onClick.RemoveListener(OnClicked);
    }
}
