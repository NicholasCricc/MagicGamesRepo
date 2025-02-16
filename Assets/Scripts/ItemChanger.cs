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
        int attempts = itemList.Count; // Prevent infinite loops
        GameObject previousItem = (currentIndex >= 0 && currentIndex < itemList.Count) ? itemList[currentIndex] : null;

        do
        {
            currentIndex = (currentIndex + 1) % itemList.Count;
        } while (itemList[currentIndex].transform.parent != null &&
                 itemList[currentIndex].transform.parent.CompareTag("DropZone") && --attempts > 0);

        if (previousItem != null)
        {
            previousItem.SetActive(false);
        }

        GameObject nextItem = itemList[currentIndex];
        nextItem.SetActive(true);
        Debug.Log($"Item switched to: {nextItem.name}");
    }

    public void ShowNextItem(Vector3 spawnPosition)
    {
        int attempts = itemList.Count;
        GameObject previousItem = (currentIndex >= 0 && currentIndex < itemList.Count) ? itemList[currentIndex] : null;

        do
        {
            currentIndex = (currentIndex + 1) % itemList.Count;
        } while (itemList[currentIndex].transform.parent != null &&
                 itemList[currentIndex].transform.parent.CompareTag("DropZone") && --attempts > 0);

        if (previousItem != null)
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

        Debug.Log($"{nextItem.name} is now active at {nextItem.transform.position}");
    }

    public void ShowNextAvailableItem()
    {
        int attempts = itemList.Count;
        GameObject previousItem = (currentIndex >= 0 && currentIndex < itemList.Count) ? itemList[currentIndex] : null;

        do
        {
            currentIndex = (currentIndex + 1) % itemList.Count;
        } while ((itemList[currentIndex].transform.parent != null &&
                 itemList[currentIndex].transform.parent.CompareTag("DropZone")) ||
                 !itemList[currentIndex].activeSelf && --attempts > 0);

        if (previousItem != null)
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

        Debug.Log($"{nextItem.name} is now active at {nextItem.transform.position}");
    }
}
