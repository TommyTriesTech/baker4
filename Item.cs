using System;
using UnityEngine;

public class Item : MonoBehaviour, IInteractable
{
    public ItemSO itemSO;

    public void Interact()
    {
        GameServices.EventManagerService.ItemPickedUp(this);
    }

    public ItemSO GetItemSO()
    {
        return itemSO;
    }
}