using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem.Controls;

public class Obstacle : MonoBehaviour
{
    GameObject uiInstance; // 인스턴스화된 UI
    ObstacleManager obstacleManager;
    AnimationCurve scaleInCurve;
    AnimationCurve scaleOutCurve;
    ParticleSystem particleSystem;
    Material materialPrefab;
    SpriteRenderer spriteRenderer;

    public float lifeTime; // 수명 시간 
    public float duration = 0.5f; // 지속 시간

    public float interval = 3f; // 회전 주기
    public float rotateTime = 1.5f; // 회전 시간 
    public float rotateAngle = 540f; // 회전값

    float maxLifeTime;

    int count = 0;
    bool isRotateStart = false;
    bool isParticleStart = false;
    Vector3 originalScale; // 원래 스케일

    private void Awake()
    {
        obstacleManager = FindAnyObjectByType<ObstacleManager>();
        particleSystem = GetComponentInChildren<ParticleSystem>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        if (gameObject.CompareTag("Obstacle"))
        {
            lifeTime = obstacleManager.obstacle_extTime;
        }
        else if (gameObject.CompareTag("Goal"))
        {
            lifeTime = obstacleManager.goal_extTime;
            transform.GetChild(1).GetComponent<ParticleSystem>().Play();
        }

        maxLifeTime = lifeTime;

        originalScale = transform.GetChild(0).localScale;

        scaleInCurve = new AnimationCurve();
        scaleInCurve.AddKey(0f, 0f);
        scaleInCurve.AddKey(1f, 1f);

        scaleOutCurve = new AnimationCurve();
        scaleOutCurve.AddKey(0f, 1f);
        scaleOutCurve.AddKey(1f, 0f);

        float scaleFactor = transform.localScale.x;
        var main = particleSystem.main;
        main.startSpeed = 4 * (10 * scaleFactor);

        particleSystem.Play();

        StartCoroutine(ScaleIn());

        //float degree = transform.eulerAngles.z;
        //spriteRenderer.material.SetFloat("_Rotation", degree);
    }

    private void Update()
    {
        // 남은 시간 계속 감소
        lifeTime -= Time.unscaledDeltaTime;

        if (lifeTime <= 0)
        {
            if (!isParticleStart)
            {
                particleSystem.Play();
                isParticleStart = true;
            }

            StartCoroutine(ScaleOut());
        }

        if (gameObject.CompareTag("Goal") && gameObject.GetComponentInChildren<SpriteRenderer>().enabled && !isRotateStart)
        {
            isRotateStart = true;
            StartCoroutine(RotateRoutine());
        }

        // 생명 게이지
        LifeGauge();
    }

    // 외부에서 lifeTime을 줄이고 싶을 때 호출
    public void ReduceLifetime(float amount)
    {
        lifeTime -= amount;
    }

    // 파괴됐을 때
    private void OnDestroy()
    {
        // 장해물 제거되면 UI도 제거
        if (uiInstance != null)
        {
            Destroy(uiInstance);
        }

        // Manager에게 알림
        obstacleManager.RemoveFromDict(gameObject);
    }

    // 스케일 인
    IEnumerator ScaleIn()
    {
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(timer / duration);
            float curveValue = scaleInCurve.Evaluate(t);
            transform.GetChild(0).localScale = originalScale * curveValue;

            yield return null;
        }
    }

    // 스케일 아웃
    IEnumerator ScaleOut()
    {
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(timer / duration);
            float curveValue = scaleOutCurve.Evaluate(t);
            transform.GetChild(0).localScale = originalScale * curveValue;

            yield return null;
        }

        Destroy(gameObject);
    }

    // 회전 주기
    IEnumerator RotateRoutine()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(interval); // 회전 주기 시간만큼 대기

            yield return StartCoroutine(RotateOverTime()); // 회전
        }
    }

    // 회전 관리
    IEnumerator RotateOverTime()
    {
        float elapsed = 0f;
        float lastAngle = 0f;
        float currentAngle = 0f;

        while (elapsed < rotateTime)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / rotateTime);

            // 현재 목표 회전량 계산
            currentAngle = rotateAngle * t;
            // 이번 프레임에서 회전해야 할 각도 차이
            float deltaAngle = currentAngle - lastAngle;
            // 누적 회전값만큼 회전
            transform.Rotate(0f, 0f, deltaAngle);
            // 현재 상태 저장
            lastAngle = currentAngle;

            yield return null;
        }

        // 최종 정렬 : 혹시라도 남은 각도 차이 마무리
        transform.Rotate(0f, 0f, rotateAngle - lastAngle);
    }

    // 생명 게이지
    void LifeGauge()
    {
        float progress = Mathf.Clamp01(lifeTime / maxLifeTime);
        spriteRenderer.material.SetFloat("_FillAmount", progress);
    }
}
