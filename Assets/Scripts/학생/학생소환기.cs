using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class 계급이미지
{
    public 학생계급 계급;
    public Sprite 이미지;
}

[Serializable]
public class 학생이름목록
{
    public string[] names;
}

[DisallowMultipleComponent]
public class 학생소환기 : MonoBehaviour
{
    [Header("연결")]
    [SerializeField] private 설정관리자 설정;
    [SerializeField] private 급식실관리자 급식실;
    [SerializeField] private 학생정보 학생프리팹;
    [SerializeField] private Transform 학생부모;
    [SerializeField] private Transform 소환위치;
    [SerializeField] private Collider2D 이동범위;
    [SerializeField] private TextAsset 학생이름파일;

    [Header("학생 설정")]
    [SerializeField] private 학생계급 소환할계급 = 학생계급.일학년;
    [SerializeField] private List<계급이미지> 계급별이미지 = new();
    [SerializeField] private Vector2 먹는시간범위 = new Vector2(2f, 10f);
    [SerializeField, Min(0f)] private float 아래쪽등장거리 = 2f;
    [SerializeField, Min(0.1f)] private float 동시소환간격 = 1.8f;

    private 학생이름목록 이름목록 = new();
    private readonly HashSet<학생계급> 소환중인특수계급 = new();
    private readonly HashSet<int> 사용중인소환자리 = new();
    private int 소환중인원;

    private void Awake()
    {
        이름불러오기();
    }

    public void 소환계급설정(int 드롭다운번호)
    {
        소환할계급 = (학생계급)Mathf.Clamp(드롭다운번호 + 1, 1, 5);
    }

    // 현재 선택된 계급의 학생을 한 명 만들고 아래쪽 등장 연출을 시작한다.
    public void 학생소환()
    {
        if (급식실 == null || 학생프리팹 == null || 소환위치 == null)
            return;

        if (급식실.현재인원 + 소환중인원 >= (설정 != null ? 설정.최대학생수 : 설정관리자.절대최대인원))
        {
            급식실.경고표시("더 이상 학생을 소환할 수 없습니다!");
            return;
        }

        if ((소환할계급 == 학생계급.학생회장 || 소환할계급 == 학생계급.교장선생님) &&
            (급식실.특정계급존재(소환할계급) || 소환중인특수계급.Contains(소환할계급)))
        {
            급식실.경고표시($"{계급이름(소환할계급)}은 1명만 있을 수 있습니다!");
            return;
        }

        int 소환자리 = 0;
        while (사용중인소환자리.Contains(소환자리))
            소환자리++;

        Vector3 등장목표 = 소환위치.position + Vector3.right * (소환자리 * 동시소환간격);
        Vector3 시작위치 = 등장목표 + Vector3.down * 아래쪽등장거리;
        학생정보 학생 = Instantiate(학생프리팹, 시작위치, Quaternion.identity, 학생부모);
        float 최소 = Mathf.Min(먹는시간범위.x, 먹는시간범위.y);
        float 최대 = Mathf.Max(먹는시간범위.x, 먹는시간범위.y);
        학생.초기화(소환할계급, 무작위이름(), UnityEngine.Random.Range(최소, 최대), 계급이미지찾기(소환할계급));
        학생.이동기?.이동범위설정(이동범위);
        학생.이동기?.속도배율설정(설정 != null ? 설정.틱배율 : 1f);
        소환중인원++;
        사용중인소환자리.Add(소환자리);
        if (소환할계급 == 학생계급.학생회장 || 소환할계급 == 학생계급.교장선생님)
            소환중인특수계급.Add(소환할계급);
        StartCoroutine(등장처리(학생, 등장목표, 소환자리, 소환할계급));
    }

    private IEnumerator 등장처리(학생정보 학생, Vector3 등장목표, int 소환자리, 학생계급 등장계급)
    {
        if (학생 == null)
        {
            소환중인원 = Mathf.Max(0, 소환중인원 - 1);
            사용중인소환자리.Remove(소환자리);
            소환중인특수계급.Remove(등장계급);
            yield break;
        }

        if (학생.이동기 == null)
        {
            소환중인원 = Mathf.Max(0, 소환중인원 - 1);
            사용중인소환자리.Remove(소환자리);
            소환중인특수계급.Remove(등장계급);
            Destroy(학생.gameObject);
            yield break;
        }

        학생.이동기.목표로이동(등장목표);

        while (학생 != null && !학생.이동기.목표에도착함)
            yield return null;

        소환중인원 = Mathf.Max(0, 소환중인원 - 1);
        사용중인소환자리.Remove(소환자리);
        소환중인특수계급.Remove(등장계급);

        if (학생 == null)
            yield break;

        급식실.학생등록(학생);

        if (설정 == null || !설정.열림)
            학생.이동기.배회시작();
    }

    private void 이름불러오기()
    {
        if (학생이름파일 == null)
            return;

        try
        {
            이름목록 = JsonUtility.FromJson<학생이름목록>(학생이름파일.text) ?? new 학생이름목록();
        }
        catch (Exception 예외)
        {
            Debug.LogWarning($"studentName.json을 읽을 수 없습니다: {예외.Message}", this);
            이름목록 = new 학생이름목록();
        }
    }

    private string 무작위이름()
    {
        if (이름목록.names == null || 이름목록.names.Length == 0)
            return "아무개";

        return 이름목록.names[UnityEngine.Random.Range(0, 이름목록.names.Length)];
    }

    private Sprite 계급이미지찾기(학생계급 계급)
    {
        계급이미지 항목 = 계급별이미지.Find(값 => 값 != null && 값.계급 == 계급);
        return 항목 != null ? 항목.이미지 : null;
    }

    private static string 계급이름(학생계급 계급)
    {
        return 계급 switch
        {
            학생계급.학생회장 => "학생회장",
            학생계급.교장선생님 => "교장선생님",
            학생계급.삼학년 => "3학년",
            학생계급.이학년 => "2학년",
            _ => "1학년"
        };
    }

    private void OnValidate()
    {
        아래쪽등장거리 = Mathf.Max(0f, 아래쪽등장거리);
        동시소환간격 = Mathf.Max(0.1f, 동시소환간격);
    }
}
