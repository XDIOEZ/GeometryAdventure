using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 面板基类
/// 该类用于自动查找并管理自身子控件，
/// 帮助我们在代码中方便地操作UI控件，
/// 提供显示与隐藏面板的接口
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class BasePanel : MonoBehaviour
{
    // 每种UI类型都有一个字典 用来存储UI组件 名字就用挂接的gameObject.name 作为Key
    private Dictionary<string, Button> buttons = new Dictionary<string, Button>();
    private Dictionary<string, TMP_InputField> inputFields = new Dictionary<string, TMP_InputField>();
    private Dictionary<string, TextMeshProUGUI> textElements = new Dictionary<string, TextMeshProUGUI>();
    private Dictionary<string, Text> legacyTextElements = new Dictionary<string, UnityEngine.UI.Text>(); // 添加对旧版Text的支持
    private Dictionary<string, Toggle> toggles = new Dictionary<string, Toggle>();
    private Dictionary<string, Slider> sliders = new Dictionary<string, Slider>();
    private Dictionary<string, ScrollRect> scrollRects = new Dictionary<string, ScrollRect>();
    private Dictionary<string, Image> images = new Dictionary<string, Image>();
    private Dictionary<string, InputField> legacyInputFields = new();
    public CanvasGroup canvasGroup;
    public RectTransform rectTransform;
    
    // 记录面板的开关状态
    [SerializeField]
    private bool isOpen = false;

    protected virtual void Awake()
    {
        // 自动获取所有子对象上的UI组件
        CollectUIComponents();
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        
        // 初始化面板状态
        if (canvasGroup != null)
        {
            isOpen = canvasGroup.alpha > 0 && canvasGroup.interactable && canvasGroup.blocksRaycasts;
        }
        
        // 自动注册到UIManager
        RegisterToUIManager();
    }

    /// <summary>
    /// 注册到UIManager
    /// </summary>
    private void RegisterToUIManager()
    {
        // 检查UIManager是否存在
        if (UIManager.Instance != null)
        {
            // 将当前面板添加到UIManager中
            UIManager.Instance.RegisterPanel(this);
        }
        else
        {
            Debug.LogWarning($"UIManager instance not found. Panel '{name}' not registered.");
        }
    }

    /// <summary>
    /// 自动收集所有子对象上的UI组件
    /// </summary>
    public void CollectUIComponents()
    {
        // 清空现有字典
        buttons.Clear();
        inputFields.Clear();
        textElements.Clear();
        legacyTextElements.Clear(); // 清空旧版Text字典
        toggles.Clear();
        sliders.Clear();
        scrollRects.Clear();
        images.Clear();
        legacyInputFields.Clear();

        // 获取所有子对象上的InputField组件
        InputField[] allInputFields_legacy = GetComponentsInChildren<InputField>(true);
        foreach (InputField inputField in allInputFields_legacy)
        {
            if (!legacyInputFields.ContainsKey(inputField.name))
            {
                legacyInputFields[inputField.name] = inputField;
            }
        }

        // 获取所有子对象上的Button组件
        Button[] allButtons = GetComponentsInChildren<Button>(true);
        foreach (Button btn in allButtons)
        {
            if (!buttons.ContainsKey(btn.name))
            {
                buttons[btn.name] = btn;
                // 为按钮绑定点击事件
                btn.onClick.AddListener(() => OnClick(btn.name));
            }
        }

        // 获取所有子对象上的TMP_InputField组件
        TMP_InputField[] allInputFields = GetComponentsInChildren<TMP_InputField>(true);
        foreach (TMP_InputField inputField in allInputFields)
        {
            if (!inputFields.ContainsKey(inputField.name))
            {
                inputFields[inputField.name] = inputField;
            }
        }

        // 获取所有子对象上的TextMeshProUGUI组件
        TextMeshProUGUI[] allTexts = GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (TextMeshProUGUI text in allTexts)
        {
            if (!textElements.ContainsKey(text.name))
            {
                textElements[text.name] = text;
            }
        }

        // 获取所有子对象上的旧版Text组件 (UnityEngine.UI.Text)
        UnityEngine.UI.Text[] allLegacyTexts = GetComponentsInChildren<UnityEngine.UI.Text>(true);
        foreach (UnityEngine.UI.Text text in allLegacyTexts)
        {
            // 只添加那些没有被TextMeshProUGUI覆盖的文本组件
            if (!legacyTextElements.ContainsKey(text.name) && !textElements.ContainsKey(text.name))
            {
                legacyTextElements[text.name] = text;
            }
        }

        // 获取所有子对象上的Toggle组件
        Toggle[] allToggles = GetComponentsInChildren<Toggle>(true);
        foreach (Toggle toggle in allToggles)
        {
            if (!toggles.ContainsKey(toggle.name))
            {
                toggles[toggle.name] = toggle;
                // 为Toggle绑定值改变事件
                toggle.onValueChanged.AddListener((value) => OnValueChanged(toggle.name, value));
            }
        }

        // 获取所有子对象上的Slider组件
        Slider[] allSliders = GetComponentsInChildren<Slider>(true);
        foreach (Slider slider in allSliders)
        {
            if (!sliders.ContainsKey(slider.name))
            {
                sliders[slider.name] = slider;
            }
        }

        // 获取所有子对象上的ScrollRect组件
        ScrollRect[] allScrollRects = GetComponentsInChildren<ScrollRect>(true);
        foreach (ScrollRect scrollRect in allScrollRects)
        {
            if (!scrollRects.ContainsKey(scrollRect.name))
            {
                scrollRects[scrollRect.name] = scrollRect;
            }
        }

        // 获取所有子对象上的Image组件
        Image[] allImages = GetComponentsInChildren<Image>(true);
        foreach (Image image in allImages)
        {
            if (!images.ContainsKey(image.name))
            {
                images[image.name] = image;
            }
        }

        // 为"关闭"按钮注册关闭事件（如果存在）
        if (buttons.ContainsKey("关闭"))
        {
            buttons["关闭"].onClick.AddListener(() => Close());
        }

        // 为"销毁"按钮注册销毁事件（如果存在）
        if (buttons.ContainsKey("销毁"))
        {
            buttons["销毁"].onClick.AddListener(() => Destroy(gameObject));
        }
    }

    #region 面板显示控制

    public void Open()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            isOpen = true;
        }
    }

    public void Close()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            isOpen = false;
        }
    }

    public bool IsOpen()
    {
        // 使用记录的状态而不是每次都检查CanvasGroup属性
        return isOpen;
    }

    public bool IsVisible()
    {
        // 使用记录的状态而不是每次都检查CanvasGroup属性
        return isOpen;
    }
    
    /// <summary>
    /// 切换当前面板的显示状态
    /// 如果当前是打开状态则关闭，否则打开
    /// </summary>
    public void Toggle()
    {
        if (IsOpen())
            Close();
        else
            Open();
    }

    #endregion

    #region 按钮操作

    /// <summary>
    /// 获取按钮组件
    /// </summary>
    /// <param name="buttonName">按钮名称</param>
    /// <returns>按钮组件，如果不存在返回null</returns>
    public Button GetButton(string buttonName)
    {
        if (buttons.TryGetValue(buttonName, out Button button))
        {
            return button;
        }
        Debug.LogWarning($"未找到名为 {buttonName} 的按钮");
        return null;
    }

    #endregion

    #region 输入框操作

    /// <summary>
    /// 获取输入框组件
    /// </summary>
    /// <param name="inputFieldName">输入框名称</param>
    /// <returns>输入框组件，如果不存在返回null</returns>
    public TMP_InputField GetInputField(string inputFieldName)
    {
        if (inputFields.TryGetValue(inputFieldName, out TMP_InputField inputField))
        {
            return inputField;
        }
        Debug.LogWarning($"未找到名为 {inputFieldName} 的输入框");
        return null;
    }

    public InputField GetInputField_Legacy(string inputFieldName)
    {
        if (legacyInputFields.TryGetValue(inputFieldName, out InputField inputField))
        {
            return inputField;
        }
        Debug.LogWarning($"未找到名为 {inputFieldName} 的输入框");
        return null;
    }

    #endregion

    #region 文本操作

    /// <summary>
    /// 获取文本组件 (已废弃，为了向后兼容保留)
    /// </summary>
    /// <param name="textName">文本名称</param>
    /// <returns>文本组件，如果不存在返回null</returns>
    public TextMeshProUGUI GetText(string textName)
    {
        if (textElements.TryGetValue(textName, out TextMeshProUGUI text))
        {
            return text;
        }
        
        // 如果找不到TMP文本，检查是否存在旧版Text
        if (legacyTextElements.ContainsKey(textName))
        {
            Debug.LogWarning($"{textName} 是一个旧版Text组件，建议使用TextMeshProUGUI替换");
        }
        
        Debug.LogWarning($"未找到名为 {textName} 的文本组件");
        return null;
    }

    public Text GetText_Legacy(string textName)
    {
        if (legacyTextElements.TryGetValue(textName, out Text text))
        {
            return text;
        }

        //Debug.LogWarning($"未找到名为 {textName} 的文本组件");
        return null;
    }

    #endregion

    #region Toggle操作

    /// <summary>
    /// 获取Toggle组件
    /// </summary>
    /// <param name="toggleName">Toggle名称</param>
    /// <returns>Toggle组件，如果不存在返回null</returns>
    public Toggle GetToggle(string toggleName)
    {
        if (toggles.TryGetValue(toggleName, out Toggle toggle))
        {
            return toggle;
        }
        Debug.LogWarning($"未找到名为 {toggleName} 的Toggle");
        return null;
    }

    #endregion

    #region Slider操作

    /// <summary>
    /// 获取Slider组件
    /// </summary>
    /// <param name="sliderName">Slider名称</param>
    /// <returns>Slider组件，如果不存在返回null</returns>
    public Slider GetSlider(string sliderName)
    {
        if (sliders.TryGetValue(sliderName, out Slider slider))
        {
            return slider;
        }
        Debug.LogWarning($"未找到名为 {sliderName} 的Slider");
        return null;
    }

    #endregion

    #region 通用操作

    /// <summary>
    /// 重新收集所有UI组件（当动态添加UI组件时调用）
    /// </summary>
    public void RefreshUIComponents()
    {
        CollectUIComponents();
    }

    #endregion

    #region 事件处理

    /// <summary>
    /// 按钮点击事件响应
    /// 通过子类重写来处理不同按钮的点击逻辑
    /// </summary>
    /// <param name="btnName">按钮名称</param>
    protected virtual void OnClick(string btnName)
    {

    }

    /// <summary>
    /// Toggle开关值改变事件响应
    /// 通过子类重写来处理不同Toggle的值变化逻辑
    /// </summary>
    /// <param name="toggleName">Toggle名称</param>
    /// <param name="value">Toggle的当前值</param>
    protected virtual void OnValueChanged(string toggleName, bool value)
    {

    }

    #endregion
}