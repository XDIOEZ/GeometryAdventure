using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : NetworkBehaviour
{
    public ItemSlot itemSlot;
    protected override void OnValidate()
    {
        BasePanel basePanel = GetComponentInChildren<BasePanel>();
        basePanel.CollectUIComponents();
        basePanel.GetText_Legacy("Name").text = itemSlot.itemData.Name;
    }
}
[System.Serializable]
public class ItemSlot
{
    public ItemData itemData;
    public int count = 1;

    public ItemSlot(ItemData itemData, int count)
    {
        this.itemData = itemData;
        this.count = count;
    }
}
