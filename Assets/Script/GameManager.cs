using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
public class GameManager : MonoBehaviour
{
    public static GameManager instance; //싱글톤 

    [Header("게임 설정")]
    public float timeRemaining = 30f; //겜 시간
    public int MyScore = 0; // 내 점수
    public int EnemyScore; // 적 점수
    private bool isGameOver = false; // 게임오버 상태

    [Header("UI 연결")]
    public Text timeText;
    public Text scoreText;

    private void Awake()
    {
        if (instance == null)
        {
            return;
        }
        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0)
        {
            timeRemaining = 0;
            //GameOver(); 이름 바꿔야 할듯? 아무튼 이건 게임 끝났을 때 처리 
        }

        if (timeText != null) //남은 시간 띄워주기 
        {
            timeText.text = "Time: " + Mathf.Ceil(timeRemaining).ToString();
        }
    }
} //적이 떨어졌을 때 점수 추가 내가 떨어졌을 때 점수추가 하는 함수 만들고 그러면 또 수정해야하는게 떨어지고 나서 
//다시 리스폰 기능도 만들어야하고 이왕 만드는 거 효과음이나 배경음도 넣으면 좋겠네
