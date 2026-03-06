using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 팝업 UI를 스택(LIFO)으로 관리하는 싱글톤.
/// ESC 키 입력 시 가장 위의 팝업을 닫고, 열려 있는 팝업이 없으면
/// OnCancelNoUI 이벤트를 발행하여 설정 UI 등을 여는 데 활용합니다.
/// 
/// <para><b>설계 의도</b>: 여러 팝업이 중첩될 때 가장 최근에 열린
/// 팝업부터 순서대로 닫히도록 스택 구조를 사용합니다.</para>
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Global Input Action")]
    [SerializeField] private InputActionReference escapeAction;

    private readonly Stack<IPopupUI> _popupStack = new();

    /// <summary>열린 팝업이 없을 때 ESC 키가 눌리면 발행됩니다.</summary>
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

    /// <summary>팝업을 스택에 추가합니다.</summary>
    public void PushUI(IPopupUI ui)
    {
        if (!_popupStack.Contains(ui))
            _popupStack.Push(ui);
    }

    /// <summary>팝업을 스택에서 제거합니다. 최상위 팝업만 제거 가능합니다.</summary>
    public void PopUI(IPopupUI ui)
    {
        if (_popupStack.Count > 0 && _popupStack.Peek() == ui)
            _popupStack.Pop();
    }
}
