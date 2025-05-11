using UnityEngine;

[CreateAssetMenu()]
public class ItemSO : ScriptableObject
{
    public Transform prefab;
    public Sprite sprite;
    public string itemName;
    public bool isStackable = true;
    public int maxStackSize = 12;
}
