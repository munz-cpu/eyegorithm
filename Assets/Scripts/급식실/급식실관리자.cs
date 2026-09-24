using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class 급식실관리자 : MonoBehaviour
{
    [Header("연결")]
    [SerializeField] private 설정관리자 설정;
    [SerializeField] private Collider2D 학생이동범위;
    [SerializeField] private 경고메시지UI 경고UI;

    [Header("이동 위치")]
    [SerializeField] private Transform 줄시작점;
    [SerializeField] private Vector2 줄방향 = Vector2.right;
    [SerializeField, Min(0.1f)] private float 줄간격 = 1.15f;
    [SerializeField, Min(0f)] private float 학생사이여백 = 0.2f;
    [SerializeField] private Vector2 비켜서기방향 = Vector2.down;
    [SerializeField, Min(0.1f)] private float 비켜서기간격 = 1.25f;
    [SerializeField] private Transform 급식실입구;
    [SerializeField] private Transform 급식실입구안쪽;
    [SerializeField] private Transform 급식실출구;
    [SerializeField] private Transform 퇴장위치;
    [SerializeField] private Transform[] 자리위치 = new Transform[0];
    [SerializeField, Min(0.1f)] private float 이동제한시간 = 8f;

    private readonly List<학생정보> 전체학생 = new();
    private readonly 급식실대기열 대기열 = new();
    private bool[] 자리사용중;
    private long 다음도착순서;
    private bool 입장처리중;

    public int 현재인원 => 전체학생.Count;
    public bool 열림 => 설정 != null && 설정.열림;

    private void Awake()
    {
        자리사용중 = new bool[자리위치.Length];
    }

    private void OnEnable()
    {
        if (설정 == null)
            return;

        설정.급식실상태변경 += 급식실상태적용;
        설정.설정변경 += 설정적용;
        설정적용();
    }

    private void OnDisable()
    {
        if (설정 == null)
            return;

        설정.급식실상태변경 -= 급식실상태적용;
        설정.설정변경 -= 설정적용;
    }

    private void Update()
    {
        전체학생.RemoveAll(학생 => 학생 == null);

        if (열림 && !입장처리중 && 대기열.인원수 > 0 && 빈자리찾기() >= 0)
            StartCoroutine(다음학생입장());
    }

    // 소환된 학생을 전체 인원에 등록하고 현재 설정을 적용한다.
    public void 학생등록(학생정보 학생)
    {
        if (학생 == null || 전체학생.Contains(학생))
            return;

        전체학생.Add(학생);
        학생.도착순서 = 다음도착순서++;
        학생.이동기?.이동범위설정(학생이동범위);
        학생.이동기?.속도배율설정(설정 != null ? 설정.틱배율 : 1f);
        학생.남은시간표시설정(설정 == null || 설정.남은시간을표시함);
        학생.효과음볼륨설정(설정 != null ? 설정.학생효과음크기 : 1f);

        if (열림)
            줄에추가(학생);
    }

    public void 학생등록해제(학생정보 학생)
    {
        대기열.제거(학생);
        전체학생.Remove(학생);
        줄정렬();
    }

    public bool 특정계급존재(학생계급 계급)
    {
        return 전체학생.Exists(학생 => 학생 != null && 학생.계급값 == 계급);
    }

    public void 경고표시(string 내용)
    {
        if (경고UI != null)
            경고UI.표시(내용);
        else
            Debug.LogWarning(내용);
    }

    private void 급식실상태적용(bool 열렸음)
    {
        if (열렸음)
        {
            foreach (학생정보 학생 in 전체학생)
            {
                if (학생 != null && !학생.식사중 && !학생.줄서는중)
                    줄에추가(학생);
            }
        }
        else
        {
            foreach (학생정보 학생 in 대기열.목록)
            {
                if (학생 == null)
                    continue;

                학생.줄서는중 = false;
                학생.이동기?.배회시작();
            }

            대기열.비우기();
        }

        줄정렬();
    }

    private void 설정적용()
    {
        if (설정 == null)
            return;

        foreach (학생정보 학생 in 전체학생)
        {
            if (학생 == null)
                continue;

            학생.남은시간표시설정(설정.남은시간을표시함);
            학생.효과음볼륨설정(설정.학생효과음크기);
            학생.이동기?.속도배율설정(설정.틱배율);
        }
    }

    private void 줄에추가(학생정보 학생)
    {
        if (학생 == null || 학생.식사중 || 학생.줄서는중)
            return;

        학생.줄서는중 = true;
        대기열.추가(학생);
        줄정렬();
    }

    // 대기열의 현재 순서에 맞춰 모든 학생의 목표 위치를 다시 지정한다.
    private void 줄정렬()
    {
        if (줄시작점 == null)
            return;

        for (int i = 0; i < 대기열.인원수; i++)
        {
            학생정보 학생 = 대기열.목록[i];
            if (학생 != null)
                학생.이동기?.목표로이동(줄위치(i));
        }
    }

    private Vector2 줄위치(int 번호)
    {
        Vector2 방향 = 줄방향.sqrMagnitude < 0.001f ? Vector2.right : 줄방향.normalized;
        float 거리 = 0f;

        for (int i = 1; i <= 번호; i++)
        {
            학생정보 앞학생 = 대기열.목록[i - 1];
            학생정보 현재학생 = 대기열.목록[i];
            거리 += Mathf.Max(줄간격, 앞학생.충돌반경 + 현재학생.충돌반경 + 학생사이여백);
        }

        return (Vector2)줄시작점.position + 방향 * 거리;
    }

    // 선택 학생 앞의 인원을 잠시 비킨 뒤 학생을 빈 좌석까지 이동시킨다.
    private IEnumerator 다음학생입장()
    {
        입장처리중 = true;
        int 자리번호 = 빈자리찾기();

        if (자리번호 < 0 || 설정 == null)
        {
            입장처리중 = false;
            yield break;
        }

        학생정보 학생 = 대기열.다음학생선택(설정.큐종류값);
        if (학생 == null)
        {
            입장처리중 = false;
            yield break;
        }

        List<학생정보> 비킬학생들 = 앞사람목록(학생.도착순서);
        Vector2 비킴방향 = 비켜서기방향.sqrMagnitude < 0.001f ? Vector2.down : 비켜서기방향.normalized;

        for (int i = 0; i < 비킬학생들.Count; i++)
        {
            학생정보 앞학생 = 비킬학생들[i];
            int 줄번호 = 대기열.위치찾기(앞학생);
            Vector2 원래자리 = 줄위치(줄번호);
            float 비킬거리 = Mathf.Max(비켜서기간격, 학생.충돌반경 + 앞학생.충돌반경 + 학생사이여백);
            앞학생.이동기?.목표로이동(원래자리 + 비킴방향 * 비킬거리);
        }

        학생.줄서는중 = false;
        학생.식사중 = true;
        Vector3 자리 = 자리위치[자리번호].position;
        Vector3 입구 = 급식실입구 != null ? 급식실입구.position : 자리;
        Vector3 입구안쪽 = 급식실입구안쪽 != null
            ? 급식실입구안쪽.position
            : 입구 + Vector3.up * 0.6f;

        yield return 목표까지이동(학생, 입구);
        yield return 목표까지이동(학생, 입구안쪽);
        줄정렬();
        yield return 목표까지이동(학생, new Vector3(자리.x, 입구안쪽.y, 자리.z));
        yield return 목표까지이동(학생, 자리);

        자리사용중[자리번호] = true;
        입장처리중 = false;
        StartCoroutine(식사처리(학생, 자리번호));
    }

    // 큐 방식에 맞는 실행 시간만큼 식사시키고 완료 또는 재대기를 처리한다.
    private IEnumerator 식사처리(학생정보 학생, int 자리번호)
    {
        학생.이동기?.정지();
        float 이번실행시간 = 설정.큐종류값 == 큐_타입.라운드로빈
            ? Mathf.Min(설정.퀀텀, 학생.남은시간)
            : 학생.남은시간;
        float 실행됨 = 0f;

        while (학생 != null && 실행됨 < 이번실행시간 && 학생.남은시간 > 0f)
        {
            float 경과 = Time.deltaTime * 설정.틱배율;
            실행됨 += 경과;
            학생.식사진행(경과);
            yield return null;
        }

        if (학생 == null)
        {
            자리사용중[자리번호] = false;
            yield break;
        }

        Vector3 출구 = 급식실출구 != null
            ? 급식실출구.position
            : 퇴장위치 != null ? new Vector3(퇴장위치.position.x, 학생.transform.position.y, 0f) : 학생.transform.position + Vector3.right * 3f;
        Vector3 오른쪽통로 = new Vector3(출구.x, 학생.transform.position.y, 학생.transform.position.z);
        yield return 목표까지이동(학생, 오른쪽통로);
        yield return 목표까지이동(학생, 출구);

        if (학생 == null)
        {
            자리사용중[자리번호] = false;
            yield break;
        }

        if (학생.남은시간 <= 0f)
        {
            yield return 목표까지이동(학생, 퇴장위치 != null ? 퇴장위치.position : 학생.transform.position + Vector3.down * 3f);
            자리사용중[자리번호] = false;
            if (학생 == null)
                yield break;

            학생등록해제(학생);
            Destroy(학생.gameObject);
        }
        else
        {
            yield return 목표까지이동(학생, 출구 + Vector3.down * (학생.충돌반경 + 0.2f));
            자리사용중[자리번호] = false;
            학생.식사중 = false;

            if (열림)
            {
                학생.도착순서 = 다음도착순서++;
                줄에추가(학생);
            }
            else
            {
                학생.이동기?.배회시작();
            }
        }
    }

    private IEnumerator 목표까지이동(학생정보 학생, Vector3 목표)
    {
        if (학생 == null || 학생.이동기 == null)
            yield break;

        학생.이동기.목표로이동(목표);
        float 예상시간 = Vector2.Distance(학생.transform.position, 목표) / Mathf.Max(0.05f, 학생.이동기.현재이동속도);
        float 경고시간 = Mathf.Max(이동제한시간, 예상시간 * 2f + 1f);
        float 경과 = 0f;
        bool 지연경고표시 = false;

        while (학생 != null && !학생.이동기.목표에도착함)
        {
            경과 += Time.deltaTime;
            if (!지연경고표시 && 경과 > 경고시간)
            {
                Debug.LogWarning($"{학생.name} 이동이 지연되고 있습니다: {목표}", 학생);
                지연경고표시 = true;
            }

            yield return null;
        }
    }

    private List<학생정보> 앞사람목록(long 선택학생도착순서)
    {
        List<학생정보> 결과 = new();

        foreach (학생정보 학생 in 대기열.목록)
        {
            if (학생 != null && 학생.도착순서 < 선택학생도착순서)
                결과.Add(학생);
        }

        return 결과;
    }

    private int 빈자리찾기()
    {
        int 사용할자리수 = 설정 == null ? 자리위치.Length : Mathf.Min(설정.활성자리수, 자리위치.Length);

        for (int i = 0; i < 사용할자리수; i++)
        {
            if (!자리사용중[i] && 자리위치[i] != null)
                return i;
        }

        return -1;
    }

    private void OnValidate()
    {
        줄간격 = Mathf.Max(0.1f, 줄간격);
        학생사이여백 = Mathf.Max(0f, 학생사이여백);
        비켜서기간격 = Mathf.Max(0.1f, 비켜서기간격);
        이동제한시간 = Mathf.Max(0.1f, 이동제한시간);
    }
}
