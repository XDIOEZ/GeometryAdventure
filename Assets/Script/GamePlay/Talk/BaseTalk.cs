using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper.SerializedCollections;
using UnityEngine.UI;

public class BaseTalk : MonoBehaviour
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
    private int currentDialogueIndex = 0;
    private string currentDialogueKey;
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
            SwitchDialogueList(defaultDialogueKey);
        }
        else if (dialogueDictionary.Count > 0)
        {
            // 如果默认键不存在，使用第一个可用的对话列表
            var firstKey = dialogueDictionary.Keys.GetEnumerator();
            firstKey.MoveNext();
            SwitchDialogueList(firstKey.Current);
        }
        
        lastDialogueTime = -dialogueInterval; // 允许游戏开始时立即显示对话
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
    
    // 显示下一条对话
    // 1. 修复DisplayNextDialogue方法中重复的代码块
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
                // 递归调用以显示新对话列表的第一条
                DisplayNextDialogue();
                return;
            }
        }
        else
        {
            // 使用游戏UI面板显示对话，只操作Text组件
            if (dialoguePanel != null)
            {
                // 使用GetText_Legacy方法获取TalkText文本组件
                Text dialogueText = dialoguePanel.GetText_Legacy("TalkText");
                if (dialogueText != null)
                {
                    dialogueText.text = dialogue;
                    dialogueText.enabled = true; // 确保文本组件启用
                }
                else
                {
                    // 回退到Debug.Log以便调试
                    Debug.LogWarning("未找到名为TalkText的文本组件，使用Debug输出: " + dialogue);
                }
            }
            else
            {
                Debug.LogWarning("dialoguePanel未设置，使用Debug输出: " + dialogue);
            }
        }
        
        currentDialogueIndex++;
    }
    
    // 2. 修改Update方法，移除对canvasGroup的所有控制
    void Update()
    {
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
            // 玩家离开检测范围，只禁用TalkText文本组件
            if (dialoguePanel != null)
            {
                Text dialogueText = dialoguePanel.GetText_Legacy("TalkText");
                if (dialogueText != null)
                {
                    dialogueText.enabled = false;
                }
            }
        }
    }
    
    // 切换对话列表
    private void SwitchDialogueList(string key)
    {
        if (dialogueDictionary.ContainsKey(key))
        {
            currentDialogueList = dialogueDictionary[key];
            currentDialogueKey = key;
            currentDialogueIndex = 0; // 重置索引到新对话列表的开始
            Debug.Log($"切换到对话列表: {key}");
        }
        else
        {
            Debug.LogWarning($"对话列表 {key} 不存在！");
        }
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