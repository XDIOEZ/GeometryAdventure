using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller_Cam : NetworkBehaviour
{
    #region Unity生命周期

    // Start is called before the first frame update
    void Start()
    {
        if (isLocalPlayer)
        {
            Camera.main.enabled = false;
            // 获取子对象上的AudioListener组件并将其设置为启用状态
            AudioListener audioListener = GetComponentInChildren<AudioListener>();
            if (audioListener != null)
            {
                audioListener.enabled = true;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnStopClient()
    {
            Camera.main.enabled = true;
    }

    #endregion
}