using UnityEngine;
using UnityEngine.Serialization;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public class 싸돌아댕기기 : MonoBehaviour
{
    private enum 이동상태
    {
        배회,
        배회대기,
        목표이동,
        정지
    }

    [Header("이동")]
    [FormerlySerializedAs("moveSpeed")]
    [SerializeField, Min(0f)] private float 이동속도 = 3f;
    [SerializeField, Min(0.01f)] private float 도착거리 = 0.08f;
    [FormerlySerializedAs("moveDurationRange")]
    [SerializeField] private Vector2 이동시간범위 = new Vector2(1.5f, 4f);
    [FormerlySerializedAs("waitDurationRange")]
    [SerializeField] private Vector2 대기시간범위 = new Vector2(0.8f, 2.5f);

    [Header("이동 범위")]
    [FormerlySerializedAs("moveArea")]
    [SerializeField] private Collider2D 이동범위;
    [FormerlySerializedAs("edgePadding")]
    [SerializeField, Min(0f)] private float 가장자리여백 = 0.1f;

    [Header("연출")]
    [FormerlySerializedAs("bouncy")]
    [SerializeField] private 쪼물쪼물 바운스;

    private Rigidbody2D 몸체;
    private 이동상태 상태;
    private Vector2 이동방향;
    private Vector2 목표위치;
    private float 상태남은시간;
    private float 속도배율 = 1f;

    public bool 목표에도착함 { get; private set; } = true;
    public float 현재이동속도 => 이동속도 * 속도배율;

    private void Awake()
    {
        몸체 = GetComponent<Rigidbody2D>();
        몸체.gravityScale = 0f;
        몸체.freezeRotation = true;

        if (바운스 == null)
            바운스 = GetComponentInChildren<쪼물쪼물>();
    }

    private void OnEnable()
    {
        배회시작();
    }

    private void FixedUpdate()
    {
        float 경과시간 = Time.fixedDeltaTime * 속도배율;

        if (상태 == 이동상태.목표이동)
            목표로이동처리(경과시간);
        else if (상태 == 이동상태.배회)
            배회처리(경과시간);
        else if (상태 == 이동상태.배회대기)
            대기처리(경과시간);
        else
            정지처리();
    }

    // 외부 시스템이 지정한 월드 좌표까지 학생을 이동시킨다.
    public void 목표로이동(Vector3 위치)
    {
        목표위치 = 위치;
        목표에도착함 = false;
        상태 = 이동상태.목표이동;
        바운스설정(true, 목표위치.x > 몸체.position.x);
    }

    public void 정지()
    {
        상태 = 이동상태.정지;
        목표에도착함 = true;
        정지처리();
    }

    public void 배회시작()
    {
        상태 = 이동상태.배회;
        목표에도착함 = true;
        새배회방향설정();
    }

    public void 이동범위설정(Collider2D 범위)
    {
        이동범위 = 범위;
    }

    public void 속도배율설정(float 배율)
    {
        속도배율 = Mathf.Max(0.05f, 배율);

        if (바운스 != null)
            바운스.SetSpeedMultiplier(속도배율);
    }

    private void 목표로이동처리(float 경과시간)
    {
        Vector2 현재 = 몸체.position;
        Vector2 차이 = 목표위치 - 현재;

        if (차이.sqrMagnitude <= 도착거리 * 도착거리)
        {
            몸체.MovePosition(목표위치);
            정지();
            return;
        }

        Vector2 다음 = Vector2.MoveTowards(현재, 목표위치, 이동속도 * 경과시간);
        몸체.MovePosition(다음);
        바운스설정(true, 차이.x > 0f);
    }

    private void 배회처리(float 경과시간)
    {
        상태남은시간 -= 경과시간;
        Vector2 다음 = 몸체.position + 이동방향 * 이동속도 * 경과시간;

        if (이동범위 != null && !이동범위.bounds.Contains(다음))
        {
            다음 = 범위안으로제한(다음);
            몸체.MovePosition(다음);
            배회대기시작();
            return;
        }

        몸체.MovePosition(다음);

        if (상태남은시간 <= 0f)
            배회대기시작();
    }

    private void 대기처리(float 경과시간)
    {
        정지처리();
        상태남은시간 -= 경과시간;

        if (상태남은시간 <= 0f)
            새배회방향설정();
    }

    private void 새배회방향설정()
    {
        상태 = 이동상태.배회;
        이동방향 = Random.insideUnitCircle.normalized;

        if (이동방향.sqrMagnitude < 0.001f)
            이동방향 = Vector2.right;

        상태남은시간 = 범위랜덤(이동시간범위);
        바운스설정(true, 이동방향.x > 0f);
    }

    private void 배회대기시작()
    {
        상태 = 이동상태.배회대기;
        상태남은시간 = 범위랜덤(대기시간범위);
        바운스설정(false, false);
    }

    private void 정지처리()
    {
        몸체.linearVelocity = Vector2.zero;
        몸체.angularVelocity = 0f;
        바운스설정(false, false);
    }

    private Vector2 범위안으로제한(Vector2 위치)
    {
        Bounds 경계 = 이동범위.bounds;
        위치.x = Mathf.Clamp(위치.x, 경계.min.x + 가장자리여백, 경계.max.x - 가장자리여백);
        위치.y = Mathf.Clamp(위치.y, 경계.min.y + 가장자리여백, 경계.max.y - 가장자리여백);
        return 위치;
    }

    private void 바운스설정(bool 달리는중, bool 오른쪽)
    {
        if (바운스 == null)
            return;

        바운스.SetRunning(달리는중);
        if (달리는중)
            바운스.SetFacingRight(오른쪽);
    }

    private static float 범위랜덤(Vector2 범위)
    {
        float 최소 = Mathf.Max(0f, Mathf.Min(범위.x, 범위.y));
        float 최대 = Mathf.Max(최소, Mathf.Max(범위.x, 범위.y));
        return Random.Range(최소, 최대);
    }

    private void OnDisable()
    {
        if (몸체 != null)
            몸체.linearVelocity = Vector2.zero;

        if (바운스 != null)
            바운스.SetRunning(false);
    }

    private void OnValidate()
    {
        이동속도 = Mathf.Max(0f, 이동속도);
        도착거리 = Mathf.Max(0.01f, 도착거리);
        가장자리여백 = Mathf.Max(0f, 가장자리여백);
    }
}
