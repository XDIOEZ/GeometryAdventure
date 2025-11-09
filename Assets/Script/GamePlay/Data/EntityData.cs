using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EntityData : NetworkBehaviour
{
    #region 字段定义

    [SyncVar(hook = nameof(OnNameChanged))]
    public string playerName = "Entity";

    [SyncVar(hook = nameof(OnColorChanged))]
    public Color playerColor = Color.white;

    [SyncVar(hook = nameof(OnHpChanged))]
    public int hp = 10;

    [SyncVar(hook = nameof(OnAttackChanged))]
    [Tooltip("玩家攻击力")]
    public int attack = 1;

    [SyncVar(hook = nameof(OnSpeedChanged))]
    [Tooltip("玩家移动速度")]
    public float speed = 5.0f;

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
        UpdateSpeedDisplay(speed);
        GameManager.Instance.AddPlayer(this);
    }

    protected override void OnValidate()
    {
        basePanel = GetComponentInChildren<BasePanel>();
        if (basePanel == null)
        {
            return;
        }
        basePanel.CollectUIComponents();
        UpdateNameDisplay(playerName);
        UpdateColorDisplay(playerColor);
        UpdateHpDisplay(hp);
        UpdateAtkDisplay(attack);
        UpdateSpeedDisplay(speed);
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

    public void OnHpChanged(int oldHp, int newHp)
    {
        UpdateHpDisplay(newHp);
        if (isServer && newHp <= 0)
        {
            ServerDie();
        }
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
    /// 玩家速度变更回调
    /// </summary>
    /// <param name="oldSpeed">旧速度</param>
    /// <param name="newSpeed">新速度</param>
    public void OnSpeedChanged(float oldSpeed, float newSpeed)
    {
        UpdateSpeedDisplay(newSpeed);
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

    public void UpdateNameDisplay(string name)
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

  
    private void UpdateHpDisplay(int Hp)
    {
        // 获取力量显示文本组件并设置文本
         var strengthText = basePanel.GetText_Legacy("HP");
         if (strengthText != null)
         {
             strengthText.text = "HP:" + Hp;
        }

        if(Hp <= 0)
        {
           Die();
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

    /// <summary>
    /// 更新速度显示
    /// </summary>
    private void UpdateSpeedDisplay(float Speed)
    {
        var speedText = basePanel.GetText_Legacy("SPEED");
        if (speedText != null)
        {
            speedText.text = "SPD:" + Speed.ToString("F1");
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
    /// 更改玩家速度命令
    /// </summary>
    /// <param name="newSpeed">新速度</param>
    [Command]
    public void CmdChangeSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    [Command]
    public void CmdTakeDamage(int damage)
    {
        if (isInvincible || hp <= 0)
            return;

        hp -= damage;

        if (hp <= 0)
        {
            ServerDie();
        }
    }

    [Server]
    public void TakeDamage(int damage)
    {
        if (isInvincible) return;

        hp -= damage;

        if (hp <= 0)
        {
            ServerDie();
        }
    }


    #endregion

    public void Die()
    {
        Debug.Log($"玩家 {playerName} 死亡（客户端表现）");
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0.2f);

        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        var rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = false;
        var cam = GetComponentInChildren<Camera>();
        if (cam != null) cam.enabled = false;
    }

    [Server]
    public void ServerDie()
    {
        if (hp <= 0)
        {
            hp = 0;
            isInvincible = true;
            RpcOnDie(); // 广播给所有客户端执行死亡表现
            GameManager.Instance.RemovePlayer(this);
        }
    }

    [ClientRpc]
    void RpcOnDie()
    {
        Die(); // 客户端执行视觉表现
    }

}