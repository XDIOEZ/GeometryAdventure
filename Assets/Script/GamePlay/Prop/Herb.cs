using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Herb : MonoBehaviour
{
    [Tooltip("使用后恢复的生命值数量")]
    public int healthRestoreAmount = 5; // 默认恢复5点生命值，可在Inspector中配置

    // 当与其他碰撞体发生碰撞时调用
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 尝试获取碰撞对象上的EntityData组件
        EntityData playerEntity = collision.gameObject.GetComponent<EntityData>();
        
        // 检查是否成功获取到EntityData组件
        if (playerEntity != null)
        {
            // 恢复玩家生命值
            playerEntity.hp += healthRestoreAmount;
            
            // 确保生命值不会超过合理范围（可选，取决于游戏设计）
            // playerEntity.hp = Mathf.Min(playerEntity.hp, playerEntity.maxHp);
            
            // 记录日志
            Debug.Log($"草药被使用，{playerEntity.playerName}恢复了{healthRestoreAmount}点生命值");
            
            // 销毁草药道具
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // 可以在这里添加初始化逻辑
    }

    // Update is called once per frame
    void Update()
    {
        // 可以在这里添加每帧更新逻辑
    }
}