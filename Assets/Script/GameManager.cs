using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    public static GameManager instance; //싱글톤 
    public static Enemy.AIType selectedDifficulty = Enemy.AIType.Beginner;

    [Header("게임 설정")]
    public float timeRemaining = 30f; //겜 시간
    public int MyScore = 0; // 내 점수
    public int EnemyScore = 0; // 적 점수
    private bool isGameOver = false; // 게임오버 상태

    [Header("UI 연결")]
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI playerScoreText;
    public TextMeshProUGUI enemyScoreText;
  

    [Header("게임 오버 처리")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI result;
    public TextMeshProUGUI announceGoToMain;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }   
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void Update()
    {
        if (isGameOver)
        {
            // 입력 처리를 Update에서 명확히 분리
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Time.timeScale = 1f; // 씬 넘어가기 전 시간 복구
                SceneManager.LoadScene("MainMenu");
            }
            else if (Input.anyKeyDown)
            {
                RestartGame();
            }
            return;
        }
        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0) //시간 다 되면 끝 
        {
            timeRemaining = 0;
            GameEnd();
        }

        if (timeText != null) //남은 시간 띄워주기 
        {
            timeText.text = "Time: " + Mathf.Ceil(timeRemaining).ToString();
        }
    }
    public void AddEnemyScore(int amount)
    {
        if (isGameOver) return;
        EnemyScore += amount;
        string sceneName = SceneManager.GetActiveScene().name;

        if (enemyScoreText != null)
        {
            // 로컬 씬이면 Player2, 아니면 Enemy로 출력
            if (sceneName == "GameSceneLocal")
            {
                enemyScoreText.text = "Player2: " + EnemyScore.ToString();
            }
            else
            {
                enemyScoreText.text = "Enemy: " + EnemyScore.ToString();
            }
        }
    }

    public void AddMyScore(int amount)
    {

        if (isGameOver) return;
        MyScore += amount;
        string sceneName = SceneManager.GetActiveScene().name;

        if (playerScoreText != null)
        {
            // 로컬 씬이면 Player1, 아니면 Player로 출력
            if (sceneName == "GameSceneLocal")
            {
                playerScoreText.text = "Player1: " + MyScore.ToString();
            }
            else
            {
                playerScoreText.text = "Player: " + MyScore.ToString();
            }
        }

    }
    void GameEnd()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        isGameOver = true;

        if (announceGoToMain != null) announceGoToMain.text = "Press any key to Restart\nPress [ESC] to MainMenu";
        if (sceneName == ("GameScene"))
        {
            if (MyScore > EnemyScore) { result.text = "YOU WIN!"; }
            else if (MyScore < EnemyScore) { result.text = "YOU LOSE"; }
            else { result.text = "DRAW!"; }
        }
        else if(sceneName == ("GameSceneLocal"))
        {
            if (MyScore > EnemyScore) { result.text = "Player1 WIN!"; }
            else if (MyScore < EnemyScore) { result.text = "Player2 WIN!"; }
            else { result.text = "DRAW!"; }
        }
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; // 재시작 시 시간 복구
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    
}