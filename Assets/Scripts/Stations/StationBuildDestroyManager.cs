using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StationBuildDestroyManager : MonoBehaviour
{
    // Constants

    // Local classes / structs

    // Enums

    // Variables

    [Header("References")]
    [SerializeField] PlaceableObjectData _station03Data;
    [SerializeField] GridOccupancy _gridOccupancy;
    [SerializeField] PlacementManager _placementManager;

    [Header("Station Panel")]
    [SerializeField] GameObject _stationPanel;
    [SerializeField] Button _buildButton;
    [SerializeField] Button _destroyButton;
    [SerializeField] Button _closeButton;

    [Header("Destroy Mode")]
    [SerializeField] GameObject _destroyModeBar;
    [SerializeField] TMP_Text _destroyModeText;
    [SerializeField] Button _cancelDestroyButton;

    bool _isDestroyMode;

    public bool IsOpen
    {
        get { return _stationPanel.activeSelf; }
    }

    public bool IsDestroyMode
    {
        get { return _isDestroyMode; }
    }

    // Methods

    void Awake()
    {
        if (_station03Data == null || _gridOccupancy == null ||
            _placementManager == null || _stationPanel == null ||
            _buildButton == null || _destroyButton == null ||
            _closeButton == null || _destroyModeBar == null ||
            _destroyModeText == null || _cancelDestroyButton == null)
        {
            Debug.LogError("Station 03 references are missing.", this);
            enabled = false;
            return;
        }

        _buildButton.onClick.AddListener(OpenBuildMode);
        _destroyButton.onClick.AddListener(BeginDestroyMode);
        _closeButton.onClick.AddListener(ClosePanel);
        _cancelDestroyButton.onClick.AddListener(CancelDestroyMode);

        _stationPanel.SetActive(false);
        _destroyModeBar.SetActive(false);
    }

    public bool OpenFor(PlaceableObject placedObject)
    {
        if (!enabled || placedObject == null)
            return false;

        if (placedObject.Data != _station03Data)
            return false;

        if (_placementManager.IsPlacing || _isDestroyMode)
            return false;

        _stationPanel.SetActive(true);

        return true;
    }

    void OpenBuildMode()
    {
        ClosePanel();
    }

    void BeginDestroyMode()
    {
        ClosePanel();

        _isDestroyMode = true;
        _destroyModeBar.SetActive(true);

        _destroyModeText.text =
            "Tap an object to destroy.";
    }

    public bool TryDestroyObject(PlaceableObject target)
    {
        if (!_isDestroyMode || target == null)
            return false;

        if (_placementManager.IsPlacing)
            return false;

        if (target.Data == _station03Data)
        {
            _destroyModeText.text = "Station 03 cannot be destroyed.";

            return false;
        }

        _gridOccupancy.Release(target);

        Destroy(target.gameObject);

        CancelDestroyMode();

        return true;
    }

    public void CancelDestroyMode()
    {
        _isDestroyMode = false;
        _destroyModeBar.SetActive(false);
    }

    void ClosePanel()
    {
        _stationPanel.SetActive(false);
    }

    void OnDestroy()
    {
        if (_buildButton != null)
            _buildButton.onClick.RemoveListener(OpenBuildMode);

        if (_destroyButton != null)
            _destroyButton.onClick.RemoveListener(BeginDestroyMode);

        if (_closeButton != null)
            _closeButton.onClick.RemoveListener(ClosePanel);

        if (_cancelDestroyButton != null)
            _cancelDestroyButton.onClick.RemoveListener(CancelDestroyMode);
    }
}
