using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class 학생소환UI : MonoBehaviour
{
    [SerializeField] private 학생소환기 소환기;
    [SerializeField] private Button 소환버튼;
    [SerializeField] private TMP_Dropdown 계급드롭다운;

    private void Update()
    {
        if (Time.timeScale == 0f)
            return;

        Keyboard 키보드 = Keyboard.current;
        if (소환기 == null || 키보드 == null ||
            (!키보드.enterKey.wasPressedThisFrame && !키보드.numpadEnterKey.wasPressedThisFrame))
            return;

        GameObject 선택 = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
        TMP_InputField 입력 = 선택 != null ? 선택.GetComponentInParent<TMP_InputField>() : null;
        if (입력 == null || !입력.isFocused)
            소환기.학생소환();
    }

    private void OnEnable()
    {
        if (소환버튼 != null)
            소환버튼.onClick.AddListener(소환기.학생소환);

        if (계급드롭다운 != null)
            계급드롭다운.onValueChanged.AddListener(소환기.소환계급설정);
    }

    private void OnDisable()
    {
        if (소환버튼 != null)
            소환버튼.onClick.RemoveListener(소환기.학생소환);

        if (계급드롭다운 != null)
            계급드롭다운.onValueChanged.RemoveListener(소환기.소환계급설정);
    }
}
