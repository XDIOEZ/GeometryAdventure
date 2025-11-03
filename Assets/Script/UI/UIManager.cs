using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UI管理器，负责管理所有UI面板的显示、隐藏和生命周期
/// </summary>
public class UIManager : MonoBehaviour
{
    #region Singleton
    private static UIManager _instance;
    
    /// <summary>
    /// 获取UIManager单例实例
    /// </summary>
    public static UIManager Instance 
    { 
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<UIManager>();
                
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject("UIManager");
                    _instance = singletonObject.AddComponent<UIManager>();
                }
            }
            return _instance;
        }
    }
    #endregion

    #region Fields
    /// <summary>
    /// 存储所有面板的字典
    /// </summary>
    private Dictionary<string, BasePanel> panels = new Dictionary<string, BasePanel>();
    
    /// <summary>
    /// 面板的父对象
    /// </summary>
    [Tooltip("面板的父对象")]
    public Transform panelRoot;
    
    /// <summary>
    /// 预制体引用（可选）
    /// </summary>
    [Tooltip("面板预制体引用")]
    public GameObject[] panelPrefabs;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        // 确保只有一个UIManager实例
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        InitializePanels();
    }
    #endregion

    #region Panel Management
    /// <summary>
    /// 初始化所有面板
    /// </summary>
    private void InitializePanels()
    {
        panels.Clear();
        
        BasePanel[] allPanels = FindObjectsOfType<BasePanel>(true);
        foreach (BasePanel panel in allPanels)
        {
            if (!panels.ContainsKey(panel.name))
            {
                panels[panel.name] = panel;
            }
            else
            {
                Debug.LogWarning($"Duplicate panel name found: {panel.name}");
            }
        }
    }

    /// <summary>
    /// 获取指定名称的面板
    /// </summary>
    /// <param name="panelName">面板名称</param>
    /// <returns>BasePanel对象，如果未找到则返回null</returns>
    public BasePanel GetPanel(string panelName)
    {
        if (panels.TryGetValue(panelName, out BasePanel panel))
        {
            return panel;
        }
        
        Debug.LogWarning($"Panel '{panelName}' not found!");
        return null;
    }

    /// <summary>
    /// 显示指定面板
    /// </summary>
    /// <param name="panelName">面板名称</param>
    public void ShowPanel(string panelName)
    {
        BasePanel panel = GetPanel(panelName);
        if (panel != null)
        {
            panel.Open();
        }
    }

    /// <summary>
    /// 隐藏指定面板
    /// </summary>
    /// <param name="panelName">面板名称</param>
    public void HidePanel(string panelName)
    {
        BasePanel panel = GetPanel(panelName);
        if (panel != null)
        {
            panel.Close();
        }
    }

    /// <summary>
    /// 切换面板显示状态
    /// </summary>
    /// <param name="panelName">面板名称</param>
    public void TogglePanel(string panelName)
    {
        BasePanel panel = GetPanel(panelName);
        if (panel != null)
        {
            panel.Toggle();
        }
    }

    /// <summary>
    /// 检查面板是否打开
    /// </summary>
    /// <param name="panelName">面板名称</param>
    /// <returns>面板是否打开</returns>
    public bool IsPanelOpen(string panelName)
    {
        BasePanel panel = GetPanel(panelName);
        if (panel != null)
        {
            return panel.IsOpen();
        }
        return false;
    }

    /// <summary>
    /// 隐藏所有面板
    /// </summary>
    public void HideAllPanels()
    {
        foreach (var panel in panels.Values)
        {
            panel.Close();
        }
    }

    /// <summary>
    /// 显示所有面板
    /// </summary>
    public void ShowAllPanels()
    {
        foreach (var panel in panels.Values)
        {
            panel.Open();
        }
    }

    /// <summary>
    /// 设置面板的可见性
    /// </summary>
    /// <param name="panelName">面板名称</param>
    /// <param name="isVisible">是否可见</param>
    public void SetPanelVisible(string panelName, bool isVisible)
    {
        BasePanel panel = GetPanel(panelName);
        if (panel != null)
        {
            if (isVisible)
            {
                panel.Open();
            }
            else
            {
                panel.Close();
            }
        }
    }
    
    /// <summary>
    /// 注册面板到UIManager
    /// </summary>
    /// <param name="panel">要注册的面板</param>
    public void RegisterPanel(BasePanel panel)
    {
        if (panel != null && !panels.ContainsKey(panel.name))
        {
            panels[panel.name] = panel;
        }
        else if (panel != null && panels.ContainsKey(panel.name))
        {
            Debug.LogWarning($"Panel '{panel.name}' is already registered!");
        }
    }
    #endregion

    #region Panel Creation and Destruction
    /// <summary>
    /// 通过预制体创建新面板
    /// </summary>
    /// <param name="panelPrefabName">面板预制体名称</param>
    /// <param name="parent">父对象</param>
    /// <returns>创建的面板</returns>
    public BasePanel CreatePanel(string panelPrefabName, Transform parent = null)
    {
        GameObject panelPrefab = null;
        foreach (GameObject prefab in panelPrefabs)
        {
            if (prefab != null && prefab.name == panelPrefabName)
            {
                panelPrefab = prefab;
                break;
            }
        }
        
        if (panelPrefab == null)
        {
            Debug.LogWarning($"Panel prefab '{panelPrefabName}' not found!");
            return null;
        }
        
        Transform parentTransform = parent != null ? parent : (panelRoot != null ? panelRoot : transform);
        GameObject panelInstance = Instantiate(panelPrefab, parentTransform);
        
        BasePanel panel = panelInstance.GetComponent<BasePanel>();
        if (panel != null)
        {
            if (!panels.ContainsKey(panelInstance.name))
            {
                panels[panelInstance.name] = panel;
            }
            return panel;
        }
        else
        {
            Debug.LogWarning($"Panel prefab '{panelPrefabName}' does not have a BasePanel component!");
            Destroy(panelInstance);
            return null;
        }
    }

    /// <summary>
    /// 销毁指定面板
    /// </summary>
    /// <param name="panelName">面板名称</param>
    public void DestroyPanel(string panelName)
    {
        if (panels.TryGetValue(panelName, out BasePanel panel))
        {
            panels.Remove(panelName);
            if (panel != null && panel.gameObject != null)
            {
                Destroy(panel.gameObject);
            }
        }
    }

    /// <summary>
    /// 刷新面板列表（当动态添加面板时调用）
    /// </summary>
    public void RefreshPanels()
    {
        InitializePanels();
    }
    #endregion

    #region Utility Methods
    /// <summary>
    /// 获取所有面板名称
    /// </summary>
    /// <returns>面板名称列表</returns>
    public List<string> GetAllPanelNames()
    {
        return new List<string>(panels.Keys);
    }
    
    /// <summary>
    /// 获取指定标签的面板列表
    /// </summary>
    /// <param name="tag">标签名称</param>
    /// <returns>匹配标签的面板列表</returns>
    public List<BasePanel> GetPanelsByTag(string tag)
    {
        List<BasePanel> taggedPanels = new List<BasePanel>();
        
        foreach (var panel in panels.Values)
        {
            if (panel != null && panel.gameObject.CompareTag(tag))
            {
                taggedPanels.Add(panel);
            }
        }
        
        return taggedPanels;
    }
    
    /// <summary>
    /// 显示指定标签的所有面板
    /// </summary>
    /// <param name="tag">标签名称</param>
    public void ShowPanelsByTag(string tag)
    {
        List<BasePanel> taggedPanels = GetPanelsByTag(tag);
        foreach (BasePanel panel in taggedPanels)
        {
            panel.Open();
        }
    }
    
    /// <summary>
    /// 隐藏指定标签的所有面板
    /// </summary>
    /// <param name="tag">标签名称</param>
    public void HidePanelsByTag(string tag)
    {
        List<BasePanel> taggedPanels = GetPanelsByTag(tag);
        foreach (BasePanel panel in taggedPanels)
        {
            panel.Close();
        }
    }
    #endregion
}