using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public enum PlayerIndex { Player1, Player2 }
    [Header("플레이어 할당")]
    public PlayerIndex playerIndex = PlayerIndex.Player1;

    public Vector2 MoveDirection { get; private set;}
    public bool IsChargind { get; private set;}
    public bool IsChargeReleased { get; private set;}

    void Update()
    {
        float moveX = 0f;
        float moveY = 0f;

        if(playerIndex == PlayerIndex.Player1) //wasd spacebar 로 조작 
        {
            moveX = Input.GetAxisRaw("Horizontal");
            moveY = Input.GetAxisRaw("Vertical");

            IsChargind = Input.GetKey(KeyCode.Space);
            IsChargeReleased = Input.GetKeyUp(KeyCode.Space);
        }

        else if(playerIndex == PlayerIndex.Player2) //player2는 방향키와 k로 조작
        {
            moveX = Input.GetAxisRaw("Horizontal2");
            moveY = Input.GetAxisRaw("Vertical2");

            IsChargind = Input.GetKey(KeyCode.K);
            IsChargeReleased = Input.GetKeyUp(KeyCode.K);
        }

        //대각선 정규화
        MoveDirection = new Vector2(moveX, moveY).normalized;
    }
}
