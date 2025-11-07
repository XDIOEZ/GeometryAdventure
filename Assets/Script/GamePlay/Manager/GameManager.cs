using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GameManager : MonoBehaviour
{
    #region 字段定义
    
    /// <summary>
    /// 玩家列表，存储所有已连接的玩家
    /// </summary>
    [Tooltip("当前游戏中的所有玩家列表")]
    [ShowInInspector]
    public List<EntityData> players = new List<EntityData>();
    
    #endregion

    #region 属性
    
    /// <summary>
    /// 获取游戏管理器的单例实例
    /// </summary>
    public static GameManager Instance { get; private set; }
    
    #endregion

    #region Unity生命周期方法
    
    private void Awake()
    {
        // 确保只有一个GameManager实例
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    #endregion

    #region 玩家管理方法
    
    /// <summary>
    /// 当玩家进入游戏时调用
    /// </summary>
    /// <param name="player">进入游戏的玩家数据</param>
    public void AddPlayer(EntityData player)
    {
        if (!players.Contains(player))
        {
            players.Add(player);
            Debug.Log($"玩家 {player.playerName} 已加入游戏。当前玩家数量: {players.Count}");
        }
    }
    
/// <summary>
/// 当玩家离开游戏时调用
/// </summary>
/// <param name="player">离开游戏的玩家数据</param>
public void RemovePlayer(EntityData player)
{
    if (players.Contains(player))
    {
        players.Remove(player);
        Debug.Log($"玩家 {player.playerName} 已离开游戏。当前玩家数量: {players.Count}");
    }
}
    
    /// <summary>
    /// 获取当前所有玩家的数量
    /// </summary>
    /// <returns>玩家数量</returns>
    public int GetPlayerCount()
    {
        return players.Count;
    }
    
    /// <summary>
    /// 获取指定索引的玩家
    /// </summary>
    /// <param name="index">玩家索引</param>
    /// <returns>玩家数据</returns>
    public EntityData GetPlayer(int index)
    {
        if (index >= 0 && index < players.Count)
        {
            return players[index];
        }
        return null;
    }
    
    /// <summary>
    /// 查找具有指定名称的玩家
    /// </summary>
    /// <param name="name">玩家名称</param>
    /// <returns>玩家数据</returns>
    public EntityData FindPlayerByName(string name)
    {
        foreach (EntityData player in players)
        {
            if (player.playerName == name)
            {
                return player;
            }
        }
        return null;
    }
    
    #endregion
}