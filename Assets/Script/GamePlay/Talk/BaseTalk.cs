using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BaseTalk : MonoBehaviour
{
    // 对话数据结构
    private Dictionary<string, List<string>> dialogueDictionary = new Dictionary<string, List<string>>();
    private List<string> currentDialogueList = new List<string>();
    private int currentDialogueIndex = 0;
    private string currentDialogueKey = "default";

    public Text dialogueText;
    
    // 射线检测相关
    public float raycastDistance = 1.5f;
    public LayerMask playerLayer;
    
    // 翻页时间间隔
    public float dialogueInterval = 2.0f;
    private float lastDialogueTime;
    
    // 特殊字符触发器
    public string dialogueListTrigger = "[SWITCH:";
    
    void Start()
    {
        // 初始化默认对话
        InitializeDialogues();
        
        // 设置当前对话列表为默认列表
        if (dialogueDictionary.ContainsKey("default"))
        {
            currentDialogueList = dialogueDictionary["default"];
        }
        
        lastDialogueTime = -dialogueInterval; // 允许游戏开始时立即显示对话
    }
    
    void Update()
    {
        // 向上发射射线检测玩家（2D版本）
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.up, raycastDistance, playerLayer);
        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            // 检测到玩家，检查是否需要显示下一条对话
            if (Time.time - lastDialogueTime >= dialogueInterval)
            {
                DisplayNextDialogue();
                lastDialogueTime = Time.time;
            }
        }
    }
    
    // 初始化对话数据
    private void InitializeDialogues()
    {
        // 这里可以硬编码对话内容，也可以从外部加载
        // 示例对话数据
        dialogueDictionary.Add("default", new List<string>{
            "你好，旅行者！",
            "欢迎来到我们的村庄。",
            "有什么我能帮助你的吗？",
            "[SWITCH:quest]" // 切换到任务对话
        });
        
        dialogueDictionary.Add("quest", new List<string>{
            "我有一个任务要交给你。",
            "你能帮我收集一些草药吗？",
            "完成后我会给你奖励。",
            "[SWITCH:default]" // 切换回默认对话
        });
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
            }
        }
        else
        {
            // 在世界公屏上输出文字
            Debug.Log($"NPC: {dialogue}");
            // 实际项目中这里应该调用UI系统显示对话
        }
        
        currentDialogueIndex++;
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
    
    // 在场景中可视化射线（2D版本）
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 start = transform.position;
        Vector3 end = transform.position + Vector3.up * raycastDistance;
        Gizmos.DrawLine(start, end);
    }
}