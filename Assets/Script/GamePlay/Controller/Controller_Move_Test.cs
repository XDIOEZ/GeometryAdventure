using System.Collections;
using System.Collections.Generic;
using UltEvents;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Controller_Move_Test : MonoBehaviour
{
    public GamePlayerInput playerInput = new GamePlayerInput();
    public UltEvent<Vector2> onMove;
    public Rigidbody2D rb;
    public float speed = 5f;

    private Vector2 moveDirection;

    private void Start()
    {
        // 需要启用输入系统
        playerInput.Enable();
        
        playerInput.Player.Move.performed += OnMovePerformed;
        playerInput.Player.Move.canceled += OnMoveCanceled;
        onMove += Move;
    }

    private void OnEnable()
    {
        // 当脚本启用时激活输入
        playerInput?.Enable();
    }

    private void OnDisable()
    {
        // 当脚本禁用时停用输入
        playerInput?.Disable();
    }

    private void Update()
    {
        onMove.Invoke(moveDirection);
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        moveDirection = context.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        moveDirection = Vector2.zero;
    }

    private void Move(Vector2 direction)
    {
        rb.velocity = direction * speed;
    }
    
    private void OnDestroy()
    {
        // 清理事件监听器
        if (playerInput != null)
        {
            playerInput.Player.Move.performed -= OnMovePerformed;
            playerInput.Player.Move.canceled -= OnMoveCanceled;
        }
    }
}