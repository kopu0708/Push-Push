using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using System.Collections;

public class NetworkPlayer : NetworkBehaviour
{
    private Rigidbody2D rb;
    private Vector2 spawnPosition;
    private Color originalColor;
    private SpriteRenderer spriteRenderer;

    private int originalLayer;
    private int invincibleLayer;

    [Header("이동 및 힘 설정")]
    public float maxForce = 25f;
    public float chargeRate = 5f;
    public float currentForce = 0f;
    private float chargeDirection = 1f;

    [Header("UI 설정")]
    public Image gaugeImage;

    [Header("사운드 설정")]
    private AudioSource audioSource;
    public AudioClip chargeSound;
    public AudioClip dashSound;
    public AudioClip hitSound;
    public AudioClip fallSound;

    // [네트워크 동기화 변수]
    private NetworkVariable<bool> netIsCharging = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<bool> netIsInvincible = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        spawnPosition = transform.position;
        originalColor = spriteRenderer.color;
        originalLayer = LayerMask.NameToLayer("Player");
        invincibleLayer = LayerMask.NameToLayer("Invincible");

        // 네트워크 변수 이벤트 구독
        netIsCharging.OnValueChanged += OnChargingStateChanged;
        netIsInvincible.OnValueChanged += OnInvincibleStateChanged;
    }

    public override void OnNetworkDespawn()
    {
        netIsCharging.OnValueChanged -= OnChargingStateChanged;
        netIsInvincible.OnValueChanged -= OnInvincibleStateChanged;

        // 자기 자신의 ID에 따라 스폰 위치를 결정 (0번은 방장, 1번은 참가자)
        if (IsOwner)
        {
            if (OwnerClientId == 0)
            {
                transform.position = new Vector3(-5f, 0f, 0f); // 방장은 왼쪽
            }
            else if(OwnerClientId == 1)
            {
                transform.position = new Vector3(5f, 0f, 0f);  // 참가자는 오른쪽
            }
        }
    }

    private void Update()
    {
        // 1. 내 화면의 게이지바 UI 업데이트
        if (IsOwner && gaugeImage != null)
        {
            gaugeImage.fillAmount = currentForce / maxForce;
        }

        // 2. 키보드 입력은 내 캐릭터(Owner)만 처리
        if (!IsOwner) return;
        HandleInput();
    }

    void HandleInput()
    {
        // 키보드 방향키 입력 받기 (기존 PlayerInput 역할 대체)
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector2 moveDirection = new Vector2(h, v).normalized;

        bool isCharging = Input.GetKey(KeyCode.Space);
        bool isChargeReleased = Input.GetKeyUp(KeyCode.Space);

        // 움직이는 중에는 차지 불가
        if (isCharging && rb.linearVelocity.magnitude < 0.1f)
        {
            if (!netIsCharging.Value)
            {
                SetChargingServerRpc(true); // 서버에 차지 시작 알림
            }

            // 게이지 핑퐁 로직 (원본 완벽 복구)
            currentForce += chargeRate * chargeDirection * Time.deltaTime;

            if (currentForce > maxForce)
            {
                currentForce = maxForce;
                chargeDirection = -1f;
            }
            else if (currentForce < 0)
            {
                currentForce = 0f;
                chargeDirection = 1f;
            }
        }
        else if (!isCharging)
        {
            // 스페이스바를 떼고 있을 때는 안전장치 작동
            if (currentForce > 0 && !isChargeReleased)
            {
                currentForce = 0f;
                if (netIsCharging.Value) SetChargingServerRpc(false);
            }
        }

        // 스페이스바를 떼는 순간
        if (isChargeReleased)
        {
            SetChargingServerRpc(false); // 차지 종료 알림

            if (moveDirection != Vector2.zero) // 방향키를 누르고 있으면 발사
            {
                RequestDashServerRpc(moveDirection, currentForce);
            }
            // 방향키를 안 누르고 있으면 그냥 초기화

            currentForce = 0f;
            chargeDirection = 1f;
        }
    }

    #region 물리 충돌 및 장외 판정 (서버 주도)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 부딪힌 소리는 각자의 컴퓨터에서 재생해도 무방합니다.
        if (collision.gameObject.CompareTag("Player"))
        {
            if (hitSound != null) audioSource.PlayOneShot(hitSound);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        //  장외 판정과 점수 계산은 '서버'에서만 처리해야 중복 점수 획득을 막을 수 있습니다.
        if (!IsServer) return;

        if (collision.gameObject.name == "Arena")
        {
            if (GameManager.instance != null && GameManager.instance.timeRemaining <= 0) return;

            Debug.Log("장외로 떨어짐 (서버 판정)");

            // 점수 처리: 떨어지는 캐릭터의 주인이 누구냐에 따라 점수 분배
            // (Host가 OwnerClientId 0, Client가 OwnerClientId 1이라고 가정)
            if (OwnerClientId == 1) // Client가 떨어지면 Host 점수 업
            {
                GameManager.instance.AddMyScore(1);
            }
            else if (OwnerClientId == 0) // Host가 떨어지면 Client 점수 업
            {
                GameManager.instance.AddEnemyScore(1);
            }

            // 리스폰 및 무적 처리
            rb.linearVelocity = Vector2.zero;
            transform.position = spawnPosition;

            // 클라이언트들에게 소리를 내고 무적을 켜라고 명령
            PlayFallAudioClientRpc();
            StartCoroutine(InvincibleRoutine());
        }
    }
    #endregion

    #region 서버 및 클라이언트 RPC
    [ServerRpc]
    void SetChargingServerRpc(bool charging)
    {
        netIsCharging.Value = charging;
    }

    [ServerRpc]
    void RequestDashServerRpc(Vector2 direction, float force)
    {
        rb.AddForce(direction * force, ForceMode2D.Impulse);
        PlayDashAudioClientRpc();
    }

    [ClientRpc]
    void PlayDashAudioClientRpc()
    {
        audioSource.Stop(); // 기존 사운드 멈춤
        if (dashSound != null) audioSource.PlayOneShot(dashSound);
    }

    [ClientRpc]
    void PlayFallAudioClientRpc()
    {
        if (fallSound != null) audioSource.PlayOneShot(fallSound);
    }
    #endregion

    #region 상태 동기화 이벤트
    private void OnChargingStateChanged(bool previousValue, bool newValue)
    {
        if (newValue) // 기 모으기 시작
        {
            if (chargeSound != null && !audioSource.isPlaying)
            {
                audioSource.clip = chargeSound;
                audioSource.loop = true;
                audioSource.Play();
            }
        }
        else // 기 모으기 중단
        {
            audioSource.Stop();
        }
    }

    private IEnumerator InvincibleRoutine()
    {
        netIsInvincible.Value = true;
        yield return new WaitForSeconds(1.0f);
        netIsInvincible.Value = false;
    }

    private void OnInvincibleStateChanged(bool previousValue, bool newValue)
    {
        if (newValue)
        {
            gameObject.layer = invincibleLayer;
            StartCoroutine(BlinkRoutine());
        }
        else
        {
            gameObject.layer = originalLayer;
            spriteRenderer.color = originalColor;
        }
    }
    public void ExitGame()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            // 방장이라면 네트워크를 완전히 종료
            NetworkManager.Singleton.Shutdown();
        }
        else
        {
            // 참가자라면 연결만 끊음
            NetworkManager.Singleton.Shutdown();
        }

        // 로비 씬으로 이동
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
    private IEnumerator BlinkRoutine()
    {
        while (netIsInvincible.Value)
        {
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.3f);
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(0.1f);
        }
    }
    #endregion
}