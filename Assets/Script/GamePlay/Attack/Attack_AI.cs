using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack_AI : Attack
{
    public override void OnCollisionEnter2D(Collision2D collision)
    {
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

    protected  override void CmdProcessCombat(PlayerData otherPlayer)
    {
        Debug.Log($"{gameObject.name} 攻击了 {otherPlayer.playerName}");
        otherPlayer.ReportAddHp(-playerData.attack);
    }
}
