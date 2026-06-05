using UnityEngine;
using UnityEngine.UI;

public class NewMonoBehaviourScript : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 SpawnPosition;
    private Vector2 moveDirection;

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
    //public AudioClip hitSound;         // 충돌 소리 적에서 내주므로 필요없음
    public AudioClip fallSound;        // 장외 추락 소리
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        SpawnPosition = transform.position;
    }

    private void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal");   
        float moveY = Input.GetAxisRaw("Vertical");

        //대각선 연산 정규화
        moveDirection = new Vector2(moveX, moveY).normalized;

        if (Input.GetKey(KeyCode.Space) && rb.linearVelocity.magnitude < 0.1f)
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
        else if (!Input.GetKey(KeyCode.Space))
        {
            // 스페이스바를 떼고 있을 때는 차지가 안 됨 (안전장치)
        }
        else
        {
            // 멈추지 않은 상태에서 스페이스바를 누르고 있다면 기가 모이지 않게 0으로 유지
            audioSource.Stop();
            currentForce = 0f;
        }


        if (Input.GetKeyUp(KeyCode.Space) && moveDirection != Vector2.zero) //스페이스바을 때면 발사
        {
            audioSource.Stop(); //돌진 직전에 소리 멈추기
            if (dashSound != null) audioSource.PlayOneShot(dashSound); //돌진 소리 재생
            rb.AddForce(moveDirection * currentForce, ForceMode2D.Impulse);
            currentForce = 0f;
            chargeDirection = 1f;
        }
        else if(Input.GetKeyUp(KeyCode.Space) && moveDirection == Vector2.zero) //방향키를 안누르고 있으면 초기화
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


    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.name == "Arena")
        {
            Debug.Log("장외로 떨어짐");
            if (fallSound != null) audioSource.PlayOneShot(fallSound);
            GameManager.instance.AddEnemyScore(1); // 적 점수 오름

            // 플레이어 리스폰
            transform.position = SpawnPosition;
            rb.linearVelocity = Vector2.zero;
            currentForce = 0f;
        }
    }
}