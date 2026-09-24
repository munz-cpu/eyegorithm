using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class 마우스위에있는동안톱니돌아가기 : MonoBehaviour
{
    [SerializeField] private float rotateSpeed = 360f;
    bool turn = false;
    private void OnMouseEnter()
    {
        turn = true;
        StartCoroutine(rotate());
    }
    private void OnMouseExit()
    {
        turn = false;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    IEnumerator rotate()
    {
        while (turn)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                //설정창 열기
            }
            transform.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);
            yield return null;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
