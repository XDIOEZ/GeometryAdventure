using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConId : NetworkBehaviour
{
    [SyncVar]
    public int myConnectionId = -1;  // 服务器同步的真实ID
    [Tooltip("客户端自己的ID,可自定义哦")]
    public int ID;  // 客户端自己的ID

    public override void OnStartServer()
    {
        base.OnStartServer();
        // 🔥 服务器端记录真实的 connectionId
        if (connectionToClient != null)
        {
            myConnectionId = connectionToClient.connectionId;
        }
    }
/*
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        GetClientID();              // 客户端生成随机 ID
        CmdUpdateServerRegisterPlayer(ID); // 发送给服务器
    }

    [Command]
    void CmdUpdateServerRegisterPlayer(int id)
    {
        myConnectionId = id;        // 服务器更新 SyncVar
    }


    public void GetClientID()
    {
        // Key 用来存储 ID
        string key = "ClientID";

        if (PlayerPrefs.HasKey(key))
        {
            // 读取已有 ID
            ID = PlayerPrefs.GetInt(key);
            Debug.Log("读取已有 ClientID: " + ID);
        }
        else
        {
            // 没有就生成新的随机 ID
            ID = Random.Range(1, 1000000);
            PlayerPrefs.SetInt(key, ID);
            PlayerPrefs.Save(); // 保存到磁盘
            Debug.Log("生成新的 ClientID 并保存: " + ID);
        }
    }*/


}
