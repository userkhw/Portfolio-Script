using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BallCrashStop : MonoBehaviour
{
    Coroutine myCoroutine;
    Rigidbody2D rb; // 리지드바디
    Obstacle Obstacle;
    PointerRotator pointerRotator; // 스크립트

    [Header("게임 배속 분기")]
    public float[] branches = new float[13];
    [Header("게임 시간 증가량")]
    public float[] increasedGameTime = new float[13]; // 게임 시간 증가량
    [Header("장애물 수명 시간 감소량")]
    public float reduceTime = 2f; // 장애물의 수명 시간 감소   
    [Header("공 반사 주기")]
    public float reflectionCycle = 2f; // 반사 주기
    [Space(10)]
    public Vector2 reflection; // 반사각 벡터
   
    Vector2 lastVelocity; // 마지막 속도

    static bool joystickActivate = true; // 정적 변수

    public static void SetJoystickActivate(bool tf)
    {
        joystickActivate = tf;
    }

    public static bool GetJoystickActivate()
    {
        return joystickActivate;
    }

    private void Awake()
    {
        pointerRotator = FindAnyObjectByType<PointerRotator>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        joystickActivate = true;
    }

    private void Update()
    {
        lastVelocity = rb.linearVelocity;
    }

    // 콜라이더와 충돌했을 때 실행되는 함수
    private void OnCollisionEnter2D(Collision2D collision)
    {
        joystickActivate = false;

        if (collision.gameObject.CompareTag("Obstacle"))
        {
            Obstacle = collision.gameObject.GetComponent<Obstacle>();
            Obstacle.ReduceLifetime(reduceTime);
        }

        float speed = lastVelocity.magnitude; // 벡터의 길이 또는 크기
        // Reflect() : 지정된 법선을 사용하여 표면을 벗어난 벡터의 반사를 계산
        reflection = Vector2.Reflect(lastVelocity.normalized, collision.contacts[0].normal); // dir : 방향 벡터값

        // 방향을 각도로 변환
        float reflectionAngle = Mathf.Atan2(reflection.y, reflection.x) * Mathf.Rad2Deg;
        // 화살표가 반사각 방향으로 회전(pointerBase의 Z축만 회전하고 -90도 보정)
        pointerRotator.pointerBase.rotation = Quaternion.Euler(0f, 0f, reflectionAngle - 90f);

        myCoroutine = StartCoroutine("Delay");  // 코루틴을 실행하고 변수에 저장    
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        joystickActivate = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Goal") && collision.gameObject.GetComponentInChildren<SpriteRenderer>().enabled == true)
        {
            collision.gameObject.GetComponentInChildren<SpriteRenderer>().enabled = false;
            collision.gameObject.GetComponentInChildren<Image>().enabled = false;

            ParticleSystem particleSystem = collision.gameObject.GetComponentInChildren<ParticleSystem>();
            particleSystem.transform.SetParent(null);
            particleSystem.Play();
            Destroy(collision.gameObject);
            Destroy(particleSystem.gameObject, particleSystem.main.duration + particleSystem.main.startLifetime.constant);

            GameManager.Instance.IncreaseTime(increasedGameTime[GameManager.Instance.count]);
        }
    }

    // 시간 지연용 코루틴
    public IEnumerator Delay()
    {
        yield return new WaitForSecondsRealtime(reflectionCycle);

        pointerRotator.FireBall(reflection);
    }
}
