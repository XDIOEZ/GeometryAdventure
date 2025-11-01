using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData : NetworkBehaviour
{
    public BasePanel basePanel;

    [SyncVar (hook = nameof(OnNameChanged))]
    public string playerName = "Player";
    [SyncVar(hook = nameof(OnColorChanged))]
    public Color playerColor = Color.white;

    public SpriteRenderer playerSpriteRenderer;

    public void OnNameChanged(string oldName, string newName)
    {
        // 确保在所有客户端上更新名字显示
        UpdateNameDisplay(newName);
    }

    public void OnColorChanged(Color oldColor, Color newColor)
    {
        // 确保在所有客户端上更新颜色显示
        UpdateColorDisplay(newColor);
    }

    private void UpdateNameDisplay(string name)
    {
        // 检查basePanel是否存在
        if (basePanel != null)
        {
            // 使用兼容的SetText方法
            basePanel.SetText("Name", name);
        }
        else
        {
            Debug.LogWarning("BasePanel is not assigned!");
        }
    }

    private void UpdateColorDisplay(Color color)
    {
        // 检查SpriteRenderer是否存在
        if (playerSpriteRenderer != null)
        {
            playerSpriteRenderer.color = color;
        }
        else
        {
            Debug.LogWarning("PlayerSpriteRenderer is not assigned!");
        }
    }

    [Command]
    public void CmdChangeName(string newName)
    {
        playerName = newName;
    }

    [Command]
    public void CmdChangeColor(Color newColor)
    {
        playerColor = newColor;
    }
    
    public override void OnStartLocalPlayer()
    {
        if (isLocalPlayer)
        {
            // 随机生成颜色和名字
            string randomName = "Player" + Random.Range(1, 100);
            Color randomColor = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
            
            // 发送到服务器
            CmdChangeName(randomName);
            CmdChangeColor(randomColor);
        }
    }
    
    // 在Start中也进行一次初始化，确保本地显示正确
    private void Start()
    {
        // 初始化显示当前值
        UpdateNameDisplay(playerName);
        UpdateColorDisplay(playerColor);
    }
    
    // 在Update中检查BasePanel是否被赋值（用于调试）
    private void Update()
    {
        // 仅用于调试，可以删除
        if (basePanel == null)
        {
            // 尝试自动查找BasePanel组件
            basePanel = GetComponentInChildren<BasePanel>();
        }
    }
}