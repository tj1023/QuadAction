using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Global Input Action")]
    [SerializeField] private InputActionReference escapeAction;

    private Stack<IPopupUI> _popupStack = new Stack<IPopupUI>();

    public event System.Action OnCancelNoUI;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        if (escapeAction != null)
        {
            escapeAction.action.Enable();
            escapeAction.action.performed += OnEscapePerformed;
        }
    }

    private void OnDisable()
    {
        if (escapeAction != null)
        {
            escapeAction.action.performed -= OnEscapePerformed;
            escapeAction.action.Disable();
        }
    }

    private void OnEscapePerformed(InputAction.CallbackContext context)
    {
        if (_popupStack.Count > 0)
        {
            IPopupUI topUI = _popupStack.Peek();
            topUI.Close();
        }
        else
        {
            OnCancelNoUI?.Invoke();
        }
    }

    public void PushUI(IPopupUI ui)
    {
        if (!_popupStack.Contains(ui))
        {
            _popupStack.Push(ui);
        }
    }

    public void PopUI(IPopupUI ui)
    {
        if (_popupStack.Count > 0 && _popupStack.Peek() == ui)
        {
            _popupStack.Pop();
        }
    }
}
