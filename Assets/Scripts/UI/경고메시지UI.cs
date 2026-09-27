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
    private RectTransform 패널;
    private Vector2 기본앵커최소;
    private Vector2 기본앵커최대;
    private Vector2 기본위치;
    private Vector2 기본크기;
    private float 기본글자크기;

    private void Awake()
    {
        패널 = GetComponent<RectTransform>();
        if (패널 != null)
        {
            기본앵커최소 = 패널.anchorMin;
            기본앵커최대 = 패널.anchorMax;
            기본위치 = 패널.anchoredPosition;
            기본크기 = 패널.sizeDelta;
        }
        if (경고문구 != null)
            기본글자크기 = 경고문구.fontSize;
        즉시숨기기();
    }

    // 기존 경고를 교체하고 정해진 시간 동안 새 문구를 보여 준다.
    public void 표시(string 내용)
    {
        기본크기로되돌리기();
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

    public void 크게표시(string 내용)
    {
        표시(내용);
        if (패널 != null)
        {
            패널.anchorMin = new Vector2(0.5f, 0.5f);
            패널.anchorMax = new Vector2(0.5f, 0.5f);
            패널.anchoredPosition = Vector2.zero;
            패널.sizeDelta = new Vector2(900f, 200f);
        }
        if (경고문구 != null)
            경고문구.fontSize = 80f;
    }

    private void 기본크기로되돌리기()
    {
        if (패널 != null)
        {
            패널.anchorMin = 기본앵커최소;
            패널.anchorMax = 기본앵커최대;
            패널.anchoredPosition = 기본위치;
            패널.sizeDelta = 기본크기;
        }
        if (경고문구 != null)
            경고문구.fontSize = 기본글자크기;
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
