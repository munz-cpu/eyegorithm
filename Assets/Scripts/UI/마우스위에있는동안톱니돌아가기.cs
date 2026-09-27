using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class 마우스위에있는동안톱니돌아가기 : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float rotateSpeed = 360f;

    private RectTransform 톱니;
    private bool 마우스위;

    private void Awake()
    {
        톱니 = GetComponent<RectTransform>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        마우스위 = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        마우스위 = false;
    }

    private void OnDisable()
    {
        마우스위 = false;
    }

    private void Update()
    {
        if (마우스위)
            톱니.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);
    }
}
