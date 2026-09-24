using System.Collections;
using UnityEngine;

public class 불만 : MonoBehaviour
{
    SpriteRenderer spriteRenderer;
    Coroutine hideRoutine;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    private void Start()
    {
        spriteRenderer.enabled = false;
    }
    public void 으아악(float sec=1f)
    {
        spriteRenderer.enabled = true;

        if (hideRoutine != null)
            StopCoroutine(hideRoutine);

        hideRoutine = StartCoroutine(좀있다숨겨(sec));
    }

    // 정해진 시간이 지나면 불만 표시를 다시 숨긴다.
    private IEnumerator 좀있다숨겨(float sec=1f)
    {
        yield return new WaitForSeconds(sec);
        spriteRenderer.enabled = false;
        hideRoutine = null;
    }
}
