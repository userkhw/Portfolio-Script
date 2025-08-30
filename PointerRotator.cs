using UnityEngine;

public class PointerRotator : MonoBehaviour
{
    public Rigidbody2D ballRigidbody;   // 공의 Rigidbody2D
    public Transform pointerBase;   // 공 중심에 위치한 빈 오브젝트
    public Transform pointer;       // 직사각형 오브젝트 (자식)
    public Transform sensor; // 센서(원형 콜라이더가 있는 빈 오브젝트)
    public Transform ball;

    BallCrashStop ballCrashStop;
    CircleCollider2D sensorCollider;
    SpriteRenderer pointerRenderer; // 직사각형의 SpriteRenderer
    SpriteRenderer sensorRenderer; // 센서의 SpriteRenderer

    [Header("공 속도")]
    public float forcePower = 5f;  // 발사되는 힘의 크기
    [Header("현재 공의 속도")]
    public float speed;
    [Space(10)]
    public static bool isGameStart = false; // 게임 시작했는지

    public bool obstacleFirstCreation = false;
    public bool goalFirstCreation = false;

    float pointerHeight;    // 직사각형의 세로 길이
    float sensorRadius;     // 센서 반지름
    bool isHolding = false; // 마우스를 누르고 있는 중인지
    bool isFired = false; // 발사를 했는지

    Vector2 mouseDirection;
    Vector2 lastDirection;

    private void Awake()
    {
        ballCrashStop = FindAnyObjectByType<BallCrashStop>();

        sensorRenderer = sensor.GetComponent<SpriteRenderer>();
        pointerRenderer = pointer.GetComponent<SpriteRenderer>(); // 직사각형의 SpriteRenderer 및 세로 길이 설정  

        // 센서의 반지름 계산 (CircleCollider2D에서 가져옴)
        sensorCollider = sensor.GetComponent<CircleCollider2D>();
    }

    private void Start()
    {
        isGameStart = false;

        sensorRadius = sensorCollider.radius * sensor.localScale.x; // 스케일 보정 포함
        pointerHeight = pointerRenderer.bounds.size.y;

        // 처음에는 안 보이도록 비활성화
        pointerRenderer.enabled = false;
        sensorRenderer.enabled = false;

        Time.timeScale = 0f;
        GameManager.Instance.enabled = false;
    }

    private void Update()
    {
        if (ballRigidbody.linearVelocity != Vector2.zero)
        {
            lastDirection = ballRigidbody.linearVelocity.normalized;
        }
        else
        {
            ballRigidbody.linearVelocity = lastDirection * forcePower;
        }

        ballRigidbody.linearVelocity = ballRigidbody.linearVelocity.normalized * forcePower;

        speed = ballRigidbody.linearVelocity.magnitude;

        // 센서의 위치를 공과 동일하게 유지
        sensor.position = ball.position;

        // 마우스의 월드 좌표를 구함 (Z는 0으로 고정)
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;

        // 마우스와 센서 사이의 거리 저장
        float distance = Vector2.Distance(mousePos, sensor.position);

        // Pointer를 위쪽으로 세로길이의 절반만큼 올려서 밑변을 중심점에 고정
        pointer.localPosition = new Vector3(0f, pointerHeight / 2f, 0f);

        // 마우스 방향 벡터
        mouseDirection = mousePos - pointerBase.position;

        // 방향을 각도로 변환하고 pointerBase의 Z축만 회전하고 -90도 보정
        float mouseAngle = Mathf.Atan2(mouseDirection.y, mouseDirection.x) * Mathf.Rad2Deg;

        // 1. 마우스 버튼을 클릭하고 있는 경우
        if (Input.GetMouseButton(0) && !isFired)
        {
            // 1-1. 센서 범위 안인 경우
            if (distance < sensorRadius)
            {
                isHolding = true;
                Activate(); // 센서와 화살표 렌더러 활성화

                if (!isGameStart)
                {
                    // 화살표가 마우스를 바라보게 회전
                    pointerBase.rotation = Quaternion.Euler(0f, 0f, mouseAngle - 90f);
                    // 이 상황에서는 방향을 BallCrashStop에서 받아서 적용시켜야 함
                    ballCrashStop.reflection = mouseDirection;
                }
            }
            // 1-2. 센서 범위 바깥에 있는 경우
            else if (isHolding)
            {
                isHolding = false;

                if (!isFired && !isGameStart)
                {
                    GameManager.Instance.enabled = true;
                    FireBall(mouseDirection);
                    isGameStart = true;
                    obstacleFirstCreation = true;
                    goalFirstCreation = true;
                }
            }
        }
        // 2. 마우스 버튼을 클릭하지 않고 있는 경우
        else
        {
            isHolding = false;
            Reset();
        }
    }

    // 발사 함수
    public void FireBall(Vector2 direction)
    {
        isFired = true;
        Time.timeScale = GameManager.Instance.timeScaleSpeed;
        ballRigidbody.linearVelocity = Vector2.zero; // 기존 속도 제거
        ballRigidbody.AddForce(direction.normalized, ForceMode2D.Impulse);
        pointerRenderer.enabled = false;
        sensorRenderer.enabled = false;
    }

    void Activate()
    {
        // 화살표와 센서 렌더러 활성화
        pointerRenderer.enabled = true;
        sensorRenderer.enabled = true;
    }

    public void Reset()
    {
        // 센서와 화살표 렌더러 비활성화
        pointerRenderer.enabled = false;
        sensorRenderer.enabled = false;
    }
}