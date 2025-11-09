using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : NetworkBehaviour
{
    public List<ItemSlot> itemSlots = new();

    public void OnTriggerEnter2D(Collider2D Collider)
    {
        if (Collider.gameObject.GetComponentInChildren<ItemPickable>() != null)
        {
            //拾取到物品,调用ItemPickable的Pickup方法
           
            Item item = Collider.gameObject.GetComponentInChildren<ItemPickable>().item;
            CmdPickUp(item);
        }
    }

    [Command]
    public void CmdPickUp(Item item)
    {
        PickUp(item);
    }

    [ClientRpc]
    public void PickUp(Item item)
    {
        AddItem(item);
        Destroy(item.gameObject);
    }

    public void AddItem(Item Item)
    {
        ItemData itemData = Item.itemSlot.itemData;
        //判断是否已经有同名物品,有的话就添加数量
        for (int i = 0; i < itemSlots.Count; i++)
        {
            if (itemSlots[i].itemData.Name == itemData.Name)
            {
                itemSlots[i].count += Item.itemSlot.count;
                return;
            }
        }
        //没有同名物品,直接添加
        itemSlots.Add(Item.itemSlot);
    }
}
