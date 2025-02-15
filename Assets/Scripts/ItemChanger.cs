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
        if (currentIndex >= 0 && currentIndex < itemList.Count)
        {
            itemList[currentIndex].SetActive(false);
        }

        currentIndex = (currentIndex + 1) % itemList.Count;
        GameObject nextItem = itemList[currentIndex];
        nextItem.SetActive(true);
        Debug.Log($"Item switched to: {nextItem.name}");
    }

    public void ShowNextItem(Vector3 spawnPosition)
    {
        if (currentIndex >= 0 && currentIndex < itemList.Count)
        {
            itemList[currentIndex].SetActive(false);
        }

        currentIndex = (currentIndex + 1) % itemList.Count;
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
