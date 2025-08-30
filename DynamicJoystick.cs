using UnityEngine;
using UnityEngine.EventSystems;


public class DynamicJoystick : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public RectTransform Base;         // 배경
    public RectTransform Handle;       // 조작 핸들
    public Rigidbody2D ballRigidbody; // Ball의 Rigidbody2D
    public Vector2 direction; // 핸들 방향

    CanvasGroup panelCanvasGroup;

    [Header("관성")]
    public float inertiaFactor = 3f;    // 관성 강도 설정 // 값이 클수록 새로운 목표 방향으로 천천히 부드럽게 보간하여 꺾임
    [Space(10)]
    public float dTheta = 0f;           // 회전 입력을 읽어 목표 회전 각도(dlelta) 결정
    public float maxRange = 100f;       // 최대 조작 거리 (픽셀)
    public float minDragDistance = 20f; // 조이스틱이 활성화되는 최소 드래그 거리 (픽셀) 

    bool isActive = false; // 활성화 여부
      
    Vector2 startPosition; // 최초 터치 위치

    private void Awake()
    {
        panelCanvasGroup = GetComponentInChildren<CanvasGroup>();
    }

    private void Start()
    {
        // 시작 시 조이스틱 비활성화
        Base.gameObject.SetActive(false);
        Handle.gameObject.SetActive(false);
    }

    private void Update()
    {
        // joystickActivate 가 false 이고 isGameStart 가 true인 경우
        if (BallCrashStop.GetJoystickActivate() == false) 
        {
            panelCanvasGroup.blocksRaycasts = false;
            Base.gameObject.SetActive(false);
            Handle.gameObject.SetActive(false);
            isActive = false;
        }
        else // true 인 경우
        {
            panelCanvasGroup.blocksRaycasts = true;
        }

        if (isActive)
        {
            Rotate();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // 터치 시작 위치 저장
        startPosition = eventData.position;
        transform.position = startPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 현재 터치 위치
        Vector2 currentPosition = eventData.position;
        // 드래그 거리를 계산
        float dragDistance = Vector2.Distance(startPosition, currentPosition);

        // 최소 거리 이상 드래그 시 조이스틱 활성화
        if (!isActive && dragDistance >= minDragDistance && PointerRotator.isGameStart)
        {
            Base.gameObject.SetActive(true);
            Handle.gameObject.SetActive(true);
            isActive = true;
        }

        // 조이스틱이 활성화된 상태라면 핸들을 따라 움직이도록 처리
        if (isActive)
        {
            // 1) 핸들 이동 // 드래그 방향 벡터 계산            
            direction = currentPosition - startPosition;
            // 최대 범위 이상으로 벗어나지 않도록 제한
            Vector2 clampedDirection = Vector2.ClampMagnitude(direction, maxRange);
            // 핸들 위치를 조정
            Handle.anchoredPosition = clampedDirection;     
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // 조이스틱 비활성화
        Base.gameObject.SetActive(false);
        Handle.gameObject.SetActive(false);

        // 상태 초기화
        isActive = false;
    }

    private void Rotate()
    {
        // 공의 현재 진행 방향
        Vector2 currentDir = ballRigidbody.linearVelocity.normalized;
        // 핸들의 방향
        Vector2 targetDir = direction.normalized;

        // 두 방향 사이의 각도 계산
        float angle = Vector2.Angle(currentDir, targetDir);

        // 만약 각도가 매우 작다면 (거의 같음), 회전하지 않고 그대로 직진
        if (angle < 1f)
        {
            // 목표 방향으로 이미 회전하고 있으므로 현재 속도 유지
            ballRigidbody.linearVelocity = targetDir * ballRigidbody.linearVelocity.magnitude;
        }

        // 현재 방향에서 목표 방향으로 관성 보간 (자연스러운 회전)
        Vector2 newDir = Vector3.Slerp(currentDir, targetDir, inertiaFactor * Time.deltaTime).normalized;

        // 속도 유지
        ballRigidbody.linearVelocity = newDir * ballRigidbody.linearVelocity.magnitude;
    }
}
