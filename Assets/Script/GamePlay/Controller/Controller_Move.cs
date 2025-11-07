using Mirror;
using System;
using UltEvents;
using UnityEngine;
using UnityEngine.InputSystem;

public class Controller_Move : NetworkBehaviour
{
    #region Fields
    
    [Header("Components")]
    public Rigidbody2D rb;
    
    [Header("Movement Settings")]
    public float speed = 5f;
    
    [Header("Events")]
    public UltEvent<Vector2> onMove;
    
    private GamePlayerInput playerInput;
    private Vector2 moveDirection = Vector2.zero;
    
    #endregion

    #region Unity Lifecycle
    
    public override void OnStartLocalPlayer()
    {
        if (isLocalPlayer)
        {
            InitializeInput();
        }
    }
    
    [Client]
    private void Update()
    {
        // 在FixedUpdate中处理移动逻辑，确保物理计算的一致性
        if (isLocalPlayer && moveDirection != Vector2.zero)
        {
            ComMove();
        }
        else if (isLocalPlayer)
        {
            ComMove();
        }
    }
    
    private void OnDestroy()
    {
        CleanupInput();
    }
    
    private void OnDisable()
    {
        // 禁用时重置移动方向
        moveDirection = Vector2.zero;
    }

    #endregion
    #region Command Methods
    [Command]
    public void ComMove()
    {
        RpcMove();
    }

    [ClientRpc]
    public void RpcMove()
    {
        rb.velocity = new Vector2(moveDirection.x * speed, rb.velocity.y);
    }
    #endregion

    #region Input Methods

    /// <summary>
    /// 初始化玩家输入系统
    /// </summary>
    private void InitializeInput()
    {
        if (playerInput == null)
        {
            // 获取PlayerInput组件关联的InputActionAsset
            // PlayerInput组件会自动管理输入系统的启用/禁用
            playerInput = GetComponent<Controller_PlayerInput>().InputActionAsset;
        }
        
        // 绑定输入事件
        playerInput.Player.Move.performed += OnMove;
        playerInput.Player.Move.canceled += OnMove;
    }
    
    /// <summary>
    /// 处理移动输入事件
    /// </summary>
    /// <param name="context">输入回调上下文</param>
    private void OnMove(InputAction.CallbackContext context)
    {
        // 获取移动方向，但只保留水平方向
        Vector2 inputDirection = context.ReadValue<Vector2>();
        moveDirection = new Vector2(inputDirection.x, 0f); // 只保留x轴移动
        // 触发事件通知其他系统
        onMove?.Invoke(moveDirection);
    }



    /// <summary>
    /// 清理输入系统资源
    /// </summary>
    private void CleanupInput()
    {
        if (playerInput != null)
        {
            try
            {
                // 清理Move事件监听器
                if (playerInput.Player.Move != null)
                {
                    playerInput.Player.Move.performed -= OnMove;
                    playerInput.Player.Move.canceled -= OnMove;
                }
                
                // 显式销毁输入对象以避免内存泄漏
                playerInput = null;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Error cleaning up input: {e.Message}");
            }
        }
        
        // 清理事件引用
        if (onMove != null)
        {
            onMove.Clear();
        }
        
        // 重置移动方向
        moveDirection = Vector2.zero;
    }
    
    #endregion
}