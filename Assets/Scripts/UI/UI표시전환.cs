using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class UI표시전환 : MonoBehaviour
{
    [SerializeField] private Canvas[] canvases;
    [SerializeField] private Button toggleButton;
    [SerializeField] private Image iconImage;
    [SerializeField] private Sprite visibleIcon;
    [SerializeField] private Sprite hiddenIcon;

    private bool 표시중 = true;

    private void OnEnable()
    {
        if (toggleButton != null)
            toggleButton.onClick.AddListener(표시전환);
    }

    private void OnDisable()
    {
        if (toggleButton != null)
            toggleButton.onClick.RemoveListener(표시전환);
    }

    private void 표시전환()
    {
        표시중 = !표시중;
        foreach (Canvas 캔버스 in canvases)
        {
            if (캔버스 == null)
                continue;

            캔버스.enabled = 표시중;
            GraphicRaycaster 레이캐스터 = 캔버스.GetComponent<GraphicRaycaster>();
            if (레이캐스터 != null)
                레이캐스터.enabled = 표시중;
        }

        if (iconImage != null)
            iconImage.sprite = 표시중 ? visibleIcon : hiddenIcon;
    }
}
