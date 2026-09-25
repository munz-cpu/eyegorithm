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
    [SerializeField] private Vector2 줄방향 = Vector2.left;
    [SerializeField, Min(0.1f)] private float 줄간격 = 0.9f;
    [SerializeField, Min(2)] private int 한줄인원 = 5;
    [SerializeField, Min(0.8f)] private float 줄행간격 = 1.25f;
    [SerializeField, Min(0f)] private float 학생사이여백 = 0.2f;
    [SerializeField] private Transform 급식실입구;
    [SerializeField] private Transform 급식실입구안쪽;
    [SerializeField] private Transform 급식실출구;
    [SerializeField] private Transform 퇴장위치;
    [SerializeField] private Transform[] 자리위치 = new Transform[0];
    [SerializeField, Min(0.1f)] private float 이동제한시간 = 8f;

    private readonly List<학생정보> 전체학생 = new();
    private readonly List<학생정보> 합류대기 = new();
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

        // 현재 줄 끝에 실제로 도착한 학생만 도착한 순서대로 대기열에 넣는다.
        for (int i = 0; i < 합류대기.Count;)
        {
            학생정보 학생 = 합류대기[i];
            if (학생 == null || !전체학생.Contains(학생))
            {
                합류대기.RemoveAt(i);
                continue;
            }

            if (열림 && 학생.이동기 != null && 학생.이동기.목표에도착함)
            {
                합류대기.RemoveAt(i);
                줄에추가(학생);
                continue;
            }

            i++;
        }

        if (열림 && !입장처리중 && 대기열.인원수 > 0 && 빈자리찾기() >= 0)
            StartCoroutine(다음학생입장());
    }

    // 소환된 학생을 전체 인원에 등록하고 현재 설정을 적용한다.
    public void 학생등록(학생정보 학생)
    {
        if (학생 == null || 전체학생.Contains(학생))
            return;

        전체학생.Add(학생);
        학생.이동기?.이동범위설정(학생이동범위);
        학생.이동기?.속도배율설정(설정 != null ? 설정.틱배율 : 1f);
        학생.남은시간표시설정(설정 == null || 설정.남은시간을표시함);
        학생.효과음볼륨설정(설정 != null ? 설정.학생효과음크기 : 1f);

        if (열림)
            줄합류시작(학생);
    }

    public void 학생등록해제(학생정보 학생)
    {
        합류대기.Remove(학생);
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
                    줄합류시작(학생);
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

            foreach (학생정보 학생 in 합류대기)
            {
                if (학생 != null)
                    학생.이동기?.배회시작();
            }

            합류대기.Clear();
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

        학생.도착순서 = 다음도착순서++;
        학생.줄서는중 = true;
        대기열.추가(학생);
        줄정렬();
    }

    private void 줄합류시작(학생정보 학생)
    {
        if (학생 == null || 학생.식사중 || 학생.줄서는중 || 합류대기.Contains(학생) || 줄시작점 == null)
            return;

        if (학생.이동기 == null)
        {
            줄에추가(학생);
            return;
        }

        합류대기.Add(학생);
        학생.이동기.목표로이동(줄위치(대기열.인원수));
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

        // 줄 길이가 바뀌어도 합류 대기 학생에게 빈 자리를 미리 배정하지 않는다.
        Vector2 합류위치 = 줄위치(대기열.인원수);
        foreach (학생정보 학생 in 합류대기)
        {
            if (학생 != null)
                학생.이동기?.목표로이동(합류위치);
        }
    }

    private Vector2 줄위치(int 번호)
    {
        float 최대반경 = 0f;
        foreach (학생정보 학생 in 대기열.목록)
        {
            if (학생 != null)
                최대반경 = Mathf.Max(최대반경, 학생.충돌반경);
        }

        float 간격 = Mathf.Max(줄간격, 최대반경 * 2f + 학생사이여백);
        Vector2 방향 = 줄방향.sqrMagnitude < 0.001f ? Vector2.left : 줄방향.normalized;
        int 행당인원 = Mathf.Max(2, 한줄인원);
        int 행 = 번호 / 행당인원;
        int 열 = 번호 % 행당인원;
        if (행 % 2 == 1)
            열 = 행당인원 - 1 - 열;

        return (Vector2)줄시작점.position + 방향 * (열 * 간격) + Vector2.down * (행 * 줄행간격);
    }

    // 선택된 학생을 입구로 보내고 남은 줄을 한 칸씩 당긴다.
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

        학생.줄서는중 = false;
        학생.식사중 = true;
        줄정렬();
        Vector3 자리 = 자리위치[자리번호].position;
        Vector3 입구 = 급식실입구 != null ? 급식실입구.position : 자리;
        Vector3 입구안쪽 = 급식실입구안쪽 != null
            ? 급식실입구안쪽.position
            : 입구 + Vector3.up * 0.6f;

        yield return 목표까지이동(학생, 입구);
        yield return 목표까지이동(학생, 입구안쪽);
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
        Vector3 자리비움위치 = new Vector3(
            Mathf.Min(학생.transform.position.x + 학생.충돌반경 * 2f + 학생사이여백, 출구.x),
            학생.transform.position.y, 학생.transform.position.z);
        yield return 목표까지이동(학생, 자리비움위치);
        자리사용중[자리번호] = false;
        if (학생 == null)
            yield break;

        Vector3 오른쪽통로 = new Vector3(출구.x, 학생.transform.position.y, 학생.transform.position.z);
        yield return 목표까지이동(학생, 오른쪽통로);
        yield return 목표까지이동(학생, 출구);

        if (학생 == null)
        {
            yield break;
        }

        if (학생.남은시간 <= 0f)
        {
            yield return 목표까지이동(학생, 퇴장위치 != null ? 퇴장위치.position : 학생.transform.position + Vector3.down * 3f);
            if (학생 == null)
                yield break;

            학생등록해제(학생);
            Destroy(학생.gameObject);
        }
        else
        {
            yield return 목표까지이동(학생, 출구 + Vector3.down * (학생.충돌반경 + 0.2f));
            학생.식사중 = false;

            if (열림)
                줄합류시작(학생);
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

    private int 빈자리찾기()
    {
        for (int i = 0; i < 자리위치.Length; i++)
        {
            if (!자리사용중[i] && 자리위치[i] != null)
                return i;
        }

        return -1;
    }

    private void OnValidate()
    {
        줄간격 = Mathf.Max(0.1f, 줄간격);
        한줄인원 = Mathf.Max(2, 한줄인원);
        줄행간격 = Mathf.Max(0.8f, 줄행간격);
        학생사이여백 = Mathf.Max(0f, 학생사이여백);
        이동제한시간 = Mathf.Max(0.1f, 이동제한시간);
    }
}
