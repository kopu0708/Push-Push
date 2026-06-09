using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private int originalLayer;
    private int invincibleLayer;

    [Header("사운드 설정")]
    private AudioSource audioSource;   // 내 몸에 붙은 스피커
    public AudioClip chargeSound;      // 기 모으기 소리 (루프)
    public AudioClip dashSound;        // 돌진 소리
    public AudioClip hitSound;         // 충돌 소리
    public AudioClip fallSound;        // 장외 추락 소리
    public enum AIType { Beginner, Intermediate, Advanced }//ai 난이도 
    [Header("AI 설정")]
    public AIType currentAI = AIType.Beginner;
    

    public float maxForce = 25f; // 충전되는 최대 힘
    public float chargeRate = 5f; // 초당 충전되는 힘
    public float currentForce = 0f;// 현재 충전 된 힘
    private float chargeDirection = 1f; //충전 방향

    private Rigidbody2D rb;
    private Transform target;
    private Vector2 SpawnPosition;
    public Image gaugeImage; // 적 게이지 바 UI

    struct PosRecord
    {
        public float time;
        public Vector2 position;
    }
    private Queue<PosRecord> positionHistory = new Queue<PosRecord>();
    private Vector2 targetPos3Ago;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();

        //몸에 붙은 이미지와 레이어 번호를 기억해 둔다.
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalLayer = gameObject.layer;
        invincibleLayer = LayerMask.NameToLayer("Invincible");

        SpawnPosition = transform.position; // 처음 시작 위치를 기억해둠
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            target = playerObj.transform;
            targetPos3Ago = target.position; //처음엔 현재 위치로 초기화
        }
        currentAI = GameManager.selectedDifficulty;
        if (currentAI == AIType.Beginner)
        {
            StartCoroutine(BeginnerRoutine());
        }
        else if (currentAI == AIType.Intermediate)
        {
            StartCoroutine(IntermediateRoutine());
        }
        else if(currentAI == AIType.Advanced)
        {
           StartCoroutine(AdvancedRoutine());
        }
    }

    void Update()
    {
        // 매 프레임 UI 게이지 바 업데이트
        if (gaugeImage != null)
        {
            gaugeImage.fillAmount = currentForce / maxForce;
        }

        if(target != null)
        {
            positionHistory.Enqueue(new PosRecord { time = Time.time, position = target.position });//현재 시간이랑 함께 위치를 올림
            while(positionHistory.Count > 0 && Time.time - positionHistory.Peek().time >= 3f)
            {
                targetPos3Ago = positionHistory.Dequeue().position; //3초 전 위치로 확정 
            }
        }
    }

    IEnumerator BeginnerRoutine() //초급 무조건 풀 충전 돌격만 함 3초전 플레이어 방향으로
    {
        while (true)
        {
            if (target == null) yield break;

            yield return new WaitUntil(() => rb.linearVelocity.magnitude < 0.1f);
            yield return new WaitForSeconds(0.5f);

            currentForce = 0f;
            
            if(chargeSound != null)
            {
                audioSource.clip = chargeSound;
                audioSource.loop = true;
                audioSource.Play();
            }

            while(currentForce < maxForce)
            {
                currentForce += chargeRate * Time.deltaTime;
                yield return null;
            }

            currentForce = maxForce;
            audioSource.Stop(); //돌직 직전에 충전 사운드 멈춤 
            if (dashSound != null) audioSource.PlayOneShot(dashSound); //돌진 소리 재생 

            if (target == null) yield break;

            Vector2 direction = (targetPos3Ago - (Vector2)transform.position).normalized;
            rb.AddForce(direction * currentForce, ForceMode2D.Impulse);

            currentForce = 0f;
        }
    }

    IEnumerator IntermediateRoutine() //중급 봇 60~100% 랜덤 충전후 발사 그리고 현재 위치로 
    {
        while (true)
        {
            if (target == null) yield break;

            yield return new WaitUntil(() => rb.linearVelocity.magnitude < 0.1f);
            yield return new WaitForSeconds(0.3f);

            currentForce = 0f;

            float chargeForce = Random.Range(maxForce * 0.6f, maxForce);

            if(chargeSound != null)
            {
                audioSource.clip = chargeSound;
                audioSource.loop = true;
                audioSource.Play();
            }

            while (currentForce < maxForce)
            {
                currentForce += chargeRate * Time.deltaTime;
                yield return null;
            }

            audioSource.Stop();
            if (dashSound != null) audioSource.PlayOneShot(dashSound);

            if (target == null) yield break;

            Vector2 direction = ((Vector2)target.position - (Vector2)transform.position).normalized;
            rb.AddForce(direction * currentForce, ForceMode2D.Impulse);

            currentForce = 0f;
        }
    }

    IEnumerator AdvancedRoutine()
    {
        Vector2 arenaCenter = Vector2.zero;// 맵 중앙 위치
        while (true)
        {
            if (target == null) yield break;

            yield return new WaitUntil(() => rb.linearVelocity.magnitude < 0.1f);
            //[1] 생존 본능(자리싸움)
            // 현재 내 위치가 중앙에서 3.5f 이상 멀어졌다면 (낭떠러지 위험 구역)
            float distanceFromCenter = Vector2.Distance(transform.position, arenaCenter);
            if(distanceFromCenter > 3.5f)
            {
                float moveTimer = 0f;
                while(moveTimer < 1f)
                {
                    moveTimer += Time.deltaTime;
                    Vector2 toCenter = (arenaCenter - (Vector2)transform.position).normalized;

                    rb.AddForce(toCenter * 10f);
                    yield return null;
                }
                yield return new WaitUntil(() => rb.linearVelocity.magnitude < 0.1f);
            }
            // [2] 맹공격 턴 (짧은 시간 2배속 충전 후 돌진)
            yield return new WaitForSeconds(Random.Range(0.1f, 0.2f));

            currentForce = 0f;
            float randomChargeTime = Random.Range(0.2f, 0.6f);
            float chargeTimer = 0f;

            if (chargeSound != null)
            {
                audioSource.clip = chargeSound;
                audioSource.loop = true;
                audioSource.Play();
            }

            while (chargeTimer < randomChargeTime)
            {
                chargeTimer += Time.deltaTime;

                // 충전 속도 2배 특권
                currentForce += (chargeRate * 2f) * Time.deltaTime;
                currentForce = Mathf.Clamp(currentForce, 0, maxForce);

                yield return null;
            }

            audioSource.Stop();
            if (dashSound != null) audioSource.PlayOneShot(dashSound);

            if (target == null) yield break;

            // 정확하게 현재 플레이어 위치를 노림
            Vector2 direction = ((Vector2)target.position - (Vector2)transform.position).normalized;
            rb.AddForce(direction * currentForce, ForceMode2D.Impulse);

            currentForce = 0f;
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
        if(collision.gameObject.name == "Arena")
        {
            if (GameManager.instance.timeRemaining <= 0) return;

            Debug.Log("적 장외로 떨어짐");
            if (fallSound != null) audioSource.PlayOneShot(fallSound); //추락 할 때 사운드
            GameManager.instance.AddMyScore(1);

            if (!gameObject.activeInHierarchy) return;

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
            spriteRenderer.color = new Color(1f, 1f, 1f, 0.3f); // 반투명하게
            yield return new WaitForSeconds(0.1f);

            spriteRenderer.color = new Color(0.9f, 0.9f, 0.18f, 1f);   // 원래 색으로
            yield return new WaitForSeconds(0.1f);
        }

        // 3. 무적이 끝나면 다시 원래 플레이어 레이어로 복구해서 들이받을 수 있게 함
        gameObject.layer = originalLayer;
    }
}