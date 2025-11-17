using System.Collections;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

/// <summary>
/// 处理玩家进入下一关卡的触发器
/// </summary>
public class NextLevel : NetworkBehaviour
{
    [Tooltip("要加载的下一个场景名称")]
    public string nextSceneName;
    
    [Tooltip("切换场景前的延迟时间（秒）")]
    public float loadDelay = 1.0f;

    /// <summary>
    /// 当玩家进入触发器时尝试加载下一个场景
    /// </summary>
    /// <param name="other">进入触发器的碰撞体</param>
    public void OnTriggerEnter2D(Collider2D other)
    {
        // 检查进入的对象是否是玩家
        if (other.CompareTag("Player"))
        {
            // 只在服务器端执行场景切换
            if (isServer)
            {
                SaveManager.Instance.SaveAllData();
                StartCoroutine(LoadNextScene());
            }
        }
    }

    /// <summary>
    /// 延迟加载下一个场景
    /// </summary>
    /// <returns>协程枚举器</returns>
    private IEnumerator LoadNextScene()
    {
        // 通知所有客户端准备加载场景
        RpcPrepareForSceneTransition();
        
        // 等待指定的延迟时间
        yield return new WaitForSeconds(loadDelay);

        // 服务器加载新场景
        ServerChangeScene(nextSceneName);
    }

    /// <summary>
    /// 在所有客户端上准备场景过渡
    /// </summary>
    [ClientRpc]
    private void RpcPrepareForSceneTransition()
    {
        // 这里可以添加过渡动画或UI提示
        Debug.Log("准备加载下一个场景...");
    }

    /// <summary>
    /// 服务器更改场景
    /// </summary>
    /// <param name="sceneName">要加载的场景名称</param>
    [Server]
    private void ServerChangeScene(string sceneName)
    {
        string targetSceneName = sceneName;
        // 如果sceneName为空，则自动切换到当前场景索引+1的场景
        if (string.IsNullOrEmpty(sceneName))
        {
            // 获取当前场景索引
            Scene currentScene = SceneManager.GetActiveScene();
            int currentSceneIndex = currentScene.buildIndex;
            
            // 计算下一个场景索引
            int nextSceneIndex = currentSceneIndex + 1;
            
            // 检查下一个场景索引是否超出范围
            if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                targetSceneName = SceneUtility.GetScenePathByBuildIndex(nextSceneIndex);
                // 提取场景名称（去掉路径和扩展名）
                targetSceneName = System.IO.Path.GetFileNameWithoutExtension(targetSceneName);
            }
            else
            {
                Debug.LogWarning("下一个场景索引超出构建设置范围，将循环到第一个场景");
                targetSceneName = SceneUtility.GetScenePathByBuildIndex(0);
                targetSceneName = System.IO.Path.GetFileNameWithoutExtension(targetSceneName);
            }
        }
        
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            // 使用Mirror的NetworkManager来切换场景
            NetworkManager.singleton.ServerChangeScene(targetSceneName);
        }
        else
        {
            Debug.LogError("无法确定要加载的有效场景名称");
        }
    }
}