using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

[DisallowMultipleComponent]
public class 학생정보 : MonoBehaviour
{
    [Header("학생 정보")]
    [SerializeField] private 학생계급 계급 = 학생계급.일학년;
    [SerializeField, Min(0.1f)] private float 소요시간 = 3f;
    [FormerlySerializedAs("studentName")]
    [SerializeField] private string 학생이름 = "아무개";
    [SerializeField, Min(1)] private int 나이 = 8;

    [Header("표시")]
    [SerializeField] private SpriteRenderer 학생이미지;
    [SerializeField] private TMP_Text 이름표;
    [SerializeField] private TMP_Text 남은시간표;
    [SerializeField, Range(0.3f, 1f)] private float 크기배율 = 0.78f;
    [SerializeField] private Vector2 먹는시간범위 = new Vector2(2f, 10f);
    [SerializeField] private Vector2 가로크기범위 = new Vector2(0.75f, 1.8f);

    private Vector3 이미지기본크기 = Vector3.one;
    private AudioSource 효과음소스;
    private Sprite 기본이미지;
    private Sprite 라운드로빈중단이미지;
    private AudioClip 라운드로빈중단효과음;
    private float 중단이미지지속시간;
    private Coroutine 중단연출;
    private CircleCollider2D 충돌체;
    private 쪼물쪼물 바운스;
    private SpriteRenderer 게이지배경;
    private SpriteRenderer 게이지채움;
    private SpriteRenderer 정보배경;
    private SortingGroup 정렬그룹;
    private BoxCollider2D 퇴학버튼;
    private GameObject 퇴학버튼문구;
    private 급식실관리자 급식실;
    private bool 게이지표시 = true;
    private bool 퇴학중;
    private static Sprite 흰색스프라이트;
    private static 학생정보 선택된학생;
    private static int 마지막클릭프레임 = -1;

    public 학생계급 계급값 => 계급;
    public float 전체소요시간 => 소요시간;
    public float 남은시간 { get; private set; }
    public string 이름 => 학생이름;
    public int 나이값 => 나이;
    public bool 줄서는중 { get; set; }
    public bool 식사중 { get; set; }
    public long 도착순서 { get; set; }
    public bool 퇴학중임 => 퇴학중;
    public 싸돌아댕기기 이동기 { get; private set; }
    public float 충돌반경 => 충돌체 != null ? 충돌체.radius * Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.y)) : 0.5f;

    private void Awake()
    {
        정렬그룹 = GetComponent<SortingGroup>();
        if (정렬그룹 == null)
            정렬그룹 = gameObject.AddComponent<SortingGroup>();
        정렬그룹.sortingOrder = 5;
        이동기 = GetComponent<싸돌아댕기기>();
        효과음소스 = GetComponent<AudioSource>();
        충돌체 = GetComponent<CircleCollider2D>();
        if (충돌체 != null)
            충돌체.isTrigger = true;

        if (학생이미지 == null)
            학생이미지 = GetComponentInChildren<SpriteRenderer>();

        if (학생이미지 != null)
        {
            이미지기본크기 = 학생이미지.transform.localScale;
            바운스 = 학생이미지.GetComponent<쪼물쪼물>();
        }

        남은시간 = 소요시간;
        먹는시간에맞춰가로크기설정();
        표시요소준비();
        화면갱신(true);
    }

    private void Update()
    {
        if (Time.timeScale == 0f)
            return;

        if (퇴학중)
            return;

        Mouse 마우스 = Mouse.current;
        if (마우스 == null || !마우스.leftButton.wasPressedThisFrame || 마지막클릭프레임 == Time.frameCount || 충돌체 == null)
            return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        Camera 카메라 = Camera.main;
        if (카메라 == null)
            return;

        Vector3 화면위치 = 마우스.position.ReadValue();
        Vector3 월드위치 = 카메라.ScreenToWorldPoint(화면위치);
        if (선택된학생 == this && 퇴학버튼 != null && 퇴학버튼.OverlapPoint(월드위치))
        {
            마지막클릭프레임 = Time.frameCount;
            급식실?.퇴학처리(this);
            return;
        }

        if (!충돌체.OverlapPoint(월드위치))
            return;

        마지막클릭프레임 = Time.frameCount;
        if (선택된학생 != null && 선택된학생 != this)
            선택된학생.정보표시설정(false);

        bool 다시누름 = 선택된학생 == this;
        정보표시설정(!다시누름);
        선택된학생 = 다시누름 ? null : this;
    }

    private void OnDestroy()
    {
        if (선택된학생 == this)
            선택된학생 = null;
    }

    public void 퇴학연출시작()
    {
        if (퇴학중)
            return;

        퇴학중 = true;
        정보표시설정(false);
        if (선택된학생 == this)
            선택된학생 = null;
        이동기?.정지();
        if (이동기 != null)
            이동기.enabled = false;
        배고파 배고픔 = GetComponent<배고파>();
        if (배고픔 != null)
            배고픔.enabled = false;
        불만 불만표시 = GetComponentInChildren<불만>(true);
        불만표시?.숨기기();
        if (불만표시 != null)
            불만표시.enabled = false;
        if (바운스 != null)
            바운스.enabled = false;
        if (중단연출 != null)
        {
            StopCoroutine(중단연출);
            중단연출 = null;
        }
        if (효과음소스 != null)
            효과음소스.Stop();
        if (충돌체 != null)
            충돌체.enabled = false;
        if (퇴학버튼 != null)
            퇴학버튼.enabled = false;
    }

    public void 퇴학페이드시작(float 지속시간)
    {
        if (퇴학중)
            StartCoroutine(퇴학페이드아웃(Mathf.Max(0.01f, 지속시간)));
    }

    private IEnumerator 퇴학페이드아웃(float 지속시간)
    {
        SpriteRenderer[] 스프라이트들 = GetComponentsInChildren<SpriteRenderer>(true);
        TMP_Text[] 글자들 = GetComponentsInChildren<TMP_Text>(true);
        float[] 스프라이트투명도 = new float[스프라이트들.Length];
        float[] 글자투명도 = new float[글자들.Length];
        for (int i = 0; i < 스프라이트들.Length; i++)
            스프라이트투명도[i] = 스프라이트들[i].color.a;
        for (int i = 0; i < 글자들.Length; i++)
            글자투명도[i] = 글자들[i].color.a;

        float 경과 = 0f;
        while (경과 < 지속시간)
        {
            경과 += Time.deltaTime;
            float 남은비율 = 1f - Mathf.Clamp01(경과 / 지속시간);
            for (int i = 0; i < 스프라이트들.Length; i++)
            {
                Color 색 = 스프라이트들[i].color;
                색.a = 스프라이트투명도[i] * 남은비율;
                스프라이트들[i].color = 색;
            }
            for (int i = 0; i < 글자들.Length; i++)
            {
                Color 색 = 글자들[i].color;
                색.a = 글자투명도[i] * 남은비율;
                글자들[i].color = 색;
            }
            yield return null;
        }

        Destroy(gameObject);
    }

    // 소환 시 정해진 계급, 이름, 먹는 시간과 이미지를 학생에게 적용한다.
    public void 초기화(학생계급 새계급, string 새이름, float 새소요시간, Sprite 새이미지)
    {
        계급 = 새계급;
        학생이름 = string.IsNullOrWhiteSpace(새이름) ? "아무개" : 새이름;
        나이 = 새계급 switch
        {
            학생계급.일학년 => 8,
            학생계급.이학년 => 9,
            학생계급.삼학년 => 10,
            학생계급.학생회장 => 10,
            학생계급.교장선생님 => 50,
            _ => 8
        };
        소요시간 = Mathf.Max(0.1f, 새소요시간);
        남은시간 = 소요시간;

        if (학생이미지 != null && 새이미지 != null)
            학생이미지.sprite = 새이미지;
        기본이미지 = 학생이미지 != null ? 학생이미지.sprite : null;

        먹는시간에맞춰가로크기설정();
        화면갱신(true);
        gameObject.name = $"{계급표시이름()}_{학생이름}";
    }

    // 한 프레임 동안 먹은 시간을 차감하고 완료 여부를 반환한다.
    public bool 식사진행(float 경과시간)
    {
        남은시간 = Mathf.Max(0f, 남은시간 - Mathf.Max(0f, 경과시간));
        게이지갱신();
        남은시간문구갱신();
        return 남은시간 <= 0f;
    }

    public void 남은시간표시설정(bool 표시)
    {
        화면갱신(표시);
    }

    public void 라운드로빈중단연출설정(Sprite 이미지, AudioClip 효과음, float 지속시간)
    {
        라운드로빈중단이미지 = 이미지;
        라운드로빈중단효과음 = 효과음;
        중단이미지지속시간 = Mathf.Max(0f, 지속시간);
    }

    public void 라운드로빈중단연출()
    {
        if (중단연출 != null)
            StopCoroutine(중단연출);
        if (학생이미지 != null)
            학생이미지.sprite = 기본이미지;
        if (효과음소스 != null && 라운드로빈중단효과음 != null)
            효과음소스.PlayOneShot(라운드로빈중단효과음);
        if (학생이미지 != null && 라운드로빈중단이미지 != null && 중단이미지지속시간 > 0f)
            중단연출 = StartCoroutine(중단이미지표시());
    }

    private IEnumerator 중단이미지표시()
    {
        학생이미지.sprite = 라운드로빈중단이미지;
        yield return new WaitForSeconds(중단이미지지속시간);
        학생이미지.sprite = 기본이미지;
        중단연출 = null;
    }

    private void OnDisable()
    {
        if (학생이미지 != null && 기본이미지 != null)
            학생이미지.sprite = 기본이미지;
        중단연출 = null;
    }

    public void 효과음볼륨설정(float 볼륨)
    {
        if (효과음소스 != null)
            효과음소스.volume = Mathf.Clamp01(볼륨);
    }

    public void 급식실설정(급식실관리자 관리자)
    {
        급식실 = 관리자;
    }

    public void 창문뒤표시설정(bool 식사중표시)
    {
        if (정렬그룹 != null)
            정렬그룹.sortingOrder = 식사중표시 ? -1 : 5;
    }

    public void 창문앞표시설정()
    {
        if (정렬그룹 != null)
            정렬그룹.sortingOrder = 22;
    }

    public string 계급표시이름()
    {
        return 계급 switch
        {
            학생계급.일학년 => "1학년",
            학생계급.이학년 => "2학년",
            학생계급.삼학년 => "3학년",
            학생계급.학생회장 => "학생회장",
            학생계급.교장선생님 => "교장선생님",
            _ => "학생"
        };
    }

    private void 먹는시간에맞춰가로크기설정()
    {
        if (학생이미지 == null)
            return;

        float 최소시간 = Mathf.Min(먹는시간범위.x, 먹는시간범위.y);
        float 최대시간 = Mathf.Max(먹는시간범위.x, 먹는시간범위.y);
        float 비율 = Mathf.InverseLerp(최소시간, 최대시간, 소요시간);
        float 가로배율 = Mathf.Lerp(가로크기범위.x, 가로크기범위.y, 비율);
        Vector3 크기 = new Vector3(이미지기본크기.x * 가로배율 * 크기배율, 이미지기본크기.y * 크기배율, 이미지기본크기.z);

        if (바운스 != null)
            바운스.SetBaseScale(크기);
        else
            학생이미지.transform.localScale = 크기;

        if (충돌체 != null && 학생이미지.sprite != null)
        {
            Vector2 그림크기 = 학생이미지.sprite.bounds.size;
            float 폭 = 그림크기.x * Mathf.Abs(크기.x);
            float 높이 = 그림크기.y * Mathf.Abs(크기.y);
            float 반경 = Mathf.Sqrt(폭 * 폭 + 높이 * 높이) * 0.54f;
            충돌체.radius = Mathf.Clamp(반경, 0.28f, 0.39f);
        }
    }

    private void 화면갱신(bool 남은시간표시)
    {
        게이지표시 = 남은시간표시;
        게이지갱신();

        if (이름표 != null)
            이름표.text = $"이름  {학생이름}\n계급  {계급표시이름()}\n먹는 시간  {소요시간:0.0}초";

        if (남은시간표 != null)
        {
            남은시간표.gameObject.SetActive(남은시간표시);
            남은시간문구갱신();
        }
    }

    private void 남은시간문구갱신()
    {
        if (남은시간표 != null && 남은시간표.gameObject.activeSelf)
            남은시간표.text = $"{남은시간:0.0}초";
    }

    private void 표시요소준비()
    {
        if (흰색스프라이트 == null)
            흰색스프라이트 = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);

        게이지배경 = 사각형만들기("남은 시간 게이지 배경", new Vector3(0f, -0.58f, 0f), new Vector3(0.64f, 0.1f, 1f), new Color(0.12f, 0.15f, 0.2f, 0.9f), 20);
        게이지채움 = 사각형만들기("남은 시간 게이지", new Vector3(0f, -0.58f, 0f), new Vector3(0.6f, 0.06f, 1f), new Color(0.25f, 0.75f, 0.4f), 21);

        if (이름표 != null)
        {
            GameObject 상태창 = new GameObject("학생 상태창");
            상태창.transform.SetParent(transform, false);
            SortingGroup 상태창그룹 = 상태창.AddComponent<SortingGroup>();
            상태창그룹.sortAtRoot = true;
            상태창그룹.sortingOrder = 31;
            이름표.transform.SetParent(상태창.transform, false);
            이름표.transform.localPosition = new Vector3(0f, 0.97f, 0f);
            이름표.rectTransform.sizeDelta = new Vector2(9f, 3f);
            이름표.fontSize = 10f;
            이름표.alignment = TextAlignmentOptions.Center;
            이름표.gameObject.SetActive(false);
            정보배경 = 사각형만들기("학생 정보 배경", new Vector3(0f, 0.97f, 0f), new Vector3(2.4f, 0.86f, 1f), new Color(0.12f, 0.15f, 0.2f, 0.96f), 19, 상태창.transform);
            정보배경.gameObject.SetActive(false);

            SpriteRenderer 버튼배경 = 사각형만들기("너 퇴학 버튼", new Vector3(0f, 0.35f, 0f), new Vector3(1.7f, 0.32f, 1f), new Color(0.7f, 0.13f, 0.15f, 1f), 22, 상태창.transform);
            퇴학버튼 = 버튼배경.gameObject.AddComponent<BoxCollider2D>();
            퇴학버튼.isTrigger = true;
            퇴학버튼.size = Vector2.one;
            버튼배경.gameObject.SetActive(false);

            GameObject 버튼문구 = new GameObject("너 퇴학 문구");
            버튼문구.transform.SetParent(상태창.transform, false);
            TextMeshPro 문구 = 버튼문구.AddComponent<TextMeshPro>();
            문구.font = 이름표.font;
            문구.text = "너 퇴학";
            문구.fontSize = 3.2f;
            문구.alignment = TextAlignmentOptions.Center;
            문구.sortingOrder = 23;
            문구.rectTransform.sizeDelta = new Vector2(15f, 2.4f);
            문구.transform.localPosition = new Vector3(0f, 0.35f, 0f);
            문구.transform.localScale = Vector3.one * 0.1f;
            퇴학버튼문구 = 버튼문구;
            퇴학버튼문구.SetActive(false);
        }
    }

    private SpriteRenderer 사각형만들기(string 이름, Vector3 위치, Vector3 크기, Color 색, int 정렬순서, Transform 부모 = null)
    {
        GameObject 오브젝트 = new GameObject(이름);
        오브젝트.transform.SetParent(부모 != null ? 부모 : transform, false);
        오브젝트.transform.localPosition = 위치;
        오브젝트.transform.localScale = 크기;
        SpriteRenderer 렌더러 = 오브젝트.AddComponent<SpriteRenderer>();
        렌더러.sprite = 흰색스프라이트;
        렌더러.color = 색;
        렌더러.sortingOrder = 정렬순서;
        return 렌더러;
    }

    private void 게이지갱신()
    {
        if (게이지배경 == null || 게이지채움 == null)
            return;

        if (게이지배경.gameObject.activeSelf != 게이지표시)
            게이지배경.gameObject.SetActive(게이지표시);
        if (게이지채움.gameObject.activeSelf != 게이지표시)
            게이지채움.gameObject.SetActive(게이지표시);
        float 비율 = Mathf.Clamp01(남은시간 / Mathf.Max(0.1f, 소요시간));
        게이지채움.transform.localScale = new Vector3(0.6f * 비율, 0.06f, 1f);
        게이지채움.transform.localPosition = new Vector3(-0.3f * (1f - 비율), -0.58f, 0f);
    }

    private void 정보표시설정(bool 표시)
    {
        if (정보배경 != null)
            정보배경.gameObject.SetActive(표시);
        if (이름표 != null)
            이름표.gameObject.SetActive(표시);
        if (퇴학버튼 != null)
            퇴학버튼.gameObject.SetActive(표시);
        if (퇴학버튼문구 != null)
            퇴학버튼문구.SetActive(표시);
    }

    private void OnValidate()
    {
        소요시간 = Mathf.Max(0.1f, 소요시간);
        나이 = Mathf.Max(1, 나이);
        크기배율 = Mathf.Clamp(크기배율, 0.3f, 1f);
    }
}
