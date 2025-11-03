using Mirror;
using UnityEngine;

public class PlayerInjured : NetworkBehaviour
{
    public PlayerData playerData;

    private void Start()
    {
        playerData = GetComponent<PlayerData>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 确保是本地玩家触发的逻辑
        if (!isLocalPlayer) return;

        PlayerData otherPlayer = collision.gameObject.GetComponent<PlayerData>();
        if (otherPlayer != null)
        {
            // 调用自己的 Command，让服务器来处理战斗逻辑
            CmdProcessCombat(otherPlayer);
        }
    }

    [Command]
    private void CmdProcessCombat(PlayerData otherPlayer)
    {
        int otherPlayerStrength = otherPlayer.strength;
        // 注意：这个函数在服务器上执行
        Debug.Log($"{gameObject.name} 攻击了 {otherPlayer.playerName}");
        otherPlayer.CmdAddStrength(-playerData.strength);  // ⚠️ 这里不应该直接调用别人的 Cmd 方法
        playerData.CmdAddStrength(-otherPlayerStrength);  // ⚠️ 这里不应该直接调用别人的 Cmd 方法
    }
}
