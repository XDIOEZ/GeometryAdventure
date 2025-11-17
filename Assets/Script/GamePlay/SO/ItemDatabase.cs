using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Item/ItemDatabase")]
public class ItemDatabase : ScriptableObject
{
    public List<ItemData> allItems;

    private static ItemDatabase instance;
    public static ItemDatabase Instance => instance;

    private Dictionary<string, ItemData> dict;

    public void Init()
    {
        dict = new();

        foreach (var item in allItems)
            dict[item.name] = item;

        instance = this;
    }

    public ItemData GetItem(string id)
    {
        return dict[id];
    }

    /// <summary>
    /// 自动获取所有ItemData资源
    /// </summary>
    [ContextMenu("自动获取所有物品数据")]
    public void AutoCollectAllItems()
    {
#if UNITY_EDITOR
        allItems = new List<ItemData>();

        // 查找所有ItemData类型的资源
        ItemData[] items = Resources.FindObjectsOfTypeAll<ItemData>();
        foreach (var item in items)
        {
            // 确保只添加Asset中的资源，而不是场景中的对象
            if (EditorUtility.IsPersistent(item))
            {
                allItems.Add(item);
            }
        }
        
        Debug.Log($"自动收集了 {allItems.Count} 个物品数据");
        EditorUtility.SetDirty(this);
#endif
    }
}