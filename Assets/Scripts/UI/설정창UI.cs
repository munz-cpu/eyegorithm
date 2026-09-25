using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class 설정창UI : MonoBehaviour
{
    [Header("연결")]
    [SerializeField] private 설정관리자 설정;
    [SerializeField] private Button 열기버튼;
    [SerializeField] private Button 닫기버튼;
    [SerializeField] private GameObject 설정패널;

    [Header("입력")]
    [SerializeField] private TMP_Dropdown 큐드롭다운;
    [SerializeField] private GameObject 라운드로빈시간행;
    [SerializeField] private TMP_InputField 라운드로빈시간입력;
    [SerializeField] private Slider 배경음슬라이더;
    [SerializeField] private Slider 학생효과음슬라이더;
    [SerializeField] private Slider 틱속도슬라이더;
    [SerializeField] private Slider 최대인원슬라이더;
    [SerializeField] private Toggle 남은시간토글;

    [Header("현재 값")]
    [SerializeField] private TMP_Text 틱속도값;
    [SerializeField] private TMP_Text 최대인원값;

    private void Awake()
    {
        값표시갱신();
        if (설정패널 != null)
            설정패널.SetActive(false);
    }

    private void OnEnable()
    {
        if (열기버튼 != null)
            열기버튼.onClick.AddListener(열기);
        if (닫기버튼 != null)
            닫기버튼.onClick.AddListener(닫기);

        if (설정 == null)
            return;

        if (큐드롭다운 != null)
            큐드롭다운.onValueChanged.AddListener(설정.큐종류설정);
        if (라운드로빈시간입력 != null)
            라운드로빈시간입력.onEndEdit.AddListener(라운드로빈시간확정);
        if (배경음슬라이더 != null)
            배경음슬라이더.onValueChanged.AddListener(설정.배경음볼륨설정);
        if (학생효과음슬라이더 != null)
            학생효과음슬라이더.onValueChanged.AddListener(설정.학생효과음볼륨설정);
        if (틱속도슬라이더 != null)
            틱속도슬라이더.onValueChanged.AddListener(설정.틱속도설정);
        if (최대인원슬라이더 != null)
            최대인원슬라이더.onValueChanged.AddListener(설정.최대인원설정);
        if (남은시간토글 != null)
            남은시간토글.onValueChanged.AddListener(설정.남은시간표시설정);
        if (설정 != null)
            설정.설정변경 += 값표시갱신;
    }

    private void OnDisable()
    {
        if (열기버튼 != null)
            열기버튼.onClick.RemoveListener(열기);
        if (닫기버튼 != null)
            닫기버튼.onClick.RemoveListener(닫기);

        if (설정 == null)
            return;

        if (큐드롭다운 != null)
            큐드롭다운.onValueChanged.RemoveListener(설정.큐종류설정);
        if (라운드로빈시간입력 != null)
            라운드로빈시간입력.onEndEdit.RemoveListener(라운드로빈시간확정);
        if (배경음슬라이더 != null)
            배경음슬라이더.onValueChanged.RemoveListener(설정.배경음볼륨설정);
        if (학생효과음슬라이더 != null)
            학생효과음슬라이더.onValueChanged.RemoveListener(설정.학생효과음볼륨설정);
        if (틱속도슬라이더 != null)
            틱속도슬라이더.onValueChanged.RemoveListener(설정.틱속도설정);
        if (최대인원슬라이더 != null)
            최대인원슬라이더.onValueChanged.RemoveListener(설정.최대인원설정);
        if (남은시간토글 != null)
            남은시간토글.onValueChanged.RemoveListener(설정.남은시간표시설정);
        if (설정 != null)
            설정.설정변경 -= 값표시갱신;
    }

    public void 열기()
    {
        if (설정패널 != null)
            설정패널.SetActive(true);
    }

    public void 닫기()
    {
        if (설정패널 != null)
            설정패널.SetActive(false);
    }

    private void 라운드로빈시간확정(string 값)
    {
        설정.라운드로빈퀀텀설정(값);
        라운드로빈시간입력.SetTextWithoutNotify(설정.퀀텀.ToString("0.##", System.Globalization.CultureInfo.CurrentCulture));
    }

    private void 값표시갱신()
    {
        if (설정 == null)
            return;

        if (큐드롭다운 != null)
            큐드롭다운.SetValueWithoutNotify((int)설정.큐종류값);
        if (라운드로빈시간행 != null)
            라운드로빈시간행.SetActive(설정.큐종류값 == 큐_타입.라운드로빈);
        if (라운드로빈시간입력 != null && !라운드로빈시간입력.isFocused)
            라운드로빈시간입력.SetTextWithoutNotify(설정.퀀텀.ToString("0.##", System.Globalization.CultureInfo.CurrentCulture));
        if (배경음슬라이더 != null)
            배경음슬라이더.SetValueWithoutNotify(설정.배경음크기);
        if (학생효과음슬라이더 != null)
            학생효과음슬라이더.SetValueWithoutNotify(설정.학생효과음크기);
        if (틱속도슬라이더 != null)
            틱속도슬라이더.SetValueWithoutNotify(설정.틱배율);
        if (최대인원슬라이더 != null)
            최대인원슬라이더.SetValueWithoutNotify(설정.최대학생수);
        if (남은시간토글 != null)
            남은시간토글.SetIsOnWithoutNotify(설정.남은시간을표시함);
        if (틱속도값 != null)
            틱속도값.text = $"{설정.틱배율:0.00}x";
        if (최대인원값 != null)
            최대인원값.text = $"{설정.최대학생수}명";
    }
}
