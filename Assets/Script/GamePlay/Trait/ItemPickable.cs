using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Item))]
public class ItemPickable : NetworkBehaviour
{
    public Item item;

    protected override void OnValidate()
    {
        item = GetComponent<Item>();
    }
}
