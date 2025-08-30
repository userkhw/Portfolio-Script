using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;


public class ObstacleManager : MonoBehaviour
{
    public GameObject[] obstacles;
    public GameObject obsPreview;
    public GameObject goalPreview;
    public GameObject container;
    public Transform ball;
    public Transform runtimeParent;
    public BoxCollider2D fieldBoxCollider2D;

    public Sprite[] sprite;

    GameObject obstacle;

    GameObject obsInstance;
    GameObject goalInstance;

    GameObject obsPreviewInstance;
    GameObject goalPreviewInstance;

    PointerRotator pointerRotator;
    Bounds bounds;

    Dictionary<GameObject, Collider2D> obstacleDict = new Dictionary<GameObject, Collider2D>();

    [Header("장해물 생성 주기")]
    public float ObstacleCycle = 3f; // 장해물 생성 주기
    [Header("오브젝트 최대 갯수")]
    public int obstacle_maxNumber = 10; // 장해물 최대 갯수
    public int goal_maxNumber = 10; // 골 최대 갯수 
    [Header("장해물 관리")]
    public float Obstacle_sizeMin = 0.75f;
    public float Obstacle_sizeMax = 1.25f;
    public float Obstacle_extTimeMin = 2.5f;
    public float Obstacle_extTimeMax = 5f;
    [Header("골 관리")]
    public float Goal_sizeMin = 0.75f;
    public float Goal_sizeMax = 1.25f;
    public float Goal_extTimeMin = 2.5f;
    public float Goal_extTimeMax = 5f;
    [Header("장해물 첫 생성 간격")]
    public float obstacleFirstCreationInterval;
    [Header("골 첫 생성 간격")]
    public float goalFirstCreationInterval;
    [Space(10)]
    public float obstacle_extTime; // 장해물 수명
    public float goal_extTime; // 골 수명
    float obstacleTimer; // 장해물 생성주기 타이머
    int currentObstacleCount;
    int currentGoalCount;

    float ballRadius;
    float ballDiameter;
    CircleCollider2D ballCollider;
    float x, y;

    bool obsPreviewFirstCre = false;
    bool isCreated = false;

    private void Awake()
    {
        pointerRotator = FindAnyObjectByType<PointerRotator>();
        // 공 콜라이더 가져오기
        ballCollider = ball.GetComponent<CircleCollider2D>();
    }

    private void Start()
    {
        obstacle = obstacles[Random.Range(1, obstacles.Length)];

        // field 콜라이더의 바운드 정보 얻기
        bounds = fieldBoxCollider2D.bounds;
        // 공의 반지름
        ballRadius = ballCollider.bounds.extents.x;
        ballDiameter = ballRadius * 2;

        // 런타임 오브젝트 정리용 부모 객체 생성
        container = new GameObject("RuntimeObjects");
        runtimeParent = container.transform;
    }

    // 위치, 회전, 사이즈
    private void Update()
    {
        if (PointerRotator.isGameStart)
        {
            // 생성주기 타이머
            obstacleTimer += Time.deltaTime; // 장해물
        }

        // 딕셔너리 순회하며 현재 개수 카운트
        currentObstacleCount = 0;
        currentGoalCount = 0;
        foreach (var obj in obstacleDict.Keys)
        {
            if (obj.CompareTag("Obstacle"))
            {
                currentObstacleCount++;
            }
            else
            {
                currentGoalCount++;
            }
        }

        //장해물 셋업=========================================================================================================================

        // 장해물 랜덤 선택
        if (isCreated)
        {
            obstacle = obstacles[Random.Range(1, obstacles.Length)]; // 0 : Goal, 1 : Circle, 2 : Square, 3 : Triangle
            isCreated = false;
        }

        // 장해물 스케일&회전 랜덤
        float randomObstacleScale = Random.Range(Obstacle_sizeMin, Obstacle_sizeMax);
        float rotation = Random.Range(0f, 360f);

        // 임시 인스턴스 생성 => 스케일&회전 적용 => 반지름 구하기 => 삭제
        GameObject obstacleTemp = Instantiate(obstacle, new Vector3(0, 0, -100f), Quaternion.Euler(0, 0, rotation));
        obstacleTemp.transform.localScale *= randomObstacleScale;
        float realObstacleRadius = obstacleTemp.GetComponent<Collider2D>().bounds.extents.magnitude;
        Destroy(obstacleTemp);

        //골 셋업==============================================================================================================================

        // 골 선택
        GameObject goal = obstacles[0];

        // 골의 스케일 랜덤
        float randomGoalScale = Random.Range(Goal_sizeMin, Goal_sizeMax);

        // 임시 임스턴스 생성
        GameObject goal_Temp = Instantiate(goal, new Vector3(0, 0, -100f), Quaternion.identity);
        goal_Temp.transform.localScale *= randomGoalScale;
        // 골의 반지름
        float realGoalRadius = goal_Temp.GetComponent<Collider2D>().bounds.extents.magnitude / 2;
        Destroy(goal_Temp);

        //장해물&골 스폰 위치 설정=================================================================================================================

        // 장해물 스폰 후보 위치 랜덤
        x = Random.Range(bounds.min.x + realObstacleRadius + ballDiameter, bounds.max.x - realObstacleRadius - ballDiameter);
        y = Random.Range(bounds.min.y + realObstacleRadius + ballDiameter, bounds.max.y - realObstacleRadius - ballDiameter);
        Vector3 obstacleSpawnPos = new Vector2(x, y);

        // 골 스폰 후보 위치 랜덤
        x = Random.Range(bounds.min.x + realGoalRadius, bounds.max.x - realGoalRadius);
        y = Random.Range(bounds.min.y + realGoalRadius, bounds.max.y - realGoalRadius);
        Vector3 goalSpawnPos = new Vector2(x, y);

        // 스폰 유효 플래그 초기화
        bool obstacleValid = true;
        bool goalValid = true;

        //장해물 스폰 위치 유효한지 검사===========================================================================================================

        //인스턴스 생성
        GameObject obsTemp = Instantiate(obstacle, obstacleSpawnPos, Quaternion.Euler(0, 0, rotation));
        SpriteRenderer obstacleSpr = obsTemp.transform.GetChild(0).GetComponent<SpriteRenderer>();
        obstacleSpr.enabled = false;
        // 스케일 적용
        obsTemp.transform.localScale *= randomObstacleScale;
        Physics2D.SyncTransforms();
        // 새로 생성될 장해물의 Collider2D 가져오기
        Collider2D obstacleCollider = obsTemp.GetComponent<Collider2D>();

        // 생성될 장해물과 존재하는 공과의 거리 계산
        ColliderDistance2D distWithBall = Physics2D.Distance(obstacleCollider, ballCollider); ;
        if (pointerRotator.obstacleFirstCreation && distWithBall.distance < obstacleFirstCreationInterval)
        {
            obstacleValid = false;
        }

        // 생성된 모든 장해물과의 충돌 검사
        foreach (var kvp in obstacleDict)
        {
            Collider2D existingObsCollider = kvp.Value;

            // 생성될 장해물과 존재하는 오브젝트들 사이 거리 계산
            ColliderDistance2D distObs = Physics2D.Distance(obstacleCollider, existingObsCollider);

            // 계산이 제대로 되었는지 여부
            if (!distObs.isValid)
            {
                obstacleValid = false;
                break;
            }
            // 생성될 장해물과 존재하는 장해물 사이 거리 계산
            if (existingObsCollider.gameObject.CompareTag("Obstacle"))
            {
                if (distObs.distance < ballRadius * 2)
                {
                    obstacleValid = false;
                    break;
                }
            }
            // 생성될 장해물과 존재하는 골 사이 거리 계산
            else
            {
                if (distObs.distance < 0)
                {
                    obstacleValid = false;
                    break;
                }
            }

            if (!distWithBall.isValid || distWithBall.distance < 0)
            {
                obstacleValid = false;
            }
        }

        Destroy(obsTemp);

        //장해물 생성=============================================================================================================================

        //오브젝트 간 거리 유효하고
        if (obstacleValid && PointerRotator.isGameStart)
        {   //(장해물의 최대갯수보다 작고 and ObsPreview 가 null 이면) or obstaclefirstCreation이 true인 경우
            if ((currentObstacleCount < obstacle_maxNumber && obstacleTimer > ObstacleCycle)
                || pointerRotator.obstacleFirstCreation || !obsPreviewFirstCre)
            {
                // 장해물 소멸 시간 랜덤
                obstacle_extTime = Random.Range(Obstacle_extTimeMin, Obstacle_extTimeMax);
                // 장해물 생성(종류 랜덤, 위치 랜덤)
                obsInstance = Instantiate(obstacle, obstacleSpawnPos + new Vector3(0, 0, -0.1f), Quaternion.Euler(0, 0, rotation), runtimeParent);
                // 랜덤 스케일 적용
                obsInstance.transform.localScale *= randomObstacleScale;
                // 딕셔너리에 "실제 생성된 콜라이더" 등록
                Collider2D actualCol = obsInstance.GetComponent<Collider2D>();
                obstacleDict.Add(obsInstance, actualCol);

                if (!pointerRotator.obstacleFirstCreation)
                {
                    obsPreviewInstance = Instantiate(obsPreview, obstacleSpawnPos, Quaternion.Euler(0, 0, rotation), runtimeParent);
                    obsPreviewInstance.transform.localScale *= randomObstacleScale;
                    PreviewSprite();
                    obsInstance.transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
                    obsInstance.GetComponentInChildren<Image>().enabled = false;
                    obsInstance.GetComponent<Obstacle>().enabled = false;
                    obsInstance.GetComponent<Collider2D>().isTrigger = true;
                    obsInstance.transform.GetChild(0).GetComponent<Collider2D>().isTrigger = true;

                    StartCoroutine(ObsPreview(obsInstance, obsPreviewInstance));

                    obsPreviewFirstCre = true;
                }

                obstacleTimer = 0;
                pointerRotator.obstacleFirstCreation = false;
                isCreated = true;
            }
        }

        // 골 스폰 위치 유효한지 검사====================================================================================================================

        // 임시 인스턴스 생성
        GameObject goalTemp = Instantiate(goal, goalSpawnPos + new Vector3(0, 0, -100f), Quaternion.identity);
        SpriteRenderer goalSpr = goalTemp.transform.GetChild(0).GetComponent<SpriteRenderer>();
        goalSpr.enabled = false;
        // 스케일 적용
        goalTemp.transform.localScale *= randomGoalScale;

        // 새로 생성될 골의 Collider2D 가져오기
        Collider2D goalCollider = goalTemp.GetComponent<Collider2D>();
        goalCollider.isTrigger = false;

        ColliderDistance2D distGoalWithBall = Physics2D.Distance(goalCollider, ballCollider);
        if (pointerRotator.goalFirstCreation && distGoalWithBall.distance < goalFirstCreationInterval)
        {
            goalValid = false;
        }

        // 생성된 모든 오브젝트와의 충돌 검사
        foreach (var kvp in obstacleDict)
        {
            Collider2D existingCollider = kvp.Value;

            // 골과 존재하는 오브젝트 사이 거리 계산
            ColliderDistance2D distGoal = Physics2D.Distance(goalCollider, existingCollider);

            if (!distGoal.isValid || distGoal.distance < 0)
            {
                goalValid = false;
                break;
            }

            // 공과 존재하는 오브젝트 사이 거리 계산
            ColliderDistance2D distGoalWithObjcet = Physics2D.Distance(ballCollider, existingCollider);

            if (!distGoalWithObjcet.isValid || distGoalWithObjcet.distance < 0)
            {
                goalValid = false;
                break;
            }
        }

        Destroy(goalTemp);

        //골 생성=============================================================================================================================

        //  오브젝트 간 거리 유효하면
        if (goalValid && PointerRotator.isGameStart)
        {   // (isGameStart가 true 이고 and 골 프리뷰가 null 이면) or goalfirstCreation이 true이면
            if (GameObject.FindWithTag("GoalPreview") == null || pointerRotator.goalFirstCreation)
            {
                // 골 소멸 시간 랜덤
                goal_extTime = Random.Range(Goal_extTimeMin, Goal_extTimeMax);
                // 골 생성 (위치 랜덤)
                goalInstance = Instantiate(goal, goalSpawnPos + new Vector3(0, 0, -0.1f), Quaternion.identity, runtimeParent);
                // 골 인스턴스의 사이즈 랜덤
                goalInstance.transform.localScale *= randomGoalScale;
                // 딕셔너리에 "골의 콜라이더" 등록
                Collider2D actualGoalCol = goalInstance.GetComponent<Collider2D>();
                obstacleDict.Add(goalInstance, actualGoalCol);

                if (!pointerRotator.goalFirstCreation)
                {
                    goalPreviewInstance = Instantiate(goalPreview, goalSpawnPos, Quaternion.identity, runtimeParent);
                    goalPreviewInstance.transform.localScale *= randomGoalScale;
                    goalInstance.transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
                    goalInstance.GetComponentInChildren<Image>().enabled = false;
                    goalInstance.GetComponent<Obstacle>().enabled = false;
                }

                pointerRotator.goalFirstCreation = false;
            }
        }
    }

    public void RemoveFromDict(GameObject obj)
    {
        if (obstacleDict.ContainsKey(obj))
        {
            obstacleDict.Remove(obj);

            if (obj.CompareTag("Goal"))
            {
                goalInstance.transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = true;
                goalInstance.GetComponentInChildren<Image>().enabled = true;
                goalInstance.GetComponent<Obstacle>().enabled = true;
                Destroy(goalPreviewInstance);
            }
        }
    }

    IEnumerator ObsPreview(GameObject obsInstance, GameObject ObsPreviewInstance)
    {
        yield return new WaitForSeconds(ObstacleCycle);

        Destroy(ObsPreviewInstance);

        obsInstance.transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = true;
        obsInstance.GetComponentInChildren<Image>().enabled = true;
        obsInstance.transform.GetChild(0).GetComponent<Collider2D>().isTrigger = false;
        obsInstance.GetComponent<Obstacle>().enabled = true;
    }

    void PreviewSprite()
    {
        // 오브젝트가 Circle 이면
        if (obstacle == obstacles[1])
        {
            obsPreviewInstance.GetComponent<SpriteRenderer>().sprite = sprite[0];
        }

        // 오브젝트가 Square 이면
        if (obstacle == obstacles[2])
        {
            obsPreviewInstance.GetComponent<SpriteRenderer>().sprite = sprite[1];
        }

        // 오브젝트가 Triangle 이면
        if (obstacle == obstacles[3])
        {
            obsPreviewInstance.GetComponent<SpriteRenderer>().sprite = sprite[2];
        }
    }
}



