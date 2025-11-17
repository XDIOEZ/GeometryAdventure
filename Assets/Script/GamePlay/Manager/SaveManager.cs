using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using Mirror;



public class SaveManager : NetworkBehaviour
{
    public static SaveManager instance;
    public static SaveManager Instance => instance;

    [Tooltip("保存的数据: Key为对象ID, Value为对象的数据数组")]
    [Sirenix.OdinInspector.ShowInInspector]
    public Dictionary<string, string[]> saveData = new();

    [Tooltip("当前注册的保存对象")]
    [Sirenix.OdinInspector.ShowInInspector]
    private List<ISaveLoad> saveObjects = new();

    private string savePath;
    public ItemDatabase itemDatabase;
    [Sirenix.OdinInspector.ShowInInspector]
    public int SaveDataCount => saveData.Count;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            itemDatabase.Init();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        savePath = Path.Combine(Application.persistentDataPath, "save.json");
    }

    private void Start()
    {
        // 游戏启动自动尝试加载
        LoadFromDisk();
    }


    public void RegisterSaveObject(ISaveLoad slObject)
    {
        if (!saveObjects.Contains(slObject))
        {
            saveObjects.Add(slObject);
            Debug.Log($"开始注册");
        }
    }

    // -----------------------------
    // 保存逻辑
    // -----------------------------
[Server]
public void SaveAllData()
{
    // 保存前清理为空的引用
    saveObjects.RemoveAll(obj => obj == null);
    
    // 创建临时列表来存储有效的对象
    var validObjects = new List<ISaveLoad>();
    
    foreach (var obj in saveObjects)
    {
        // 检查对象是否仍然有效
        try 
        {
            // 尝试访问对象的ID来验证它是否有效
            var id = obj.GetId();
            validObjects.Add(obj);
        }
        catch (MissingReferenceException)
        {
            Debug.LogWarning($"跳过已销毁的对象: {obj.GetType().Name}");
            continue;
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"检查对象有效性时出错: {ex.Message}");
            continue;
        }
    }
    
    // 只保存有效的对象
    foreach (var obj in validObjects)
    {
        try
        {
            saveData[obj.GetId()] = obj.Save();
        }
        catch (MissingReferenceException)
        {
            Debug.LogWarning($"保存时跳过已销毁的对象: {obj.GetType().Name}");
            continue;
        }
    }
    
    Debug.Log($"保存数据{saveData.Count}个对象");
}

    // -----------------------------
    // 加载逻辑
    // -----------------------------
    public void Load()
    {
        foreach (var obj in saveObjects)
        {
            string id = obj.GetId();

            if (saveData.ContainsKey(id))
            {
                obj.Load(saveData[id]);
            }
        }
    }

    // -----------------------------
    // 保存到硬盘（JSON）
    // -----------------------------
    public void SaveToDisk()
    {
        string json = JsonConvert.SerializeObject(saveData, Formatting.Indented);
        File.WriteAllText(savePath, json);
        Debug.Log($"保存成功：{savePath}");
    }

    // -----------------------------
    // 从硬盘加载
    // -----------------------------
    public void LoadFromDisk()
    {
        if (!File.Exists(savePath))
        {
            Debug.Log("没有找到存档");
            return;
        }

        string json = File.ReadAllText(savePath);
        saveData = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(json);
        Debug.Log("存档加载成功");
    }
    public string[] LoadData(string id)
    {
        if (!saveData.ContainsKey(id))
        {
            Debug.Log($"没有找到存档数据:{id}");
            Debug.Log($"存档数据数量：{saveData.Count}");
            return null;
        }
        Debug.Log($"加载存档数据:{id}");
        Debug.Log($"头数据：{saveData[id][0]}");

        return saveData[id];
    }
}
