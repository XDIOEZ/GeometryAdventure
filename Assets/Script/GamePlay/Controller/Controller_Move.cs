using Mirror;
using System;
using UltEvents;
using UnityEngine;
using UnityEngine.InputSystem;

public class Controller_Move : NetworkBehaviour
{
    [Header("Components")]
    public Rigidbody2D rb;
    
    [Header("Movement Settings")]
    public float speed = 5f;
    
    [Header("Events")]
    public UltEvent<Vector2> onMove;
    
    private GamePlayerInput playerInput;
    private Vector2 moveDirection;

    public override void OnStartLocalPlayer()
    {
        if (isLocalPlayer)
        {
            InitializeInput();
        }
    }

    private void InitializeInput()
    {
        if (playerInput == null)
        {
            playerInput = new GamePlayerInput();
        }
        
        // 绑定输入事件
        playerInput.Player.Move.performed += OnMove;
        playerInput.Player.Move.canceled += OnMove;
        
        // 启用输入系统
        playerInput.Enable();
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        // 获取移动方向并更新刚体速度
        moveDirection = context.ReadValue<Vector2>();
        rb.velocity = moveDirection * speed;
        
        // 触发事件通知其他系统
        if (onMove != null)
        {
            onMove.Invoke(moveDirection);
        }
    }

    private void OnDestroy()
    {
        CleanupInput();
    }
    
    private void OnDisable()
    {
        // 只禁用，不销毁，这样在重新启用时可以恢复
        if (playerInput != null)
        {
            playerInput.Disable();
        }
    }
    
    private void OnEnable()
    {
        // 如果之前被禁用过，重新启用输入
        if (isLocalPlayer && playerInput != null)
        {
            playerInput.Enable();
        }
    }
    
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
                
                // 禁用输入系统
                playerInput.Disable();
                
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
    }
}