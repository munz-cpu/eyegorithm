using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class 학생소환UI : MonoBehaviour
{
    [SerializeField] private 학생소환기 소환기;
    [SerializeField] private Button 소환버튼;
    [SerializeField] private TMP_Dropdown 계급드롭다운;

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
