using System;
using UnityEngine;

public enum 큐_타입
{
    일반,
    FCFS,
    라운드로빈,
    최소실행시간우선,
    우선순위
}

public enum 학생계급
{
    일학년 = 1,
    이학년 = 2,
    삼학년 = 3,
    학생회장 = 4,
    교장선생님 = 5
}

[DisallowMultipleComponent]
public class 설정관리자 : MonoBehaviour
{
    public const int 절대최대인원 = 15;

    [Header("스케줄링")]
    [SerializeField] private 큐_타입 큐종류 = 큐_타입.일반;
    [SerializeField, Min(0.1f)] private float 라운드로빈퀀텀 = 2f;
    [SerializeField, Range(0.25f, 3f)] private float 틱속도 = 1f;

    [Header("급식실")]
    [SerializeField] private bool 급식실열림;
    [SerializeField, Range(1, 절대최대인원)] private int 최대인원 = 절대최대인원;
    [SerializeField] private bool 남은시간표시 = true;

    [Header("소리")]
    [SerializeField, Range(0f, 1f)] private float 배경음볼륨 = 0.7f;
    [SerializeField, Range(0f, 1f)] private float 학생효과음볼륨 = 1f;
    [SerializeField] private AudioSource 배경음소스;

    public event Action 설정변경;
    public event Action<bool> 급식실상태변경;

    public 큐_타입 큐종류값 => 큐종류;
    public float 퀀텀 => 라운드로빈퀀텀;
    public float 틱배율 => 틱속도;
    public bool 열림 => 급식실열림;
    public int 최대학생수 => 최대인원;
    public bool 남은시간을표시함 => 남은시간표시;
    public float 배경음크기 => 배경음볼륨;
    public float 학생효과음크기 => 학생효과음볼륨;

    private void Awake()
    {
        적용();
    }

    // 급식실을 열거나 닫고 대기열 관리자에게 상태 변경을 알린다.
    public void 급식실상태전환()
    {
        급식실열림 = !급식실열림;
        급식실상태변경?.Invoke(급식실열림);
        설정변경?.Invoke();
    }

    // 드롭다운의 번호를 큐 종류로 변환한다.
    public void 큐종류설정(int 값)
    {
        큐종류 = (큐_타입)Mathf.Clamp(값, 0, Enum.GetValues(typeof(큐_타입)).Length - 1);
        설정변경?.Invoke();
    }

    public void 라운드로빈퀀텀설정(string 값)
    {
        if (!float.TryParse(값, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.CurrentCulture, out float 시간) &&
            !float.TryParse(값, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out 시간))
            return;

        라운드로빈퀀텀 = Mathf.Max(0.1f, 시간);
        설정변경?.Invoke();
    }

    public void 배경음볼륨설정(float 값)
    {
        배경음볼륨 = Mathf.Clamp01(값);
        적용();
    }

    public void 학생효과음볼륨설정(float 값)
    {
        학생효과음볼륨 = Mathf.Clamp01(값);
        설정변경?.Invoke();
    }

    public void 틱속도설정(float 값)
    {
        틱속도 = Mathf.Clamp(값, 0.25f, 3f);
        설정변경?.Invoke();
    }

    public void 최대인원설정(float 값)
    {
        최대인원 = Mathf.Clamp(Mathf.RoundToInt(값), 1, 절대최대인원);
        설정변경?.Invoke();
    }

    public void 남은시간표시설정(bool 값)
    {
        남은시간표시 = 값;
        설정변경?.Invoke();
    }

    private void 적용()
    {
        if (배경음소스 != null)
            배경음소스.volume = 배경음볼륨;

        설정변경?.Invoke();
    }

    private void OnValidate()
    {
        라운드로빈퀀텀 = Mathf.Max(0.1f, 라운드로빈퀀텀);
        최대인원 = Mathf.Clamp(최대인원, 1, 절대최대인원);
    }
}
