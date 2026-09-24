using System.Collections;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class 경고메시지UI : MonoBehaviour
{
    [SerializeField] private TMP_Text 경고문구;
    [SerializeField] private CanvasGroup 캔버스그룹;
    [SerializeField, Min(0.1f)] private float 표시시간 = 2f;

    private Coroutine 숨김작업;

    private void Awake()
    {
        즉시숨기기();
    }

    // 기존 경고를 교체하고 정해진 시간 동안 새 문구를 보여 준다.
    public void 표시(string 내용)
    {
        if (경고문구 != null)
            경고문구.text = 내용;

        if (캔버스그룹 != null)
        {
            캔버스그룹.alpha = 1f;
            캔버스그룹.blocksRaycasts = false;
        }

        if (숨김작업 != null)
            StopCoroutine(숨김작업);

        숨김작업 = StartCoroutine(잠시후숨기기());
    }

    private IEnumerator 잠시후숨기기()
    {
        yield return new WaitForSecondsRealtime(표시시간);
        즉시숨기기();
        숨김작업 = null;
    }

    private void 즉시숨기기()
    {
        if (캔버스그룹 != null)
        {
            캔버스그룹.alpha = 0f;
            캔버스그룹.blocksRaycasts = false;
        }
    }
}
