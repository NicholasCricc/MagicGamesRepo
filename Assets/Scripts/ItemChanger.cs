using UnityEngine;
using System.Collections.Generic;

public class ItemChanger : MonoBehaviour
{
    [Header("List of Items")]
    public List<GameObject> itemList;
    private int currentIndex = -1;

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

        do
        {
            currentIndex = (currentIndex + 1) % itemList.Count;
            attempts--;

            // ✅ Prevent infinite loop
            if (currentIndex == startIndex)
            {
                Debug.LogWarning("⚠️ No available items to cycle through. Stopping.");
                return;
            }

        } while (IsItemInDropZone(itemList[currentIndex]) && attempts > 0);

        if (attempts <= 0)
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

        // ✅ Ensure only the next item is active
        EnableItemInteraction(nextItem);

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
            
            collider.enabled = false; // ✅ Temporarily disable collider
            collider.enabled = true;  // ✅ Reactivate collider to force Unity to detect clicks

            Debug.Log($"✅ Collider refreshed and enabled for {item.name}");
        }

        DraggableItem draggable = item.GetComponent<DraggableItem>();
        if (draggable != null)
        {
            draggable.CacheStartPosition();
            draggable.enabled = false;
            draggable.enabled = true; // ✅ Reactivate the script

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

    //public void RegisterItem(GameObject item)
    //{
    //    if (!itemList.Contains(item))
    //    {
    //        itemList.Add(item);

    //        // ✅ Ensure it is interactive again
    //        DropZone dropZone = Object.FindFirstObjectByType<DropZone>();
    //        if (dropZone != null)
    //        {
    //            dropZone.EnableItemInteraction(item);
    //        }

    //        Debug.Log($"🔄 {item.name} re-added and reactivated.");
    //    }
    //}

    public void MarkItemAsPlaced(GameObject placedItem)
    {
        if (itemList.Contains(placedItem))
        {
            itemList.Remove(placedItem); // ✅ Stop cycling this item
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
        if (itemList == null || itemList.Count == 0)
        {
            Debug.LogError("❌ itemList is empty in ShowNextAvailableItem()");
            return;
        }

        // ✅ Check if all items are already placed
        bool allPlaced = true;
        foreach (GameObject item in itemList)
        {
            if (!IsItemInDropZone(item))
            {
                allPlaced = false;
                break;
            }
        }

        if (allPlaced)
        {
            Debug.Log("🟣 All items are placed. Nothing to show.");
            return;
        }

        int attempts = itemList.Count;
        GameObject previousItem = (currentIndex >= 0 && currentIndex < itemList.Count) ? itemList[currentIndex] : null;

        int startIndex = currentIndex;

        do
        {
            currentIndex = (currentIndex + 1) % itemList.Count;
            attempts--;

            if (currentIndex == startIndex)
            {
                Debug.LogWarning("⚠️ No available items to cycle through. Stopping.");
                return;
            }

        } while (IsItemInDropZone(itemList[currentIndex]) && attempts > 0);

        if (currentIndex < 0 || currentIndex >= itemList.Count)
        {
            Debug.LogError("❌ currentIndex is out of range in ShowNextAvailableItem()");
            return;
        }

        // ✅ Hide previous item if it’s not already placed
        if (previousItem != null && !IsItemInDropZone(previousItem))
        {
            previousItem.SetActive(false);
            previousItem.GetComponent<Collider2D>().enabled = false;
            previousItem.GetComponent<DraggableItem>().enabled = false;
            Debug.Log($"🔻 Hiding {previousItem.name} and DISABLING it.");
        }

        GameObject nextItem = itemList[currentIndex];

        // ✅ Fully enable the next item
        EnableItemInteraction(nextItem);

        DraggableItem draggable = nextItem.GetComponent<DraggableItem>();
        if (draggable != null)
        {
            nextItem.transform.position = draggable.GetStartingPosition();
        }

        Debug.Log($"🔹 {nextItem.name} is now active at {nextItem.transform.position}");
    }

}
