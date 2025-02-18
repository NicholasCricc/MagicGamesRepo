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

    public void ShowNextAvailableItem()
{
    if (itemList == null || itemList.Count == 0)
    {
        Debug.LogError("❌ itemList is empty in ShowNextAvailableItem()");
        return;
    }

    int attempts = itemList.Count;
    GameObject previousItem = (currentIndex >= 0 && currentIndex < itemList.Count) ? itemList[currentIndex] : null;

    do
    {
        currentIndex = (currentIndex + 1) % itemList.Count;
    } while (IsItemInDropZone(itemList[currentIndex]) && --attempts > 0);

    if (currentIndex < 0 || currentIndex >= itemList.Count)
    {
        Debug.LogError("❌ currentIndex is out of range in ShowNextAvailableItem()");
        return;
    }

    // ✅ Deactivate the previous item if not in a drop zone
    if (previousItem != null && !IsItemInDropZone(previousItem))
    {
        previousItem.SetActive(false);
    }

    // ✅ Activate and reset position of the next item
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
        Debug.LogError($"❌ DraggableItem component missing on {nextItem.name}");
    }

    Debug.Log($"🔹 {nextItem.name} is now active at {nextItem.transform.position}");
}

public void RegisterItem(GameObject item)
{
    if (!itemList.Contains(item))
    {
        itemList.Add(item); // ✅ Add the item back to the cycling list
        Debug.Log($"🔄 {item.name} re-added to item list.");
    }
}


}
