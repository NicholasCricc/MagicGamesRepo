using UnityEngine;
using System.Collections.Generic;

public class ItemChanger : MonoBehaviour
{
    [Header("List of Items")]
    public List<GameObject> itemList;
    private int currentIndex = -1;
    private GameObject currentRodItem;

    void Start()
    {
        if (itemList == null || itemList.Count == 0)
        {
            Debug.LogError("ItemChanger: itemList is empty! Add items in the Inspector.");
            return;
        }

        foreach (GameObject item in itemList)
        {
            item.SetActive(false);
        }

        ChangeToNextItem();
    }

    public void ChangeToNextItem()
    {
        if (itemList == null || itemList.Count == 0)
        {
            Debug.LogError("❌ itemList is empty in ChangeToNextItem()");
            return;
        }

        GameObject previousItem = (currentIndex >= 0 && currentIndex < itemList.Count) ? itemList[currentIndex] : null;

        int attempts = itemList.Count;
        int startIndex = currentIndex;
        bool found = false;

        for (int i = 0; i < attempts; i++)
        {
            currentIndex = (currentIndex + 1) % itemList.Count;

            if (!IsItemInDropZone(itemList[currentIndex]))
            {
                found = true;
                break;
            }
        }

        if (!found)
        {
            Debug.LogWarning("⚠️ No available items to cycle through.");
            return;
        }

        if (previousItem != null && !IsItemInDropZone(previousItem))
        {
            previousItem.SetActive(false);
            previousItem.GetComponent<Collider2D>().enabled = false;
            previousItem.GetComponent<DraggableItem>().enabled = false;
        }

        GameObject nextItem = itemList[currentIndex];

        EnableItemInteraction(nextItem);
        currentRodItem = nextItem;

        DraggableItem draggable = nextItem.GetComponent<DraggableItem>();
        if (draggable != null)
        {
            draggable.CacheStartPosition();
        }

        Debug.Log($"🔹 {nextItem.name} is now active at {nextItem.transform.position}");
    }

    public void EnableItemInteraction(GameObject item)
    {
        if (item == null) return;

        item.SetActive(true);
        Collider2D collider = item.GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
            collider.enabled = true;
        }

        DraggableItem draggable = item.GetComponent<DraggableItem>();
        if (draggable != null)
        {
            draggable.CacheStartPosition();
            draggable.enabled = false;
            draggable.enabled = true;
        }
    }

    private bool IsItemInDropZone(GameObject item)
    {
        Collider2D collider = item.GetComponent<Collider2D>();
        if (collider != null)
        {
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(item.transform.position, 0.5f);
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag("DropZone_Head") || hitCollider.CompareTag("DropZone_Body") ||
                    hitCollider.CompareTag("DropZone_Legs") || hitCollider.CompareTag("DropZone_Feet"))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void MarkItemAsPlaced(GameObject placedItem)
    {
ClothingItem placedClothing = placedItem.GetComponent<ClothingItem>();
if (itemList.Contains(placedItem) && placedClothing != null)
{
    itemList.Remove(placedItem);
    Debug.Log($"✅ {placedItem.name} marked as placed and removed from itemList.");
}

    }

    public bool HasAvailableItems()
    {
        return itemList != null && itemList.Count > 0;
    }

    public void ResetIndex()
    {
        currentIndex = -1;
    }

    public void ShowNextAvailableItem()
    {
        ChangeToNextItem();
    }

    public bool CompareCurrentRodItem(GameObject item)
    {
        return currentRodItem == item;
    }

    public void ClearCurrentRodItem()
    {
        currentRodItem = null;
    }

public void SetCurrentRodItem(GameObject item)
{
    if (currentRodItem != null && currentRodItem != item)
    {
        if (!IsItemInDropZone(currentRodItem))
        {
            currentRodItem.SetActive(false);
            Collider2D col = currentRodItem.GetComponent<Collider2D>();
            if (col) col.enabled = false;

            DraggableItem drag = currentRodItem.GetComponent<DraggableItem>();
            if (drag) drag.enabled = false;

            Debug.Log($"🧹 Previous rod item {currentRodItem.name} fully disabled.");
        }
    }

    currentRodItem = item;
    Debug.Log($"📌 New rod item set: {item.name}");
}

}
