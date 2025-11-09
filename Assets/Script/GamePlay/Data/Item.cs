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

    /// <summary>
    /// 重写ToString方法，返回物品名称和数量信息
    /// </summary>
    /// <returns>物品名称和数量的字符串表示</returns>
    public override string ToString()
    {
        return $"{itemData.Name} * {count}";
    }
}