using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class 급식실상태UI : MonoBehaviour
{
    [SerializeField] private 설정관리자 설정;
    [SerializeField] private Button 열기닫기버튼;
    [SerializeField] private TMP_Text 버튼문구;

    private void OnEnable()
    {
        if (열기닫기버튼 != null)
            열기닫기버튼.onClick.AddListener(설정.급식실상태전환);

        if (설정 != null)
            설정.설정변경 += 문구갱신;

        문구갱신();
    }

    private void OnDisable()
    {
        if (열기닫기버튼 != null)
            열기닫기버튼.onClick.RemoveListener(설정.급식실상태전환);

        if (설정 != null)
            설정.설정변경 -= 문구갱신;
    }

    private void 문구갱신()
    {
        if (버튼문구 != null && 설정 != null)
            버튼문구.text = 설정.열림 ? "급식실 CLOSE" : "급식실 OPEN";
    }
}
