using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode.Transports.UTP;

public class LobbyManager : MonoBehaviour
{
    [Header("UI 버튼 연결")]
    public Button hostButton;
    public Button clientButton;
    public TMP_InputField ipInputField;

    // 게임 씬의 이름을 정확히 적어주세요. (예: Scene_Multi)
    public string gameSceneName = "Scene_Multi";

    private void Start()
    {
        // 버튼을 클릭했을 때 실행될 함수들을 연결해 줍니다.
        if (hostButton != null) hostButton.onClick.AddListener(StartHost);
        if (clientButton != null) clientButton.onClick.AddListener(StartClient);
    }

    private void StartHost()
    {
        // 1. 내가 방장이 되면서 내 캐릭터를 소환합니다.
        NetworkManager.Singleton.StartHost();

        // 2. 방장이 네트워크 권한을 가지고 다 함께 게임 씬으로 이동합니다.
        NetworkManager.Singleton.SceneManager.LoadScene(gameSceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    private void StartClient()
    {
        if (ipInputField != null && !string.IsNullOrEmpty(ipInputField.text))
        {
            // NetworkManager에 있는 UnityTransport 부품을 가져옵니다.
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            if (transport != null)
            {
                // 접속 주소를 입력된 텍스트로 강제 교체!
                transport.ConnectionData.Address = ipInputField.text;
            }
        }

        NetworkManager.Singleton.StartClient();
    }
}