using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

[DisallowMultipleComponent]
public class 학생정보 : MonoBehaviour
{
    [Header("학생 정보")]
    [SerializeField] private 학생계급 계급 = 학생계급.일학년;
    [SerializeField, Min(0.1f)] private float 소요시간 = 3f;
    [FormerlySerializedAs("studentName")]
    [SerializeField] private string 학생이름 = "아무개";

    [Header("표시")]
    [SerializeField] private SpriteRenderer 학생이미지;
    [SerializeField] private TMP_Text 이름표;
    [SerializeField] private TMP_Text 남은시간표;
    [SerializeField] private Vector2 먹는시간범위 = new Vector2(2f, 10f);
    [SerializeField] private Vector2 가로크기범위 = new Vector2(0.75f, 1.8f);

    private Vector3 이미지기본크기 = Vector3.one;
    private AudioSource 효과음소스;
    private CircleCollider2D 충돌체;
    private 쪼물쪼물 바운스;

    public 학생계급 계급값 => 계급;
    public float 전체소요시간 => 소요시간;
    public float 남은시간 { get; private set; }
    public string 이름 => 학생이름;
    public bool 줄서는중 { get; set; }
    public bool 식사중 { get; set; }
    public long 도착순서 { get; set; }
    public 싸돌아댕기기 이동기 { get; private set; }
    public float 충돌반경 => 충돌체 != null ? 충돌체.radius * Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.y)) : 0.5f;

    private void Awake()
    {
        이동기 = GetComponent<싸돌아댕기기>();
        효과음소스 = GetComponent<AudioSource>();
        충돌체 = GetComponent<CircleCollider2D>();

        if (학생이미지 == null)
            학생이미지 = GetComponentInChildren<SpriteRenderer>();

        if (학생이미지 != null)
        {
            이미지기본크기 = 학생이미지.transform.localScale;
            바운스 = 학생이미지.GetComponent<쪼물쪼물>();
        }

        남은시간 = 소요시간;
        먹는시간에맞춰가로크기설정();
        화면갱신(true);
    }

    // 소환 시 정해진 계급, 이름, 먹는 시간과 이미지를 학생에게 적용한다.
    public void 초기화(학생계급 새계급, string 새이름, float 새소요시간, Sprite 새이미지)
    {
        계급 = 새계급;
        학생이름 = string.IsNullOrWhiteSpace(새이름) ? "아무개" : 새이름;
        소요시간 = Mathf.Max(0.1f, 새소요시간);
        남은시간 = 소요시간;

        if (학생이미지 != null && 새이미지 != null)
            학생이미지.sprite = 새이미지;

        먹는시간에맞춰가로크기설정();
        화면갱신(true);
        gameObject.name = $"{계급표시이름()}_{학생이름}";
    }

    // 한 프레임 동안 먹은 시간을 차감하고 완료 여부를 반환한다.
    public bool 식사진행(float 경과시간)
    {
        남은시간 = Mathf.Max(0f, 남은시간 - Mathf.Max(0f, 경과시간));
        화면갱신(남은시간표 != null && 남은시간표.gameObject.activeSelf);
        return 남은시간 <= 0f;
    }

    public void 남은시간표시설정(bool 표시)
    {
        화면갱신(표시);
    }

    public void 효과음볼륨설정(float 볼륨)
    {
        if (효과음소스 != null)
            효과음소스.volume = Mathf.Clamp01(볼륨);
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
        Vector3 크기 = new Vector3(이미지기본크기.x * 가로배율, 이미지기본크기.y, 이미지기본크기.z);

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
            충돌체.radius = Mathf.Max(0.5f, 반경);
        }
    }

    private void 화면갱신(bool 남은시간표시)
    {
        if (이름표 != null)
            이름표.text = $"{학생이름}  |  {계급표시이름()}";

        if (남은시간표 != null)
        {
            남은시간표.gameObject.SetActive(남은시간표시);
            남은시간표.text = $"{남은시간:0.0}s";
        }
    }

    private void OnValidate()
    {
        소요시간 = Mathf.Max(0.1f, 소요시간);
    }
}
