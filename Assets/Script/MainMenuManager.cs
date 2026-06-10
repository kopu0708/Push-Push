using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Rendering;
public class MainMenuManager : MonoBehaviour
{
    [Header("UI 판넬 연결")]
    public GameObject difficultyPopupPanel; // 난이도 팝업 
    public GameObject MultiSelectPanel; //멀티 선택 팝업

    [Header("드롭다운 연결")]
    public TMP_Dropdown difficultyDropdown; //드롭다운 컴포넌트 불러오기

    private void Start()
    {
        if(difficultyPopupPanel != null)
        {
            difficultyPopupPanel.SetActive(false); // 처음엔 꺼두기 
        }
        if(MultiSelectPanel != null)
        {
            MultiSelectPanel.SetActive(false); //마찬가지로 
        }
    }
    public void openDifficultyPopup()
    {
        if(difficultyPopupPanel != null)
        {
            difficultyPopupPanel.SetActive(true);
        }
    }

    public void CloseDifficultyPopup()
    {
        if(difficultyPopupPanel != null)
        {
            difficultyPopupPanel.SetActive(false);
        }
    }

    public void ClickMultiPlay()
    {
        if(MultiSelectPanel != null)
        {
            MultiSelectPanel.SetActive(true);
        }
    }
    public void StartLocalGame()
    {
        SceneManager.LoadScene("GameSceneLocal");
    }

    public void StartMultiGame()
    {
        //아직 구현 안됨
    }
    public void closeMultiPlay()
    {
        if(MultiSelectPanel != null)
        {
            MultiSelectPanel.SetActive(false);
        }
    }
    public void QuitGame()
    {
        Debug.Log("이제 그만할거야");
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; 
        #else
        Application.Quit(); 
        #endif
    }
    
    public void ConfirmAndStartGame()
    {
        if (difficultyPopupPanel == null) return;

        int selectedIndex = difficultyDropdown.value; //드롭다운에서 몇 번째 항목을 골랐는지 번호를 가져옴 
        switch (selectedIndex)
        {
            case 0:
                GameManager.selectedDifficulty = Enemy.AIType.Beginner;
                break;
            case 1:
                GameManager.selectedDifficulty = Enemy.AIType.Intermediate;
                break;
            case 2:
                GameManager.selectedDifficulty = Enemy.AIType.Advanced;
                break;
        }
        SceneManager.LoadScene("GameScene");
    }
}
