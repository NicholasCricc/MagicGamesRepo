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

        // ✅ Hide previous item before cycling
        if (previousItem != null && !IsItemInDropZone(previousItem))
        {
            previousItem.SetActive(false);
            previousItem.GetComponent<Collider2D>().enabled = false;
            previousItem.GetComponent<DraggableItem>().enabled = false;
            Debug.Log($"🔻 Hiding {previousItem.name} and DISABLING it.");
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
            Debug.Log($"✅ Collider refreshed and enabled for {item.name}");
        }

        DraggableItem draggable = item.GetComponent<DraggableItem>();
        if (draggable != null)
        {
            draggable.CacheStartPosition();
            draggable.enabled = false;
            draggable.enabled = true;
            Debug.Log($"✅ DraggableItem script refreshed for {item.name}");
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
        if (itemList.Contains(placedItem))
        {
            itemList.Remove(placedItem);
            Debug.Log($"✅ {placedItem.name} marked as placed and removed from itemList.");

            if (currentRodItem == placedItem)
            {
                currentRodItem = null;
                Debug.Log($"🧹 {placedItem.name} was currentRodItem — cleared reference.");
            }
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

    public void ShowNextAvailableItem(ClothingType categoryType)
    {
        if (itemList == null || itemList.Count == 0)
        {
            Debug.LogError("❌ itemList is empty in ShowNextAvailableItem()");
            return;
        }

        bool allPlaced = true;
        Debug.Log($"📋 Cycling for type: {categoryType}. Item count: {itemList.Count}");

        foreach (GameObject item in itemList)
        {
            ClothingItem clothing = item.GetComponent<ClothingItem>();
            Debug.Log($"🔍 {item.name} - Type: {(clothing != null ? clothing.clothingType.ToString() : "None")}, InDropZone: {IsItemInDropZone(item)}");
            if (clothing != null && clothing.clothingType == categoryType && !IsItemInDropZone(item))
            {
                allPlaced = false;
                break;
            }
        }

        if (allPlaced)
        {
            Debug.Log("🟣 All items of this type are placed. Nothing to show.");
            return;
        }

        int attempts = itemList.Count;
        int startIndex = currentIndex;

        do
        {
            currentIndex = (currentIndex + 1) % itemList.Count;
            attempts--;

            ClothingItem clothing = itemList[currentIndex].GetComponent<ClothingItem>();
            if (clothing != null && clothing.clothingType == categoryType && !IsItemInDropZone(itemList[currentIndex]))
            {
                break;
            }

            if (currentIndex == startIndex || attempts <= 0)
            {
                Debug.LogWarning("⚠️ No available items of this type to cycle through. Stopping.");
                return;
            }

        } while (true);

        if (currentIndex < 0 || currentIndex >= itemList.Count)
        {
            Debug.LogError("❌ currentIndex is out of range in ShowNextAvailableItem()");
            return;
        }

        if (currentRodItem != null)
        {
            currentRodItem.SetActive(false);
            currentRodItem.GetComponent<Collider2D>().enabled = false;
            currentRodItem.GetComponent<DraggableItem>().enabled = false;
            Debug.Log($"🔻 Cleaned up lingering rod item: {currentRodItem.name}");
            currentRodItem = null;
        }

        GameObject nextItem = itemList[currentIndex];

        EnableItemInteraction(nextItem);
        currentRodItem = nextItem;

        DraggableItem draggable = nextItem.GetComponent<DraggableItem>();
        if (draggable != null)
        {
            nextItem.transform.position = draggable.GetStartingPosition();
        }

        Debug.Log($"🔹 {nextItem.name} is now active at {nextItem.transform.position}");
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
                currentRodItem.GetComponent<Collider2D>().enabled = false;
                currentRodItem.GetComponent<DraggableItem>().enabled = false;
                Debug.Log($"🧹 Previous rod item {currentRodItem.name} deactivated.");
            }
            else
            {
                Debug.Log($"⚠️ {currentRodItem.name} is placed — not deactivating.");
            }
        }

        currentRodItem = item;
        Debug.Log($"📌 New rod item set: {item.name}");
    }
}
