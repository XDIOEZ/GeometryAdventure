using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 玩家背包系统，管理物品槽位和物品拾取
/// </summary>
public class Inventory : NetworkBehaviour
{
    #region 字段和属性

    [Tooltip("物品槽位列表")]
    public List<ItemSlot> itemSlots = new();

    [SyncVar(hook = nameof(UpdateInventoryUI))]
    [Tooltip("当前选中的物品索引")]
    public int CurrentIndex = 0;

    [Tooltip("基础面板引用")]
    public BasePanel basePanel;

    #endregion

    #region Unity生命周期

    /// <summary>
    /// 初始化背包系统，设置输入控制监听
    /// </summary>
    public void Start()
    {
        basePanel = transform.parent.GetComponentInChildren<BasePanel>();

        if (!isLocalPlayer)
        {
            return;
        }

        Controller_PlayerInput controller = GetComponentInParent<Controller_PlayerInput>();
        controller.InputActionAsset.Player.ChangeSlot.performed += ctx => {
            // 读取float值并根据正负判断方向
            float value = ctx.ReadValue<float>();
            if (value > 0)
            {
                ChangeIndex(CurrentIndex + 1);
            }
            else if (value < 0)
            {
                ChangeIndex(CurrentIndex - 1);
            }
        };
    }


    /// <summary>
    /// 处理物品碰撞拾取
    /// </summary>
    /// <param name="Collider">碰撞体</param>
    public void OnTriggerEnter2D(Collider2D Collider)
    {
        if (Collider.gameObject.GetComponentInChildren<ItemPickable>() != null)
        {
            // 拾取到物品,调用ItemPickable的Pickup方法

            Item item = Collider.gameObject.GetComponentInChildren<ItemPickable>().item;
        
            if (isLocalPlayer)
            {
                CmdPickUp(item);
            }
        }
    }

    #endregion

    #region 物品管理

    /// <summary>
    /// 添加物品到背包
    /// </summary>
    /// <param name="itemSlot">要添加的物品槽</param>
    public void AddItem(ItemSlot itemSlot)
    {
        ItemData itemData = itemSlot.itemData;
        // 判断是否已经有同名物品,有的话就添加数量
        for (int i = 0; i < itemSlots.Count; i++)
        {
            if (itemSlots[i].itemData.Name == itemData.Name)
            {
                itemSlots[i].count += itemSlot.count;
                return;
            }
        }
        // 没有同名物品,直接添加
        itemSlots.Add(itemSlot);
    }

    /// <summary>
    /// 拾取物品的客户端命令
    /// </summary>
    /// <param name="item">要拾取的物品</param>
    [Command]
    public void CmdPickUp(Item item)
    {
        RpcPickUp(item);
    }

    /// <summary>
    /// 拾取物品的客户端RPC调用
    /// </summary>
    /// <param name="item">被拾取的物品</param>
    [ClientRpc]
    public void RpcPickUp(Item item)
    {
        if (item == null || item.itemSlot == null) return;

        AddItem(item.itemSlot);
        UpdateInventoryUI();

        if (isServer && item.gameObject != null)
            NetworkServer.Destroy(item.gameObject);
    }



    #endregion

    #region 索引管理

    public void ChangeIndex(int index)
    {
        if (itemSlots == null || itemSlots.Count == 0) return;

        if (isLocalPlayer)
            CmdChangeIndex(((index % itemSlots.Count) + itemSlots.Count) % itemSlots.Count);
    }

    [Command]
    public void CmdChangeIndex(int index)
    {
        if (itemSlots == null || itemSlots.Count == 0) return;

        CurrentIndex = ((index % itemSlots.Count) + itemSlots.Count) % itemSlots.Count;
    }


    #endregion

    #region UI更新

    /// <summary>
    /// 更新背包UI显示
    /// </summary>
    /// <param name="oldCurrentIndex">旧索引</param>
    /// <param name="newCurrentIndex">新索引</param>
    public void UpdateInventoryUI(int oldCurrentIndex, int newCurrentIndex)
    {
        Text nameText = basePanel.GetText_Legacy("Name*Count");
        nameText.text = itemSlots[newCurrentIndex].ToString();
    }

    public void UpdateInventoryUI()
    {
        if (itemSlots == null || itemSlots.Count == 0 || basePanel == null)
            return; // 安全返回

        Text nameText = basePanel.GetText_Legacy("Name*Count");
        nameText.text = itemSlots[CurrentIndex].ToString();
    }


    #endregion
}