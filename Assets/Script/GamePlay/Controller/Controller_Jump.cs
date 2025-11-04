using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 控制玩家跳跃功能（保留原输入系统写法）
/// </summary>
public class Controller_Jump : MonoBehaviour
{
    #region Fields
    [Header("Components")]
    public Rigidbody2D rb;

    [Header("Jump Settings")]
    public float jumpForce = 10f;
    public LayerMask groundLayerMask = 1;
    public Transform groundCheck;
    public float footSpacing = 0.2f;
    public float groundCheckDistance = 0.15f; // 稍微加长一点，避免检测不到地面

    public GamePlayerInput playerInput;
    public bool isGrounded;
    public bool jumpInput;

    // 跳跃次数控制
    [Header("Multi-jump Settings")]
    public int maxJumpCount = 1; // 最大跳跃次数，1=单跳，2=二段跳
    private int currentJumpCount; // 当前剩余跳跃次数

    // 用来防止连跳
    public float lastGroundedTime;
    public float coyoteTime = 0.1f; // 土狼时间（离地一点点还可以跳）
    #endregion

    private void Start()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (groundCheck == null)
            groundCheck = transform;

        InitializeInput();
    }

    private void Update()
    {
        CheckGrounded();

        if (isGrounded == false)
        {
            // 判断是否处于土狼时间内（即仍然视为在地面）
            bool coyoteGrounded = (Time.time - lastGroundedTime <= coyoteTime);
            // 如果处于有效接地状态，则重置跳跃次数
            if (coyoteGrounded)
            {
                currentJumpCount = maxJumpCount;
            }
            else
            {
                currentJumpCount = 0;
            }
        }

        // 如果有跳跃输入且还有跳跃次数
        if (jumpInput && currentJumpCount > 0)
        {
            PerformJump();
            jumpInput = false;
        }
    }

    private void InitializeInput()
    {
        if (playerInput == null)
        {
            playerInput = GetComponent<Controller_PlayerInput>().InputActionAsset;
        }

        playerInput.Player.Jump.started += OnJumpStarted;
        playerInput.Player.Jump.canceled += OnJumpCanceled;
    }

    private void OnJumpStarted(InputAction.CallbackContext context)
    {
        jumpInput = true;
    }

    private void OnJumpCanceled(InputAction.CallbackContext context)
    {
        jumpInput = false;
    }

    private void CheckGrounded()
    {
        Vector2 leftFoot = new Vector2(groundCheck.position.x - footSpacing, groundCheck.position.y);
        Vector2 rightFoot = new Vector2(groundCheck.position.x + footSpacing, groundCheck.position.y);

        // 使用射线投射过滤器排除自身碰撞体
        RaycastHit2D leftHit = Physics2D.Raycast(leftFoot, Vector2.down, groundCheckDistance, groundLayerMask);
        RaycastHit2D rightHit = Physics2D.Raycast(rightFoot, Vector2.down, groundCheckDistance, groundLayerMask);

        // 检查射线是否击中了自身
        bool leftValid = leftHit.collider != null && leftHit.collider.gameObject != gameObject;
        bool rightValid = rightHit.collider != null && rightHit.collider.gameObject != gameObject;

        // 更新接地状态
        bool wasGrounded = isGrounded;
        isGrounded = leftValid || rightValid;

        // 如果刚刚接触地面，重置跳跃次数和最后接地时间
        if (isGrounded)
        {
            currentJumpCount = maxJumpCount;
            lastGroundedTime = Time.time;
        }
        // 如果之前在地面，现在不在地面，记录离开地面的时间
        else if (wasGrounded)
        {
            lastGroundedTime = Time.time;
        }
    }

    private void PerformJump()
    {
        rb.velocity = new Vector2(rb.velocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        
        // 减少跳跃次数
        currentJumpCount--;
    }

    private void OnDestroy()
    {
        if (playerInput != null && playerInput.Player.Jump != null)
        {
            playerInput.Player.Jump.started -= OnJumpStarted;
            playerInput.Player.Jump.canceled -= OnJumpCanceled;
        }
    }

    // ✅ 改成 OnDrawGizmos —— 即使没选中也能看见射线
    private void OnDrawGizmos()
    {
        if (groundCheck == null)
            return;

        Gizmos.color = Color.green;
        Vector2 leftFoot = new Vector2(groundCheck.position.x - footSpacing, groundCheck.position.y);
        Vector2 rightFoot = new Vector2(groundCheck.position.x + footSpacing, groundCheck.position.y);

        Gizmos.DrawLine(leftFoot, leftFoot + Vector2.down * groundCheckDistance);
        Gizmos.DrawLine(rightFoot, rightFoot + Vector2.down * groundCheckDistance);
    }
}