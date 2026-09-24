using UnityEngine;

public class 쪼물쪼물 : MonoBehaviour
{
    [Header("달리는 중인지")]
    public bool isRunning = true;

    [Header("통통 튀는 정도")]
    [SerializeField] private float bounceHeight = 0.12f;
    [SerializeField] private float bounceSpeed = 10f;
    [SerializeField] private float tiltAngle = 8f;
    [SerializeField] private float squashAmount = 0.08f;

    private Vector3 startPosition;
    private Vector3 startScale;
    private Quaternion startRotation;
    private float phase;
    private float blend;
    private float speedMultiplier = 1f;
    private bool facingRight;

    public Vector3 BasePosition => startPosition;

    private void Awake()
    {
        startPosition = transform.localPosition;
        startScale = transform.localScale;
        startRotation = transform.localRotation;
    }

    private void Update()
    {
        float targetBlend = isRunning ? 1f : 0f;
        blend = Mathf.MoveTowards(blend, targetBlend, Time.deltaTime * 8f);

        if (isRunning)
            phase += Time.deltaTime * bounceSpeed * speedMultiplier;

        float wave = Mathf.Sin(phase);
        float bounce = Mathf.Abs(wave) * bounceHeight * blend;
        float tilt = wave * tiltAngle * blend;
        float squash = Mathf.Abs(wave) * squashAmount * blend;

        transform.localPosition = startPosition + Vector3.up * bounce;

        transform.localRotation =
            startRotation * Quaternion.Euler(0f, 0f, tilt);

        transform.localScale = new Vector3(
            startScale.x * (facingRight ? -1f : 1f) * (1f + squash),
            startScale.y * (1f - squash),
            startScale.z
        );
    }

    public void SetRunning(bool running)
    {
        isRunning = running;
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = Mathf.Max(0f, multiplier);
    }

    public void SetFacingRight(bool shouldFaceRight)
    {
        facingRight = shouldFaceRight;
    }

    public void SetBasePosition(Vector3 position)
    {
        startPosition = position;
    }

    public void SetBaseScale(Vector3 scale)
    {
        startScale = scale;
        transform.localScale = scale;
    }

    private void OnDisable()
    {
        transform.localPosition = startPosition;
        transform.localRotation = startRotation;
        transform.localScale = startScale;
    }
}
