using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class 불만 : MonoBehaviour
{
    SpriteRenderer spriteRenderer;
    Coroutine hideRoutine;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        SortingGroup 표시그룹 = GetComponent<SortingGroup>();
        if (표시그룹 == null)
            표시그룹 = gameObject.AddComponent<SortingGroup>();
        표시그룹.sortAtRoot = true;
        표시그룹.sortingOrder = 30;
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

    public void 숨기기()
    {
        if (hideRoutine != null)
        {
            StopCoroutine(hideRoutine);
            hideRoutine = null;
        }

        spriteRenderer.enabled = false;
    }

    // 정해진 시간이 지나면 불만 표시를 다시 숨긴다.
    private IEnumerator 좀있다숨겨(float sec=1f)
    {
        yield return new WaitForSeconds(sec);
        spriteRenderer.enabled = false;
        hideRoutine = null;
    }
}
