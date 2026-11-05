using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class ObjectPlacementControlsUI : MonoBehaviour
{
    // Constants

    // Local classes / structs

    // Enums

    // Variables

    [Header("References")]
    [SerializeField] Canvas _canvas;
    [SerializeField] PlacementManager _placementManager;
    [SerializeField] Button _confirmButton;
    [SerializeField] Button _cancelButton;
    [SerializeField] Button _flipButton;

    RectTransform _canvasRect;
    RectTransform _controlsRect;
    CanvasGroup _canvasGroup;

    bool _isVisible;

    // Methods

    void Awake()
    {
        _canvasRect = _canvas != null
            ? _canvas.GetComponent<RectTransform>()
            : null;

        _controlsRect = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();

        Hide();

        if (_canvasRect == null || _placementManager == null ||
            _confirmButton == null || _cancelButton == null ||
            _flipButton == null)
        {
            enabled = false;
            return;
        }

        _confirmButton.onClick.AddListener(_placementManager.ConfirmPlacement);
        _cancelButton.onClick.AddListener(_placementManager.CancelPlacement);
        _flipButton.onClick.AddListener(_placementManager.FlipPlacement);
    }

    public void ShowAtWorldPosition(Vector3 worldPosition, Camera gameCamera)
    {
        if (!enabled || gameCamera == null || _canvasRect == null)
            return;

        Vector3 screenPosition = gameCamera.WorldToScreenPoint(worldPosition);

        if (screenPosition.z <= 0f)
        {
            Hide();
            return;
        }

        Vector2 localPosition;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvasRect,
            new Vector2(screenPosition.x, screenPosition.y),
            null,
            out localPosition))
        {
            Hide();
            return;
        }

        float halfWidth = _controlsRect.rect.width * 0.5f;
        float halfHeight = _controlsRect.rect.height * 0.5f;

        localPosition.x = Mathf.Clamp(
            localPosition.x,
            _canvasRect.rect.xMin + halfWidth,
            _canvasRect.rect.xMax - halfWidth
        );

        localPosition.y = Mathf.Clamp(
            localPosition.y,
            _canvasRect.rect.yMin + halfHeight,
            _canvasRect.rect.yMax - halfHeight
        );

        _controlsRect.anchoredPosition = localPosition;

        _canvasGroup.alpha = 1f;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;

        _isVisible = true;
    }

    public bool IsPointerOverButtons(Vector2 screenPosition)
    {
        if (!_isVisible)
            return false;

        RectTransform confirmRect = (RectTransform)_confirmButton.transform;
        RectTransform cancelRect = (RectTransform)_cancelButton.transform;
        RectTransform flipRect = (RectTransform)_flipButton.transform;

        if (RectTransformUtility.RectangleContainsScreenPoint(confirmRect, screenPosition))
            return true;

        if (RectTransformUtility.RectangleContainsScreenPoint(cancelRect, screenPosition))
            return true;

        if (RectTransformUtility.RectangleContainsScreenPoint(flipRect, screenPosition))
            return true;

        return false;
    }

    public void Hide()
    {
        _isVisible = false;

        if (_canvasGroup == null)
            return;

        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }

    void OnDestroy()
    {
        if (_placementManager == null)
            return;

        if (_confirmButton != null)
            _confirmButton.onClick.RemoveListener(_placementManager.ConfirmPlacement);

        if (_cancelButton != null)
            _cancelButton.onClick.RemoveListener(_placementManager.CancelPlacement);

        if (_flipButton != null)
            _flipButton.onClick.RemoveListener(_placementManager.FlipPlacement);
    }
}