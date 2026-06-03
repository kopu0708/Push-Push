using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class Enemy : MonoBehaviour
{
    public enum AIType { Beginner, Intermediate, Advanced }//ai 난이도 
    [Header("AI 설정")]
    public AIType currentAI = AIType.Beginner;

    public float maxForce = 25f; // 충전되는 최대 힘
    public float chargeRate = 5f; // 초당 충전되는 힘
    public float currentForce = 0f;// 현재 충전 된 힘
    private float chargeDirection = 1f; //충전 방향

    private Rigidbody2D rb;
    private Transform target;
    public Image gaugeImage; // 적 게이지 바 UI

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            target = playerObj.transform;
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
    }

    IEnumerator BeginnerRoutine() //초급 무조건 풀 충전 돌격만 함 플레이어 방향으로
    {
        while (true)
        {
            if (target == null) yield break;

            yield return new WaitUntil(() => rb.linearVelocity.magnitude < 0.1f);
            yield return new WaitForSeconds(0.5f);

            currentForce = 0f;
            
            while(currentForce < maxForce)
            {
                currentForce += chargeRate * Time.deltaTime;

                yield return null;
            }

            currentForce = maxForce;

            if (target == null)
            {
                yield break;
            }

            Vector2 direction = (target.position - transform.position).normalized;
            rb.AddForce(direction * currentForce, ForceMode2D.Impulse);

            currentForce = 0f;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.gameObject.name == "Arena")
        {
            Debug.Log("적 장외로 떨어짐");

            Destroy(gameObject);
        }

        
    }
}