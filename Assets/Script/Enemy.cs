using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
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

        SpawnPosition = transform.position; // 처음 시작 위치를 기억해둠
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            target = playerObj.transform;
            targetPos3Ago = target.position; //처음엔 현재 위치로 초기화
        }

        if(currentAI == AIType.Beginner)
        {
            StartCoroutine(BeginnerRoutine());
        }
        //여기에 나중에 다른 난이도 ai도 넣기
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
            Debug.Log("적 장외로 떨어짐");
            if (fallSound != null) audioSource.PlayOneShot(fallSound); //추락 할 때 사운드
            GameManager.instance.AddMyScore(1);

            transform.position = SpawnPosition;
            rb.linearVelocity = Vector2.zero;
            currentForce = 0f; 
        }
    }
}