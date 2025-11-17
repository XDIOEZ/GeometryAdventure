using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 玩家背包系统，管理物品槽位和物品拾取
/// </summary>
public class Inventory : NetworkBehaviour,ISaveLoad
{
    #region 字段和属性

    [Tooltip("物品槽位列表")]
    public List<ItemSlot> itemSlots = new();

    [SyncVar(hook = nameof(UpdateInventoryUI))]
    [Tooltip("当前选中的物品索引")]
    public int CurrentIndex = 0;

    [Tooltip("基础面板引用")]
    public BasePanel basePanel;

    public ConId conId;

    #endregion

    #region Unity生命周期



    public void Awake()
    {
        // 🔥 提前初始化，避免反序列化时为空
        basePanel = transform.parent?.GetComponentInChildren<BasePanel>();
    }
    /// <summary>
    /// 初始化背包系统，设置输入控制监听
    /// </summary>
    public void Start()
    {
      //  conId = GetComponentInParent<ConId>();
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

    public override void OnStartLocalPlayer()
    {
        CmdLoadData(GetId());
    }

    [Command]
    public void CmdLoadData(string id)
    {
        // 注册
        SaveManager.instance.RegisterSaveObject(this);

        // 加载
        var data = SaveManager.instance.LoadData(id);
        if (data == null)
        {
            Debug.Log("Inventory: No save data found.");
            return;
        }
        RpcLoadData(data);
    }

    [ClientRpc]
    public void RpcLoadData(string[] data)
    {
        Load(data);
        UpdateInventoryUI();
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

    public void UpdateInventoryUI(int oldCurrentIndex, int newCurrentIndex)
    {
        // 🔒 安全防护：确保数据有效
        if (basePanel == null || itemSlots == null || itemSlots.Count == 0)
            return;

        // 🔒 确保 newIndex 合法
        if (newCurrentIndex < 0 || newCurrentIndex >= itemSlots.Count)
            return;

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
    #region ISaveLoad接口

    public string[] Save()
    {
        string[] data = new string[2];
        data[0] = ItemSlot.SaveAll(itemSlots);
        data[1] = CurrentIndex.ToString();
        return data;
    }

    public void Load(string[] data)
    {
        itemSlots = ItemSlot.LoadAll(data[0]);
        CurrentIndex = int.Parse(data[1]);
    }
    public string GetId()
    {
        return $"{conId.myConnectionId}_Inventory";
    }

/*    public string GetId()
    {
        //TODO 获取父对象上的网络组件的ID
        string id  = NetworkClient.connection.connectionId.ToString();
        id+= "_Inventory";
        return id;
    }*/
    #endregion

}