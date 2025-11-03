using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;

public class Controller_AddPower : NetworkBehaviour
{
    private PlayerData playerData;
    private PlayerInput playerInput;
    
    private void Awake()
    {
        playerData = GetComponent<PlayerData>();
        playerInput = GetComponent<PlayerInput>();
    }
    
    public override void OnStartLocalPlayer()
    {
        // 获取PlayerInput组件（如果Awake中没有获取到）
        if (playerInput == null)
        {
            playerInput = GetComponent<PlayerInput>();
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
    // 在服务器上执行力量增加操作
    if (playerData != null)
    {
        int amount = Mathf.RoundToInt(playerData.strengthGrowthRate);
        playerData.CmdAddStrength(amount);
    }
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