using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper.SerializedCollections;
using UnityEngine.UI;
using Mirror; // 添加Mirror命名空间

public class BaseTalk : NetworkBehaviour // 修改为继承NetworkBehaviour
{
    // 使用SerializedCollections插件的序列化字典作为配置中心
    [Header("对话配置")]
    public SerializedDictionary<string, List<string>> dialogueDictionary = new SerializedDictionary<string, List<string>>();
    public string defaultDialogueKey = "default";
    public string dialogueListTrigger = "[SWITCH:";
    public float dialogueInterval = 2.0f;
    
    [Header("射线检测配置")]
    public float raycastDistance = 1.5f;
    public LayerMask playerLayer;

    public BasePanel dialoguePanel;
    
    // 内部状态
    private List<string> currentDialogueList;
    
    [SyncVar(hook = nameof(OnCurrentDialogueIndexChanged))] // 同步当前对话索引
    private int currentDialogueIndex = 0;
    
    [SyncVar(hook = nameof(OnCurrentDialogueKeyChanged))] // 重新添加同步对话键
    private string currentDialogueKey;
    
    // 新增同步对话内容的SyncVar
    [SyncVar(hook = nameof(OnDialogueContentChanged))]
    private string currentDialogueContent;
    
    // 新增同步对话框显示状态的SyncVar
    [SyncVar(hook = nameof(OnDialogueVisibleChanged))]
    private bool isDialogueVisible = false;
    
    private float lastDialogueTime;
    
    void Start()
    {
        //TDOO 自动获取子对象上的BasePanel
        dialoguePanel = GetComponentInChildren<BasePanel>();
        if (dialoguePanel == null)
        {
            Debug.LogError("BaseTalk 未找到子对象上的 BasePanel 组件");
        }

        // 初始化对话系统
        InitializeDialogueSystem();
    }
    
    // 初始化对话系统
    private void InitializeDialogueSystem()
    {
        // 确保字典不为空，如果为空则创建默认对话
        if (dialogueDictionary.Count == 0)
        {
            CreateDefaultDialogue();
        }
        
        // 设置当前对话列表为默认列表
        if (dialogueDictionary.ContainsKey(defaultDialogueKey))
        {
            // 在服务器端设置对话键，会通过SyncVar同步到客户端
            if (isServer)
            {
                currentDialogueKey = defaultDialogueKey;
                currentDialogueIndex = 0;
                isDialogueVisible = false; // 初始时隐藏对话框
            }
            else if (isClient)
            {
                // 客户端初始化时也直接设置对话列表，避免等待SyncVar同步的延迟
                SetDialogueList(defaultDialogueKey);
                // 隐藏占位符文本
                HidePlaceholderText();
            }
        }
        else if (dialogueDictionary.Count > 0)
        {
            // 如果默认键不存在，使用第一个可用的对话列表
            var firstKey = dialogueDictionary.Keys.GetEnumerator();
            firstKey.MoveNext();
            if (isServer)
            {
                currentDialogueKey = firstKey.Current;
                currentDialogueIndex = 0;
                isDialogueVisible = false; // 初始时隐藏对话框
            }
            else if (isClient)
            {
                // 客户端直接设置
                SetDialogueList(firstKey.Current);
                // 隐藏占位符文本
                HidePlaceholderText();
            }
        }
        
        lastDialogueTime = -dialogueInterval; // 允许游戏开始时立即显示对话
    }
    
    // 隐藏占位符文本
    private void HidePlaceholderText()
    {
        if (dialoguePanel != null)
        {
            Text dialogueText = dialoguePanel.GetText_Legacy("TalkText");
            if (dialogueText != null)
            {
                dialogueText.enabled = false; // 先隐藏占位符
            }
        }
    }
    
    // 创建默认对话（当编辑器中未配置时使用）
    private void CreateDefaultDialogue()
    {
        // 第一条对话线 - 默认对话
        List<string> defaultLines = new List<string>();
        defaultLines.Add("你好，旅行者！");
        defaultLines.Add("欢迎来到我们的村庄。");
        defaultLines.Add("有什么我能帮助你的吗？");
        defaultLines.Add("[SWITCH:story]"); // 切换到故事对话线
        defaultLines.Add("这是默认对话线的最后一句。"); // 这条不会显示，因为上面已经切换了
        
        // 第二条对话线 - 故事对话
        List<string> storyLines = new List<string>();
        storyLines.Add("你想听个故事吗？");
        storyLines.Add("很久以前，这个村庄里有一位勇敢的战士。");
        storyLines.Add("他保护着我们免受外敌侵害。");
        storyLines.Add("[SWITCH:default]"); // 切换回默认对话线
        storyLines.Add("这是故事对话线的最后一句。"); // 这条不会显示，因为上面已经切换了
        
        // 添加两条对话线到字典
        dialogueDictionary.Add("default", defaultLines);
        dialogueDictionary.Add("story", storyLines);
    }
    
    // 设置对话列表的辅助方法
    private void SetDialogueList(string key)
    {
        if (dialogueDictionary.ContainsKey(key))
        {
            currentDialogueList = dialogueDictionary[key];
            // 重置索引
            currentDialogueIndex = 0;
        }
    }
    
    // SyncVar钩子方法 - 当currentDialogueKey变更时调用
    private void OnCurrentDialogueKeyChanged(string oldKey, string newKey)
    {
        // 更新本地currentDialogueList
        if (dialogueDictionary.ContainsKey(newKey))
        {
            currentDialogueList = dialogueDictionary[newKey];
            // 如果索引超出范围，重置索引
            if (currentDialogueIndex >= currentDialogueList.Count)
            {
                currentDialogueIndex = 0;
            }
            // 确保客户端也能正确显示对话
            if (currentDialogueList.Count > 0 && currentDialogueIndex < currentDialogueList.Count)
            {
                string dialogue = currentDialogueList[currentDialogueIndex];
                // 检查是否是对话列表切换触发器
                if (!dialogue.StartsWith(dialogueListTrigger))
                {
                    UpdateDialogueUI(dialogue);
                }
            }
        }
    }
    
    // SyncVar钩子方法 - 当currentDialogueIndex变更时调用
    private void OnCurrentDialogueIndexChanged(int oldIndex, int newIndex)
    {
        // 索引变更时可以在这里添加额外的逻辑
        // 例如触发动画、音效等
    }
    
    // SyncVar钩子方法 - 当对话内容变更时调用（关键修复：客户端更新UI）
    private void OnDialogueContentChanged(string oldContent, string newContent)
    {
        // 当服务器同步对话内容到客户端时，更新UI显示
        UpdateDialogueUI(newContent);
        // 设置对话框可见
        if (dialoguePanel != null)
        {
            Text dialogueText = dialoguePanel.GetText_Legacy("TalkText");
            if (dialogueText != null)
            {
                dialogueText.enabled = true;
            }
        }
    }
    
    // SyncVar钩子方法 - 当对话框显示状态变更时调用
    private void OnDialogueVisibleChanged(bool oldValue, bool newValue)
    {
        // 根据服务器同步的状态更新UI显示
        if (dialoguePanel != null)
        {
            Text dialogueText = dialoguePanel.GetText_Legacy("TalkText");
            if (dialogueText != null)
            {
                dialogueText.enabled = newValue;
            }
        }
    }
    
    // 更新对话UI的辅助方法
    private void UpdateDialogueUI(string dialogue)
    {
        if (dialoguePanel != null)
        {
            Text dialogueText = dialoguePanel.GetText_Legacy("TalkText");
            if (dialogueText != null)
            {
                dialogueText.text = dialogue;
                dialogueText.enabled = true; // 确保文本组件启用
            }
            else
            {
                Debug.LogWarning("未找到名为TalkText的文本组件，使用Debug输出: " + dialogue);
            }
        }
        else
        {
            Debug.LogWarning("dialoguePanel未设置，使用Debug输出: " + dialogue);
        }
    }
    
    // 显示下一条对话
    private void DisplayNextDialogue()
    {
        if (currentDialogueList == null || currentDialogueList.Count == 0)
            return;
        
        // 检查是否到达对话列表末尾
        if (currentDialogueIndex >= currentDialogueList.Count)
        {
            // 循环对话 - 重置索引
            currentDialogueIndex = 0;
        }
        
        string dialogue = currentDialogueList[currentDialogueIndex];
        
        // 检查是否是对话列表切换触发器
        if (dialogue.StartsWith(dialogueListTrigger))
        {
            // 提取目标对话列表的键
            int endBracketIndex = dialogue.IndexOf("]");
            if (endBracketIndex > dialogueListTrigger.Length)
            {
                string targetKey = dialogue.Substring(dialogueListTrigger.Length, endBracketIndex - dialogueListTrigger.Length);
                SwitchDialogueList(targetKey);
                return;
            }
        }
        else
        {
            // 关键修复：服务器端更新同步变量，这样会自动同步到所有客户端
            currentDialogueContent = dialogue;
            // 设置对话框可见
            isDialogueVisible = true;
        }
        
        // 在服务器上更新索引，会自动同步到所有客户端
        if (isServer)
        {
            currentDialogueIndex++;
        }
    }
    
    // 修改Update方法，移除对canvasGroup的所有控制
    void Update()
    {
        // 只有服务器需要执行检测逻辑并控制显示状态
        if (!isServer)
            return;
        
        // 向左和向右发射射线检测玩家（2D横版游戏版本）
        RaycastHit2D hitLeft = Physics2D.Raycast(transform.position, Vector2.left, raycastDistance, playerLayer);
        RaycastHit2D hitRight = Physics2D.Raycast(transform.position, Vector2.right, raycastDistance, playerLayer);
        
        // 检查任一方向是否检测到玩家
        bool playerDetected = (hitLeft.collider != null && hitLeft.collider.CompareTag("Player")) || 
                             (hitRight.collider != null && hitRight.collider.CompareTag("Player"));
        
        if (playerDetected)
        {
            // 检测到玩家，检查是否需要显示下一条对话
            if (Time.time - lastDialogueTime >= dialogueInterval)
            {
                DisplayNextDialogue();
                lastDialogueTime = Time.time;
            }
        }
        else
        {
            // 关键修复：玩家离开检测范围，服务器设置同步变量隐藏对话框
            isDialogueVisible = false;
        }
    }
    
    // 网络命令 - 客户端请求服务器显示下一条对话
    [Command]
    private void CmdDisplayNextDialogue()
    {
        DisplayNextDialogue();
    }
    
    // 切换对话列表
    public void SwitchDialogueList(string key)
    {
        // 只在服务器上执行切换操作
        if (isServer)
        {
            if (dialogueDictionary.ContainsKey(key))
            {
                // 通过设置currentDialogueKey来同步到所有客户端
                currentDialogueKey = key;
                currentDialogueIndex = 0; // 重置索引到新对话列表的开始
                Debug.Log($"切换到对话列表: {key}");
            }
            else
            {
                Debug.LogWarning($"对话列表 {key} 不存在！");
            }
        }
        else
        {
            // 客户端请求服务器切换对话列表
            CmdSwitchDialogueList(key);
        }
    }
    
    // 网络命令 - 客户端请求服务器切换对话列表
    [Command]
    private void CmdSwitchDialogueList(string key)
    {
        SwitchDialogueList(key);
    }
    
    // 在场景中可视化射线（2D横版游戏版本）
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 start = transform.position;
        // 绘制向左的射线
        Vector3 leftEnd = transform.position + Vector3.left * raycastDistance;
        Gizmos.DrawLine(start, leftEnd);
        // 绘制向右的射线
        Vector3 rightEnd = transform.position + Vector3.right * raycastDistance;
        Gizmos.DrawLine(start, rightEnd);
    }
}