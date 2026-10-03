using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

[RequireComponent(typeof(Camera))]
[DisallowMultipleComponent]
public class 카메라확대이동 : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float 최소크기 = 2f;
    [SerializeField, Min(0.1f)] private float 최대크기 = 8f;
    [SerializeField, Range(0.01f, 1f)] private float 휠감도 = 0.25f;
    [SerializeField, Min(0f)] private float 드래그시작거리 = 4f;

    private Camera 대상카메라;
    private Vector3 초기위치;
    private float 초기크기;
    private ButtonControl 드래그버튼;
    private Vector2 시작화면위치;
    private Vector2 이전화면위치;
    private bool 이동중;

    private void Awake()
    {
        대상카메라 = GetComponent<Camera>();
        초기위치 = transform.position;
        초기크기 = 대상카메라.orthographicSize;
    }

    public void 화면초기화()
    {
        transform.position = 초기위치;
        대상카메라.orthographicSize = 초기크기;
        드래그버튼 = null;
        이동중 = false;
    }

    private void Update()
    {
        Mouse 마우스 = Mouse.current;
        if (마우스 == null || !대상카메라.orthographic)
            return;

        Vector2 화면위치 = 마우스.position.ReadValue();
        if (화면위치.x < 0f || 화면위치.y < 0f || 화면위치.x > Screen.width || 화면위치.y > Screen.height)
        {
            드래그버튼 = null;
            이동중 = false;
            return;
        }

        bool UI위 = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        float 휠 = 마우스.scroll.ReadValue().y;
        if (!UI위 && !Mathf.Approximately(휠, 0f))
            확대축소(화면위치, 휠);

        if (드래그버튼 == null)
        {
            if (UI위)
                return;

            if (마우스.rightButton.wasPressedThisFrame)
                드래그버튼 = 마우스.rightButton;
            else if (마우스.leftButton.wasPressedThisFrame && !학생위인가(화면위치))
                드래그버튼 = 마우스.leftButton;

            시작화면위치 = 화면위치;
            이전화면위치 = 화면위치;
            이동중 = false;
            return;
        }

        if (!드래그버튼.isPressed)
        {
            드래그버튼 = null;
            이동중 = false;
            return;
        }

        if (!이동중 && (화면위치 - 시작화면위치).sqrMagnitude < 드래그시작거리 * 드래그시작거리)
            return;

        이동중 = true;
        Vector3 이전월드 = 화면에서월드로(이전화면위치);
        Vector3 현재월드 = 화면에서월드로(화면위치);
        transform.position += 이전월드 - 현재월드;
        이전화면위치 = 화면위치;
    }

    private void 확대축소(Vector2 화면위치, float 휠)
    {
        Vector3 확대전 = 화면에서월드로(화면위치);
        float 배율 = Mathf.Pow(1f - 휠감도, 휠 / 120f);
        대상카메라.orthographicSize = Mathf.Clamp(대상카메라.orthographicSize * 배율, 최소크기, 최대크기);
        transform.position += 확대전 - 화면에서월드로(화면위치);
    }

    private Vector3 화면에서월드로(Vector2 화면위치)
    {
        return 대상카메라.ScreenToWorldPoint(new Vector3(화면위치.x, 화면위치.y, -transform.position.z));
    }

    private bool 학생위인가(Vector2 화면위치)
    {
        Vector2 월드위치 = 화면에서월드로(화면위치);
        foreach (Collider2D 충돌체 in Physics2D.OverlapPointAll(월드위치))
        {
            if (충돌체.GetComponentInParent<학생정보>() != null)
                return true;
        }

        return false;
    }

    private void OnDisable()
    {
        드래그버튼 = null;
        이동중 = false;
    }

    private void OnValidate()
    {
        최대크기 = Mathf.Max(최소크기, 최대크기);
    }
}
