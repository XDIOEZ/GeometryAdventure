using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack_AI : NetworkBehaviour
{
    [Header("攻击通用设置")]
    [SerializeField] protected string AvoidTagName = "Player";
    [Tooltip("指定攻击检测的图层")]
    [SerializeField] private LayerMask targetLayer = 1 << 6; // 默认为Monster图层（假设为第6层）
    public EntityData Data;
    
    void Start()
    {
        Data = GetComponent<EntityData>();
        AvoidTagName = Data.gameObject.tag;
    }

    public void OnCollisionStay2D(Collision2D collision)
    {
        if (!isServer) return; // ✅ 确保只有服务器处理攻击

        // 检查碰撞对象是否在指定图层
        if ((targetLayer & (1 << collision.gameObject.layer)) == 0)
            return;

        if (collision.gameObject.CompareTag(AvoidTagName))
            return;

        HP otherPlayer = collision.gameObject.GetComponent<HP>();
        if (otherPlayer != null)
        {
            otherPlayer.TakeDamage(Data.attack);
        }
    }
}