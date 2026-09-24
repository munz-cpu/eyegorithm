using UnityEngine;
[RequireComponent(typeof(AudioSource))]
public class 배고파 : MonoBehaviour
{
    [SerializeField] private float 불만지속시간 = 1f;
    [SerializeField] private AudioClip 불만효과음;
    [SerializeField] private float 처음쿨 = 5f;
    [SerializeField] private Vector2 쿨 = new Vector2(5f,10f);

    불만 밥달라는불만;

    AudioSource audioSource;
    float timer;
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        밥달라는불만 = GetComponentInChildren<불만>();
    }
    void Start()
    {
        timer = 처음쿨; 
    }

    // Update is called once per frame
    void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
            return;
        }
        밥갖고와();
        timer = Random.Range(쿨.x, 쿨.y);
    }
    void 밥갖고와()
    {
        Debug.Log(gameObject + "배고파!!!");
        밥달라는불만.으아악(불만지속시간);
        if (불만효과음) audioSource.PlayOneShot(불만효과음);
    }
}
