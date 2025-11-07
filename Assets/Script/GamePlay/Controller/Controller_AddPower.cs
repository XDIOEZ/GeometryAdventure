using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;

public class Controller_AddPower : NetworkBehaviour
{
    private EntityData playerData;
    private Controller_PlayerInput playerInput;
    
    private void Awake()
    {
        playerData = GetComponent<EntityData>();
        playerInput = GetComponent<Controller_PlayerInput>();
    }
    
    public override void OnStartLocalPlayer()
    {
        // 获取PlayerInput组件（如果Awake中没有获取到）
        if (playerInput == null)
        {
            playerInput = GetComponent<Controller_PlayerInput>();
        }
        
        // 注册空格键按下事件
        if (playerInput != null && playerInput.InputActionAsset != null)
        {
            var addPowerAction = playerInput.InputActionAsset.Player.AddPower;
            if (addPowerAction != null)
            {
                addPowerAction.performed += OnAddPowerPressed;
            }
        }
    }
    
    private void OnAddPowerPressed(InputAction.CallbackContext context)
    {
        // 只有本地玩家才能发送命令
        if (isLocalPlayer && playerData != null)
        {
            // 发送命令到服务器增加力量
            CmdAddPower();
        }
    }
    
    [Command]
private void CmdAddPower()
{

}
    
    private void OnDisable()
    {
        // 只有本地玩家需要取消注册事件
        if (isLocalPlayer && playerInput != null && playerInput.InputActionAsset != null)
        {
            var addPowerAction = playerInput.InputActionAsset.Player.AddPower;
            if (addPowerAction != null)
            {
                addPowerAction.performed -= OnAddPowerPressed;
            }
        }
    }
}