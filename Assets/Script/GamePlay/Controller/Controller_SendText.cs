using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;
using System;
using System.Linq;

public class Controller_SendText : NetworkBehaviour
{
    #region 字段定义
    
    [Header("Network")]
    public NetworkManager networkManager;

    [Header("Player Data")]
    public EntityData playerData;

    [Header("UI Components")]
    [Tooltip("面板预制体")]
    public GameObject basePanelPrefab; // 现在场景中不会拥有BasePanel而是需要再开始的时候自动实例化一个面板出来

    [Tooltip("面板名称")]
    public string panelName = "ChatPanel";

    [Tooltip("输入框名称")]
    public string inputFieldName = "InputField";

    [Tooltip("发送按钮名称")]
    public string sendButtonName = "SendButton";

    [Tooltip("场景文本显示名称")]
    public string sceneTextName = "SceneText";

    // 存储实例化的面板引用
    private BasePanel basePanelInstance;
    private BasePanel basePanel;

    #endregion

    #region Unity生命周期方法

    public void Start()
    {
        // 所有客户端都需要监听文本更新
        // 通过UIManager获取指定面板（用于显示文本）
        if (basePanel == null && UIManager.Instance != null)
        {
            basePanel = UIManager.Instance.GetPanel(panelName);
        }
    }

    public override void OnStartLocalPlayer()
    {
        // 为本地玩家实例化UI面板
        if (isLocalPlayer)
        {
            InitializeUIPanel();
            
            // 初始化UI事件
            InitializeUIEvents();
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
            
            var inputField = basePanel.GetInputField_Legacy(inputFieldName);
            if (inputField != null)
            {
                inputField.onSubmit.RemoveListener(OnInputFieldSubmit);
            }
        }
        
        // 销毁实例化的面板
        if (isLocalPlayer && basePanelInstance != null)
        {
            Destroy(basePanelInstance.gameObject);
        }
    }
    
    #endregion

    #region UI初始化方法
    
    /// <summary>
    /// 初始化UI面板
    /// </summary>
    private void InitializeUIPanel()
    {
        // 如果提供了预制体，则实例化面板
        if (basePanelPrefab != null)
        {
            GameObject panelObject = Instantiate(basePanelPrefab);
            basePanelInstance = panelObject.GetComponent<BasePanel>();
            
            if (basePanelInstance != null)
            {
                basePanel = basePanelInstance;
                
                // 确保面板不被销毁
                DontDestroyOnLoad(panelObject);
                
                // 如果UIManager存在，注册面板
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.RegisterPanel(panelName, basePanelInstance);
                }
            }
            else
            {
                Debug.LogError("BasePanel组件在预制体中未找到！");
            }
        }
        else
        {
            // 如果没有提供预制体，尝试从UIManager获取现有面板
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
            var inputField = basePanel.GetInputField_Legacy(inputFieldName);
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
    
    #endregion

    #region UI事件处理方法
    
    /// <summary>
    /// 发送按钮点击事件
    /// </summary>
    private void SendText()
    {
        if (!isLocalPlayer)
        {
            return;
        }
        if (basePanel == null) return;
        
        // 获取输入框文本
        var inputField = basePanel.GetInputField_Legacy(inputFieldName);
        string text = "";
        if (inputField != null)
        {
            text = inputField.text;
        }
        
        // 检查文本是否为空
        if (!string.IsNullOrEmpty(text))
        {
            // 发送文本到场景
            SendTextToScene(text);
            
            // 清空输入框
            if (inputField != null)
            {
                inputField.text = "";
            }
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
            var inputField = basePanel.GetInputField_Legacy(inputFieldName);
            if (inputField != null)
            {
                inputField.text = "";
            }
        }
    }
    
    #endregion

    #region 文本发送相关方法
    
    /// <summary>
    /// 发送文本到场景
    /// </summary>
    /// <param name="text">要发送的文本</param>
    private void SendTextToScene(string text)
    {
        // 检查是否为命令（以/开头）
        if (text.StartsWith("/"))
        {
            // 处理命令
            ProcessCommand(text);
            return;
        }
        
        // 获取玩家名称
        string playerName = "Unknown";
        if (playerData != null)
        {
            playerName = playerData.playerName;
        }
        
        // 通过Command发送文本到服务器，附加玩家名称
        string formattedText = $"[{playerName}] {text}\n";
        CmdAppendSceneText(formattedText);
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
            // 获取文本组件并直接修改其文本属性
            var sceneText = basePanel.GetText_Legacy(sceneTextName);
            if (sceneText != null)
            {
                sceneText.text += text;
            }
        }
    }

    #endregion

    #region 文本管理命令方法
    
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
            // 获取文本组件并直接清空其文本属性
            var sceneText = basePanel.GetText_Legacy(sceneTextName);
            if (sceneText != null)
            {
                sceneText.text = "";
            }
        }
    }
    #endregion

    #region 命令处理相关方法

    /// <summary>
    /// 处理管理员命令
    /// </summary>
    private void ProcessCommand(string commandText)
    {
        // 移除开头的斜杠
        string command = commandText.Substring(1).Trim();

        string[] parts = command.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return;

        string cmd = parts[0].ToLower();
        string[] args = parts.Skip(1).ToArray();

        switch (cmd)
        {
            case "rename":
                if (args.Length > 0)
                {
                    string newName = args[0].Trim('"');
                    if (isLocalPlayer)
                    {
                        CmdRenamePlayer(newName);
                    }
                    else
                    {
                        ShowLocalCommandFeedback("错误: 权限不足");
                    }
                }
                else
                {
                    ShowLocalCommandFeedback("错误: rename命令需要指定新名字");
                }
                break;

            default:
                ShowLocalCommandFeedback($"未知命令: {cmd}");
                break;
        }
    }

    /// <summary>
    /// 重命名玩家
    /// </summary>
    [Command]
    private void CmdRenamePlayer(string newName)
    {
        if (!string.IsNullOrEmpty(newName) && newName.Length <= 20)
        {
            playerData.playerName = newName;
            // 通知客户端
            RpcPlayerRenamed(newName);
        }
        else
        {
            RpcShowCommandFeedback($"错误: 名字 '{newName}' 不合法，长度应在1-20个字符之间");
        }
    }

    /// <summary>
    /// 客户端接收到的新名字
    /// </summary>
    [ClientRpc]
    private void RpcPlayerRenamed(string newName)
    {
        playerData.playerName = newName;
        playerData.UpdateNameDisplay(playerData.playerName);
        ShowLocalCommandFeedback($"玩家已更名为: {newName}");
    }

    /// <summary>
    /// 广播命令反馈
    /// </summary>
    [ClientRpc]
    private void RpcShowCommandFeedback(string message)
    {
        ShowLocalCommandFeedback(message);
    }

    /// <summary>
    /// 本地 UI 显示反馈（不走网络）
    /// </summary>
    private void ShowLocalCommandFeedback(string message)
    {
        Debug.Log($"[系统] {message}");

        // 这里放你的 UI 显示逻辑，例如：
        // chatPanel.AddMessage($"[系统] {message}");
    }

    /// <summary>
    /// 服务器显示系统消息
    /// </summary>
    [Command]
    private void CmdSendSystemMessage(string message)
    {
        string formatted = $"[系统] {message}\n";
        CmdAppendSceneText(formatted);
    }

    #endregion
}