using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class Controller_SendText : NetworkBehaviour
{
    public BasePanel basePanel;
    
    [Header("Network")]
    public NetworkManager networkManager;
    
    [Header("UI Components")]
    public string panelName = "ChatPanel"; // 面板名称
    public string inputFieldName = "InputField"; // 输入框名称
    public string sendButtonName = "SendButton"; // 发送按钮名称
    public string sceneTextName = "SceneText"; // 场景文本显示名称

    public override void OnStartLocalPlayer()
    {
        // 只有本地玩家才需要处理UI交互
        if (isLocalPlayer)
        {
            // 通过UIManager获取指定面板
            if (UIManager.Instance != null)
            {
                basePanel = UIManager.Instance.GetPanel(panelName);
                if (basePanel == null)
                {
                    Debug.LogWarning($"Panel '{panelName}' not found in UIManager!");
                }
            }
            else
            {
                Debug.LogWarning("UIManager instance not found!");
            }
            
            // 初始化UI事件
            InitializeUIEvents();
        }
    }
    
    public override void OnStartClient()
    {
        // 所有客户端都需要监听文本更新
        // 通过UIManager获取指定面板（用于显示文本）
        if (basePanel == null && UIManager.Instance != null)
        {
            basePanel = UIManager.Instance.GetPanel(panelName);
        }
    }

    /// <summary>
    /// 初始化UI事件
    /// </summary>
    private void InitializeUIEvents()
    {
        if (basePanel != null)
        {
            // 为发送按钮添加点击事件
            Button sendButton = basePanel.GetButton(sendButtonName);
            if (sendButton != null)
            {
                sendButton.onClick.AddListener(SendText);
            }
            
            // 也可以为输入框添加回车发送功能
            TMP_InputField inputField = basePanel.GetInputField(inputFieldName);
            if (inputField != null)
            {
                inputField.onSubmit.AddListener(OnInputFieldSubmit);
            }
        }
        
        // 如果没有指定NetworkManager，尝试自动查找
        if (networkManager == null)
        {
            networkManager = FindObjectOfType<NetworkManager>();
        }
    }

    /// <summary>
    /// 发送按钮点击事件
    /// </summary>
    private void SendText()
    {
        if (basePanel == null) return;
        
        // 获取输入框文本
        string text = basePanel.GetInputFieldText(inputFieldName);
        
        // 检查文本是否为空
        if (!string.IsNullOrEmpty(text))
        {
            // 发送文本到场景
            SendTextToScene(text);
            
            // 清空输入框
            basePanel.SetInputFieldText(inputFieldName, "");
        }
    }
    
    /// <summary>
    /// 输入框回车提交事件
    /// </summary>
    /// <param name="text">输入的文本</param>
    private void OnInputFieldSubmit(string text)
    {
        // 检查文本是否为空
        if (!string.IsNullOrEmpty(text))
        {
            // 发送文本到场景
            SendTextToScene(text);
            
            // 清空输入框
            basePanel.SetInputFieldText(inputFieldName, "");
        }
    }

    /// <summary>
    /// 发送文本到场景
    /// </summary>
    /// <param name="text">要发送的文本</param>
    private void SendTextToScene(string text)
    {
        // 通过Command发送文本到服务器
        CmdAppendSceneText(text + "\n");
    }
    
    /// <summary>
    /// 客户端请求添加文本到场景文本
    /// </summary>
    /// <param name="text">要添加的文本</param>
    [Command]
    private void CmdAppendSceneText(string text)
    {
        // 在服务器上更新所有客户端的场景文本
        RpcUpdateSceneText(text);
    }
    
    /// <summary>
    /// 服务器向所有客户端广播文本更新
    /// </summary>
    /// <param name="text">要添加的文本</param>
    [ClientRpc]
    private void RpcUpdateSceneText(string text)
    {
        // 在所有客户端上更新场景文本显示
        if (basePanel != null)
        {
            string currentText = basePanel.GetTextContent(sceneTextName);
            basePanel.SetText(sceneTextName, currentText + text);
        }
    }
    
    /// <summary>
    /// 清空场景文本
    /// </summary>
    [Command]
    public void CmdClearSceneText()
    {
        RpcClearSceneText();
    }
    
    /// <summary>
    /// 在所有客户端上清空场景文本
    /// </summary>
    [ClientRpc]
    private void RpcClearSceneText()
    {
        if (basePanel != null)
        {
            basePanel.SetText(sceneTextName, "");
        }
    }
    
    /// <summary>
    /// 设置场景文本
    /// </summary>
    /// <param name="text">要设置的文本</param>
    [Command]
    public void CmdSetSceneText(string text)
    {
        RpcSetSceneText(text);
    }
    
    /// <summary>
    /// 在所有客户端上设置场景文本
    /// </summary>
    /// <param name="text">要设置的文本</param>
    [ClientRpc]
    private void RpcSetSceneText(string text)
    {
        if (basePanel != null)
        {
            basePanel.SetText(sceneTextName, text);
        }
    }
    
    private void OnDestroy()
    {
        // 只有本地玩家才需要清理UI事件
        if (isLocalPlayer && basePanel != null)
        {
            Button sendButton = basePanel.GetButton(sendButtonName);
            if (sendButton != null)
            {
                sendButton.onClick.RemoveListener(SendText);
            }
            
            TMP_InputField inputField = basePanel.GetInputField(inputFieldName);
            if (inputField != null)
            {
                inputField.onSubmit.RemoveListener(OnInputFieldSubmit);
            }
        }
    }
}