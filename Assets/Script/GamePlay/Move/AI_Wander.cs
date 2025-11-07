using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class AI_Wander : MonoBehaviour
{
    [Tooltip("AI 的闲逛点（可为空，如果为空则自动从子物体中加载）")]
    public List<Transform> waypoints = new List<Transform>();

    [Tooltip("移动速度")]
    public float moveSpeed
    {
        get
        {
           return playerData.speed;
        }
        set
        {
            playerData.speed = value;
        }
    }
    
    [Tooltip("随机速度偏移量 表示 moveSpeed 会在下面的范围内随机加减一个随机的值")]
    public Vector2 RandomSpeedOffset = new Vector2(-1, 1);


    public EntityData playerData;

    [Tooltip("到达目标点的判定距离")]
    public float arriveDistance = 0.1f;

    private Rigidbody2D rb;
    private int currentWaypointIndex = 0;
    private float currentRandomSpeed = 0f;

    // 额外维护一份世界坐标的副本
    private List<Vector2> waypointPositions = new List<Vector2>();

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerData = GetComponent<EntityData>();
        // 如果没有手动设置闲逛点，则自动从子物体中获取
        if (waypoints.Count == 0)
        {
            foreach (Transform child in transform)
            {
                waypoints.Add(child);
            }
        }

        if (waypoints.Count == 0)
        {
            Debug.LogWarning("未找到任何闲逛点，请在对象下创建子物体作为巡逻点！");
            enabled = false;
            return;
        }

        // 记录每个闲逛点的世界坐标，避免被父对象移动影响
        waypointPositions.Clear();
        foreach (var wp in waypoints)
        {
            waypointPositions.Add(wp.position);
        }

        // 找出距离AI最近的闲逛点
        float closestDistance = Mathf.Infinity;
        int closestIndex = 0;
        for (int i = 0; i < waypointPositions.Count; i++)
        {
            float distance = Vector2.Distance(transform.position, waypointPositions[i]);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }

        currentWaypointIndex = closestIndex;
        // 到达目标点后重新生成随机速度
        /*GenerateRandomSpeed();*/
        currentRandomSpeed = moveSpeed;
    }


    void FixedUpdate()
    {
        if (waypointPositions.Count == 0) return;

        Vector2 currentPos = rb.position;
        Vector2 targetPos = waypointPositions[currentWaypointIndex];

        // 只移动X轴（横版游戏）
        Vector2 newPos = new Vector2(targetPos.x, currentPos.y);
        float direction = Mathf.Sign(newPos.x - currentPos.x);

        // 移动 使用带随机偏移的速度
        rb.velocity = new Vector2(direction * currentRandomSpeed, rb.velocity.y);

        // 到达目标点判定
        if (Vector2.Distance(currentPos, targetPos) <= arriveDistance)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            currentWaypointIndex = (currentWaypointIndex + 1) % waypointPositions.Count;
        }
    }
    
#region 工具方法

/// <summary>
/// 生成一个新的随机速度值
/// </summary>
private void GenerateRandomSpeed()
{
    // 如果有PlayerData组件且是服务器，则在服务器端生成随机数并同步
    if (playerData != null && playerData.isServer)
    {
        float randomOffset = Random.Range(RandomSpeedOffset.x, RandomSpeedOffset.y);
        currentRandomSpeed = moveSpeed + randomOffset;
        // 同步到所有客户端
        playerData.CmdChangeSpeed(currentRandomSpeed);
    }
    else if (playerData != null)
    {
        // 客户端直接使用PlayerData中的speed值
        currentRandomSpeed = playerData.speed;
    }
    else
    {
        // 没有PlayerData时使用本地随机数（用于单机模式）
        currentRandomSpeed = moveSpeed + Random.Range(RandomSpeedOffset.x, RandomSpeedOffset.y);
    }
    
    // 确保速度不会变成负数或零
    currentRandomSpeed = Mathf.Max(0.1f, currentRandomSpeed);
}

#endregion
    

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;

        // 如果在运行时，且我们已经生成了 waypointPositions
        if (waypointPositions != null && waypointPositions.Count > 0)
        {
            for (int i = 0; i < waypointPositions.Count; i++)
            {
                Gizmos.DrawSphere(waypointPositions[i], 0.1f);

                if (i + 1 < waypointPositions.Count)
                    Gizmos.DrawLine(waypointPositions[i], waypointPositions[i + 1]);
            }
        }
        // 如果在编辑器中（未运行），使用 waypoints 的位置绘制
        else if (waypoints != null && waypoints.Count > 0)
        {
            for (int i = 0; i < waypoints.Count; i++)
            {
                if (waypoints[i] != null)
                {
                    Gizmos.DrawSphere(waypoints[i].position, 0.1f);

                    if (i + 1 < waypoints.Count && waypoints[i + 1] != null)
                        Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                }
            }
        }
    }
}