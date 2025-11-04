using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// 玙家数据管理类
/// 负责管理玩家名称和颜色的同步显示
/// </summary>
public class PlayerData : NetworkBehaviour
{
    #region 字段定义

    [SyncVar(hook = nameof(OnNameChanged))]
    public string playerName = "Player";

    [SyncVar(hook = nameof(OnColorChanged))]
    public Color playerColor = Color.white;

    [SyncVar(hook = nameof(OnHpChanged))]
    public int hp = 10;

    [SyncVar(hook = nameof(OnAttackChanged))]
    [Tooltip("玩家攻击力")]
    public int attack = 1;

    [SyncVar(hook = nameof(OnInvincibleChanged))]
    [Tooltip("无敌状态")]
    public bool isInvincible = false;

    [Tooltip("受伤后的无敌时间（秒）")]
    public float invincibleDuration = 1.0f;

    public SpriteRenderer playerSpriteRenderer;

    // 记录无敌状态的协程，用于取消之前的无敌状态
    private Coroutine invincibleCoroutine;


    #endregion

    #region 属性

    public BasePanel basePanel;

    #endregion

    #region Unity生命周期方法

    private void Start()
    {
        basePanel = GetComponentInChildren<BasePanel>();
        // 初始化显示当前值
        UpdateNameDisplay(playerName);
        UpdateColorDisplay(playerColor);
        UpdateHpDisplay(hp);
        UpdateAtkDisplay(attack);
        GameManager.Instance.AddPlayer(this);
    }

    protected override void OnValidate()
    {
        basePanel = GetComponentInChildren<BasePanel>();
        basePanel.CollectUIComponents();
        UpdateNameDisplay(playerName);
        UpdateColorDisplay(playerColor);
        UpdateHpDisplay(hp);
        UpdateAtkDisplay(attack);
    }
    public override void OnStartLocalPlayer()
    {
        if (isLocalPlayer)
        {
            // 生成随机名称和颜色
            string randomName = "Player" + Random.Range(1, 100);
            Color randomColor = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));

            // 发送到服务器
            CmdChangeName(randomName);
            CmdChangeColor(randomColor);
        }
    }

    #endregion

    #region 同步变量回调方法
    /// <summary>
    /// 玩家名称变更回调
    /// </summary>
    /// <param name="oldName">旧名称</param>
    /// <param name="newName">新名称</param>
    public void OnNameChanged(string oldName, string newName)
    {
        // 确保在所有客户端上正确更新名称显示
        UpdateNameDisplay(newName);
    }

    /// <summary>
    /// 玩家颜色变更回调
    /// </summary>
    /// <param name="oldColor">旧颜色</param>
    /// <param name="newColor">新颜色</param>
    public void OnColorChanged(Color oldColor, Color newColor)
    {
        // 确保在所有客户端上正确更新颜色显示
        UpdateColorDisplay(newColor);
    }

    /// <summary>
    /// 玩家生命值变更回调
    /// </summary>
    /// <param name="oldHp">旧生命值</param>
    /// <param name="newHp">新生命值</param>
    public void OnHpChanged(int oldHp, int newHp)
    {
        // 确保在所有客户端上正确更新生命值显示
        UpdateHpDisplay(newHp);
    }

    /// <summary>
    /// 玩家攻击力变更回调
    /// </summary>
    /// <param name="oldAttack">旧攻击力</param>
    /// <param name="newAttack">新攻击力</param>
    public void OnAttackChanged(int oldAttack, int newAttack)
    {
        UpdateAtkDisplay(newAttack);
    }

    /// <summary>
    /// 玩家无敌状态变更回调
    /// </summary>
    /// <param name="oldValue">旧无敌状态</param>
    /// <param name="newValue">新无敌状态</param>
    public void OnInvincibleChanged(bool oldValue, bool newValue)
    {
        // 可以在这里添加无敌状态变化时的视觉效果
        if (newValue)
        {
            // 进入无敌状态时的处理
            Debug.Log($"{playerName} 进入无敌状态");
        }
        else
        {
            // 退出无敌状态时的处理
            Debug.Log($"{playerName} 退出无敌状态");
        }
    }

    #endregion

    #region 显示更新方法

    /// <summary>
    /// 更新名称显示
    /// </summary>
    /// <param name="name">要显示的名称</param>
    private void UpdateNameDisplay(string name)
    {
        // 检查basePanel是否存在
        if (basePanel != null)
        {
            // 直接获取TextMeshProUGUI组件并设置文本
            basePanel.GetText_Legacy("Name").text = name;
        }
        else
        {
            Debug.LogWarning("BasePanel is not assigned!");
        }
    }

    /// <summary>
    /// 更新颜色显示
    /// </summary>
    /// <param name="color">要显示的颜色</param>
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

    /// <summary>
    /// 更新力量显示
    /// </summary>
    private void UpdateHpDisplay(int Hp)
    {
            // 获取力量显示文本组件并设置文本
            var strengthText = basePanel.GetText_Legacy("HP");
            if (strengthText != null)
            {
                strengthText.text = "HP:" + Hp;
            }
    }

    /// <summary>
    /// 更新攻击力显示
    /// </summary>
    private void UpdateAtkDisplay(int ATK)
    {
        var strengthText = basePanel.GetText_Legacy("ATK");
        if (strengthText != null)
        {
            strengthText.text = "ATK:" + ATK;
        }
    }

    #endregion

    #region 网络命令方法

    /// <summary>
    /// 更改玩家名称命令
    /// </summary>
    /// <param name="newName">新名称</param>
    [Command]
    public void CmdChangeName(string newName)
    {
        playerName = newName;
    }

    /// <summary>
    /// 更改玩家颜色命令
    /// </summary>
    /// <param name="newColor">新颜色</param>
    [Command]
    public void CmdChangeColor(Color newColor)
    {
        playerColor = newColor;
    }

    /// <summary>
    /// 增加玩家力量命令
    /// </summary>
    /// <param name="amount">增加的力量值</param>
    [Server]
    public void CmdAddHp(int amount)
    {
        hp += amount;

        if (hp <= 0)
        {
            hp = 0;
            CmdDie();
        }
    }
    public void ReportAddHp(int amount)
    {
        hp += amount;

        if (hp <= 0)
        {
            hp = 0;
            CmdDie();
        }
    }

    #endregion

    [Server]
    public void CmdDie()
    {
        Debug.Log($"玩家 {playerName} 死亡，进入失活状态");

        // 移除逻辑层引用（从 GameManager 活跃玩家列表中移除）
        GameManager.Instance.RemovePlayer(this);

        // 设置玩家为“失活”状态
        RpcSetInactive();

        // 禁止该玩家再参与碰撞、战斗等
        isInvincible = true;
        hp = 0;
    }
    [ClientRpc]
    private void RpcSetInactive()
    {
        // 禁用玩家外观、碰撞、输入控制等
        GetComponent<Collider2D>().enabled = false;

        var rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = false;

        // 变灰/半透明显示死亡状态
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0.2f);

        Debug.Log($"{playerName} 已死亡（客户端表现）");
    }


}