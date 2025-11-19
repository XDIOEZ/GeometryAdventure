using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Task : MonoBehaviour
{
    public virtual void StartTask()
    {
        // 任务开始时的逻辑
    }
    public virtual void CompleteTask()
    {
        // 任务完成时的逻辑
    }
}
