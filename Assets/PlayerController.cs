using UnityEngine;
using UnityEngine.UI;

public class NewMonoBehaviourScript : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 moveDirection;

    [Header("이동 및 힘 설정")]
    public float maxForce = 25f; // 충전되는 최대 힘
    public float chargeRate = 5f; // 초당 충전되는 힘
    public float currentForce = 0f;// 현재 충전 된 힘
    private float chargeDirection = 1f; //충전 방향

    [Header("UI 설정")]
    public Image gaugeImage; //게이지 바 연결할 변수
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal");   
        float moveY = Input.GetAxisRaw("Vertical");

        //대각선 연산 정규화
        moveDirection = new Vector2(moveX, moveY).normalized;

        if (Input.GetKey(KeyCode.Space) && rb.linearVelocity.magnitude < 0.1f)
        {
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
            currentForce = 0f;
        }


        if (Input.GetKeyUp(KeyCode.Space) && moveDirection != Vector2.zero) //스페이스바을 때면 발사
        {
            rb.AddForce(moveDirection * currentForce, ForceMode2D.Impulse);
            currentForce = 0f;
            chargeDirection = 1f;
        }
        else if(Input.GetKeyUp(KeyCode.Space) && moveDirection == Vector2.zero) //방향키를 안누르고 있으면 초기화
        {
            currentForce = 0f;
            chargeDirection = 1f;
        }

        if (gaugeImage != null)
        {
            gaugeImage.fillAmount = currentForce / maxForce;
        }
    }
}