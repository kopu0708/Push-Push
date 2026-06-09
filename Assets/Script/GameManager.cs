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
    public TextMeshProUGUI AnounceGoToMain;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void Update()
    {
        if (isGameOver)
        {
            if (Input.anyKeyDown)
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
        if (enemyScoreText != null) enemyScoreText.text = "Enemy: " + EnemyScore.ToString();
    }

    public void AddMyScore(int amount)
    {
        if (isGameOver) return;
        MyScore += amount;
        if (playerScoreText != null) playerScoreText.text = "Player: " + MyScore.ToString();
    }
    void GameEnd()
    {
        isGameOver = true;
        if(AnounceGoToMain != null)AnounceGoToMain.text = "Press any key to Restart\nPress [ESC] to MainMenu";
        if(MyScore > EnemyScore) { result.text = "YOU WIN!"; }
        else if(MyScore < EnemyScore) { result.text = "YOU LOSE"; }
        else { result.text = "DRAW!"; }

        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}