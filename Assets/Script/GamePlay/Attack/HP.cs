using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HP : MonoBehaviour
{
    public EntityData data;
    [Tooltip("最小受伤间隔时间")]
    public float MinTakeDamageTime = 0.5f;
    public float lastTakeDamageTime;

    public void TakeDamage(int damage)
    {
        if (Time.time - lastTakeDamageTime >= MinTakeDamageTime)
        {
            lastTakeDamageTime = Time.time;

            // 只有服务器有权修改数据
            if (data.isServer)
            {
                data.TakeDamage(damage);
            }
        }
    }

}