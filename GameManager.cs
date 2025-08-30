using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public Slider progressBar; // 프로그래스 바 UI 오브젝트
    public GameObject gameOverPanel; // 게임오버 UI
    public Text highscoreText;
    public Text currentscoreText;
    public TextMeshProUGUI tmpText;

    PointerRotator pointerRotator;
    ObstacleManager obstacleManager;

    [Header("총 게임 시간")]
    public float totalTime = 10f; // 총 게임 시간
    [Header("게임 배속 더하기 수치")]
    public float timeSpeed = 0.25f; // 게임 배속 더하기 수치
    [Header("게임 배속 주기")]
    public float cycle = 5f; // 배속 주기
    [Header("현재 게임의 배속")]
    public float timeScaleSpeed = 1f; // 게임의 배속
    [Header("현재 게임이 흐른 시간")]
    [SerializeField] float realTime = 0f; // 현재 게임이 흐른 시간
    [Header("현재 남은 게임 시간")]
    public float currentTime = 0f; // 현재 남은 게임 시간
    [Header("게임 배속 더한 횟수")]
    public int count = 0; // (0부터 4까지 12번 더하고 갯수는 13개)

    float gameTime = 0f; // 게임 시간
    bool isGameOver = false;
    int currentScore = 0;
    int highScore = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

        }
        else if (Instance != this)
        {
            // 이미 다른 인스턴스가 존재하면 자신을 파괴
            Destroy(gameObject);
        }

        pointerRotator = FindAnyObjectByType<PointerRotator>();
        obstacleManager = FindAnyObjectByType<ObstacleManager>();
    }

    private void Start()
    {
        highScore = PlayerPrefs.GetInt("highScore");

        // 현재 시간에 게임 총 시간 넣기
        currentTime = totalTime;
        isGameOver = false;
    }

    private void Update()
    {
        realTime += Time.unscaledDeltaTime; // 현재 게임이 흐른 시간
        gameTime += Time.unscaledDeltaTime;

        if (gameTime >= cycle && Time.timeScale != 0 && timeScaleSpeed < 4)
        {
            Time.timeScale += timeSpeed; // 합연산
            timeScaleSpeed = Time.timeScale;
            tmpText.text = "x" + timeScaleSpeed;

            gameTime = 0f;

            if (count < 12)
            {
                count++;
            }
        }

        // 현재 시간 실시간으로 줄어듬
        currentTime -= Time.unscaledDeltaTime;

        if (currentTime > totalTime)
        {
            totalTime = currentTime;
        }

        // 슬라이더의 value(0,1)를 (현재시간,총시간)의 비율로 나타냄
        progressBar.value = currentTime / totalTime;

        // 게임 오버
        if (currentTime <= 0 && !isGameOver)
        {
            currentScore = (int)realTime;
            if(currentScore > highScore)
            {
                highScore = currentScore;               
            }
            PlayerPrefs.SetInt("highScore", highScore);
            highscoreText.text = highScore.ToString();
            currentscoreText.text = currentScore.ToString();

            Time.timeScale = 0f;
            gameOverPanel.SetActive(true);
            pointerRotator.Reset();
            BallCrashStop.SetJoystickActivate(false);
            isGameOver = true;

            obstacleManager.container.SetActive(false);
        }
    }

    public void Restart()
    {       
        SceneManager.LoadScene("MainScene");
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void IncreaseTime(float amount)
    {
        currentTime += amount;
    }
}
