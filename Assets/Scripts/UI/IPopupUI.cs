/// <summary>
/// ESC 키로 닫을 수 있는 팝업 UI가 구현하는 인터페이스.
/// UIManager가 스택으로 관리하여 LIFO 순서로 닫기를 처리합니다.
/// </summary>
public interface IPopupUI
{
    /// <summary>팝업을 닫습니다.</summary>
    void Close();
}
