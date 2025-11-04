using Mirror;
using UnityEngine;

public class Attack : NetworkBehaviour
{
    #region Fields
    [Tooltip("用于标识玩家对象的标签")]
    [SerializeField] protected string tagName = "Player";

    public PlayerData playerData;

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        playerData = GetComponent<PlayerData>();
    }

    public virtual void OnCollisionEnter2D(Collision2D collision)
    {
        // 确保是本地玩家触发的逻辑
        if (!isLocalPlayer) return;
        
        if (collision.gameObject.CompareTag(tagName))
        {
            return;
        }

        PlayerData otherPlayer = collision.gameObject.GetComponent<PlayerData>();
        if (otherPlayer != null)
        {
            // 调用自己的 Command，让服务器来处理战斗逻辑
            CmdProcessCombat(otherPlayer);
        }
    }

    #endregion

    #region Commands

    /// <summary>
    /// 处理战斗逻辑的命令
    /// </summary>
    /// <param name="otherPlayer">被攻击的玩家数据组件</param>
    [Command]
    protected virtual void CmdProcessCombat(PlayerData otherPlayer)
    {
        Debug.Log($"{gameObject.name} 攻击了 {otherPlayer.playerName}");
        otherPlayer.CmdAddHp(-playerData.attack);
    }

    #endregion
}