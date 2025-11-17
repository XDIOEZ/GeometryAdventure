using Mirror;
using System;
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

[Serializable]
public class ItemSlot
{
    public ItemData itemData;
    public int count = 1;

    public ItemSlot(ItemData itemData, int count)
    {
        this.itemData = itemData;
        this.count = count;
    }

    public override string ToString()
    {
        return $"{itemData.Name} * {count}";
    }

    // -------------------------------
    // Save / Load 版本
    // -------------------------------

    [Serializable]
    private class DTO   // 简单的序列化结构
    {
        public string itemId;
        public int count;
    }

    /// <summary>
    /// 序列化为 JSON 字符串
    /// </summary>
    public string Save()
    {
        DTO dto = new DTO()
        {
            itemId = itemData.name,
            count = count
        };

        return JsonUtility.ToJson(dto);
    }

    /// <summary>
    /// 根据数据库反序列化回物品槽
    /// </summary>
    public void Load(string json)
    {
        DTO dto = JsonUtility.FromJson<DTO>(json);

        // 从 ItemDatabase 恢复真正的 ItemData
        itemData = ItemDatabase.Instance.GetItem(dto.itemId);
        count = dto.count;
    }
    
    /// <summary>
    /// 序列化所有物品槽为 JSON 字符串
    /// </summary>
    public static string SaveAll(List<ItemSlot> slots)
    {
        List<string> list = new();
        foreach (var slot in slots)
            list.Add(slot.Save());

        return Newtonsoft.Json.JsonConvert.SerializeObject(list);
    }
    
    /// <summary>
    /// 从 JSON 字符串反序列化出物品槽列表
    /// </summary>
    public static List<ItemSlot> LoadAll(string json)
    {
        List<ItemSlot> slots = new List<ItemSlot>();
        
        // 反序列化 JSON 字符串为字符串列表
        List<string> list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(json);
        
        // 遍历每个字符串并创建对应的 ItemSlot
        foreach (string itemJson in list)
        {
            ItemSlot slot = new ItemSlot(null, 0);
            slot.Load(itemJson);
            slots.Add(slot);
        }
        
        return slots;
    }
}