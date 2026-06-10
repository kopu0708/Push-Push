using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 SpawnPosition;
    private Color originalColor; 
    private PlayerInput playerInput;

    private SpriteRenderer spriteRenderer;
    private int originalLayer;
    private int invincibleLayer;

    [Header("이동 및 힘 설정")]
    public float maxForce = 25f; // 충전되는 최대 힘
    public float chargeRate = 5f; // 초당 충전되는 힘
    public float currentForce = 0f;// 현재 충전 된 힘
    private float chargeDirection = 1f; //충전 방향

    [Header("UI 설정")]
    public Image gaugeImage; //게이지 바 연결할 변수

    [Header("사운드 설정")]
    private AudioSource audioSource;   // 내 몸에 붙은 스피커
    public AudioClip chargeSound;      // 기 모으기 소리 (루프)
    public AudioClip dashSound;        // 돌진 소리
    public AudioClip hitSound;         // 충돌 소리 적에서 내주므로 필요없음
    public AudioClip fallSound;        // 장외 추락 소리

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        SpawnPosition = transform.position;

        //몸에 붙은 이미지와 레이어 번호를 기억해 둔다. 색상도
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
        originalLayer = LayerMask.NameToLayer("Player");
        invincibleLayer = LayerMask.NameToLayer("Invincible");
    }

    private void Update()
    {
        Vector2 moveDirection = playerInput.MoveDirection;
        bool isCharging = playerInput.IsChargind;
        bool isChargeReleased = playerInput.IsChargeReleased;
        
        if (isCharging && rb.linearVelocity.magnitude < 0.1f)
        {
            if (chargeSound != null && !audioSource.isPlaying)
            {
                audioSource.clip = chargeSound;
                audioSource.loop = true;
                audioSource.Play();
            }
            currentForce += chargeRate * chargeDirection * Time.deltaTime;
            
            if(currentForce > maxForce)
            {
                currentForce = maxForce;
                chargeDirection = -1f; //다 찼으면 감소
            }
            else if(currentForce < 0)
            {
                currentForce = 0;
                chargeDirection = 1f; // 더하기로 전환
            }
           
        }
        else if (!isCharging)
        {
            // 스페이스바를 떼고 있을 때는 차지가 안 됨 (안전장치)
        }
        else
        {
            // 멈추지 않은 상태에서 스페이스바를 누르고 있다면 기가 모이지 않게 0으로 유지
            audioSource.Stop();
            currentForce = 0f;
        }


        if (isChargeReleased && moveDirection != Vector2.zero) //스페이스바을 때면 발사
        {
            audioSource.Stop(); //돌진 직전에 소리 멈추기
            if (dashSound != null) audioSource.PlayOneShot(dashSound); //돌진 소리 재생
            rb.AddForce(moveDirection * currentForce, ForceMode2D.Impulse);
            currentForce = 0f;
            chargeDirection = 1f;
        }
        else if(isChargeReleased && moveDirection == Vector2.zero) //방향키를 안누르고 있으면 초기화
        {
            audioSource.Stop();
            currentForce = 0f;
            chargeDirection = 1f;
        }

        if (gaugeImage != null)
        {
            gaugeImage.fillAmount = currentForce / maxForce;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // 상대방과 부딪히면 충돌 효과음 재생
            if (hitSound != null) audioSource.PlayOneShot(hitSound);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.name == "Arena")
        {
            if (GameManager.instance.timeRemaining <= 0) return;

            Debug.Log("장외로 떨어짐");
            if (fallSound != null) audioSource.PlayOneShot(fallSound);

            if (playerInput.playerIndex == PlayerInput.PlayerIndex.Player2)
            {
                GameManager.instance.AddMyScore(1); //플레이어2가 떨어지면 내 점수가 오르게 
            }
            else if(playerInput.playerIndex == PlayerInput.PlayerIndex.Player1)
            {
                GameManager.instance.AddEnemyScore(1); // 적 점수 오름
            }
          
            if (!gameObject.activeInHierarchy) return;

            // 플레이어 리스폰
            transform.position = SpawnPosition;
            rb.linearVelocity = Vector2.zero;
            currentForce = 0f;

            StartCoroutine(InvincibleRoutine());
        }
    }

    IEnumerator InvincibleRoutine() //1초 무적 코루틴 
    {
        // 1.레이어를 '무적'으로 바꿔서 적이 통과하게 만듦
        gameObject.layer = invincibleLayer;

        // 2. 1초 동안 5번 깜빡이기 (0.1초 투명, 0.1초 불투명)
        for (int i = 0; i < 5; i++)
        {
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.3f); // 반투명하게
            yield return new WaitForSeconds(0.1f);

            spriteRenderer.color = originalColor; 
            yield return new WaitForSeconds(0.1f);
        }

        // 3. 무적이 끝나면 다시 원래 플레이어 레이어로 복구해서 들이받을 수 있게 함
        gameObject.layer = originalLayer;
    }
}