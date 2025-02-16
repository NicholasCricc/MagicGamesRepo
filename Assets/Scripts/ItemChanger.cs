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

        // Deactivate all items at the start
        foreach (GameObject item in itemList)
        {
            item.SetActive(false);
        }

        ChangeToNextItem();
    }

public void ChangeToNextItem()
{
    int attempts = itemList.Count;
    GameObject previousItem = (currentIndex >= 0 && currentIndex < itemList.Count) ? itemList[currentIndex] : null;

    do
    {
        currentIndex = (currentIndex + 1) % itemList.Count;
    } while (IsItemInDropZone(itemList[currentIndex]) && --attempts > 0);

    if (previousItem != null && !IsItemInDropZone(previousItem))
    {
        previousItem.SetActive(false);
    }

    GameObject nextItem = itemList[currentIndex];
    nextItem.SetActive(true);

    // 🔹 Ensure the next item appears at the correct start position
    DraggableItem draggable = nextItem.GetComponent<DraggableItem>();
    if (draggable != null)
    {
        nextItem.transform.position = draggable.GetStartingPosition();
    }
    else
    {
        Debug.LogError($"DraggableItem component missing on {nextItem.name}");
    }

    Debug.Log($"🔁 {nextItem.name} is now active at {nextItem.transform.position}");
}


    public void ShowNextItem(Vector3 spawnPosition)
    {
        int attempts = itemList.Count;
        GameObject previousItem = (currentIndex >= 0 && currentIndex < itemList.Count) ? itemList[currentIndex] : null;

        do
        {
            currentIndex = (currentIndex + 1) % itemList.Count;
        } while (IsItemInDropZone(itemList[currentIndex]) && --attempts > 0);

        // **Deactivate previous item before activating next**
        if (previousItem != null)
        {
            previousItem.SetActive(false);
        }

        // Activate the next item
        GameObject nextItem = itemList[currentIndex];
        nextItem.SetActive(true);

        // Ensure the next item appears at the correct start position
        DraggableItem draggable = nextItem.GetComponent<DraggableItem>();
        if (draggable != null)
        {
            nextItem.transform.position = draggable.GetStartingPosition();
        }
        else
        {
            Debug.LogError($"DraggableItem component missing on {nextItem.name}");
        }

        Debug.Log($"{nextItem.name} is now active at {nextItem.transform.position}");
    }

    public void ShowNextAvailableItem()
{
    int attempts = itemList.Count;
    GameObject previousItem = (currentIndex >= 0 && currentIndex < itemList.Count) ? itemList[currentIndex] : null;

    do
    {
        currentIndex = (currentIndex + 1) % itemList.Count;
    } while (IsItemInDropZone(itemList[currentIndex]) && --attempts > 0);

    // ✅ Do NOT deactivate the previous item if it's in a drop zone
    if (previousItem != null && !IsItemInDropZone(previousItem))
    {
        previousItem.SetActive(false);
    }

    // ✅ Activate the next available item
    GameObject nextItem = itemList[currentIndex];
    nextItem.SetActive(true);

    // ✅ Ensure the next item appears at the correct start position
    DraggableItem draggable = nextItem.GetComponent<DraggableItem>();
    if (draggable != null)
    {
        nextItem.transform.position = draggable.GetStartingPosition();
    }
    else
    {
        Debug.LogError($"DraggableItem component missing on {nextItem.name}");
    }

    Debug.Log($"🔹 {nextItem.name} is now active at {nextItem.transform.position}");
}


    private bool IsItemInDropZone(GameObject item)
    {
        Collider2D collider = item.GetComponent<Collider2D>();
        if (collider != null)
        {
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(item.transform.position, 0.1f);
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag("DropZone"))
                {
                    return true;
                }
            }
        }
        return false;
    }
}
