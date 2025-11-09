
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "New Item Data", menuName = "Item Data")]
public class ItemData : ScriptableObject
{
    public string Name;
    public string description;
    public GameObject itemPrefab;
    public bool CanStack = true;
}
