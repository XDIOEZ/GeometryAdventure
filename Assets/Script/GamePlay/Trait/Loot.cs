using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loot : NetworkBehaviour
{
    //TOOD 只有服务器启动后,监听父对象上的HP变化事件
    public List<LootData> lootDatas = new();

    public void Start()
    {
        if (isServer)
        {
            EntityData entityData = GetComponent<EntityData>();
            entityData.onHPChanged += OnHPChanged;
        }
    }

    public void OnHPChanged(int hp)
    {
        if (hp <= 0)
        {
            DropLoot();
        }
    }
    [Server]
    public void DropLoot()
    {
        foreach (LootData lootData in lootDatas)
        {
            for (int i = 0; i < lootData.count; i++)
            {
                GameObject item = Instantiate(lootData.itemData.itemPrefab, transform.position, Quaternion.identity);
                NetworkServer.Spawn(item);
            }
        }
    }

}

[System.Serializable]
public class LootData
{
    public ItemData itemData;
    public int count;
}