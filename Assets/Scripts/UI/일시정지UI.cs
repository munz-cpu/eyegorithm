using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class 일시정지UI : MonoBehaviour
{
    [SerializeField] private Button 버튼;
    [SerializeField] private Image 정지왼쪽;
    [SerializeField] private Image 정지오른쪽;
    [SerializeField] private Image 재생아이콘;

    private bool 일시정지중;
    private float 이전시간배율 = 1f;
    private bool 이전오디오정지;

    private void OnEnable()
    {
        if (버튼 != null)
            버튼.onClick.AddListener(전환);
        아이콘갱신();
    }

    private void OnDisable()
    {
        if (버튼 != null)
            버튼.onClick.RemoveListener(전환);
        if (일시정지중)
            재개();
    }

    private void 전환()
    {
        if (일시정지중)
        {
            재개();
            return;
        }

        이전시간배율 = Time.timeScale;
        이전오디오정지 = AudioListener.pause;
        일시정지중 = true;
        Time.timeScale = 0f;
        AudioListener.pause = true;
        아이콘갱신();
    }

    private void 재개()
    {
        Time.timeScale = 이전시간배율;
        AudioListener.pause = 이전오디오정지;
        일시정지중 = false;
        아이콘갱신();
    }

    private void 아이콘갱신()
    {
        if (정지왼쪽 != null)
            정지왼쪽.enabled = !일시정지중;
        if (정지오른쪽 != null)
            정지오른쪽.enabled = !일시정지중;
        if (재생아이콘 != null)
            재생아이콘.enabled = 일시정지중;
    }
}
