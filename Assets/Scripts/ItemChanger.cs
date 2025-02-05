using UnityEngine;
using System.Collections.Generic;

public class ItemChanger : MonoBehaviour
{
    [Header("List of Items")]
    public List<GameObject> itemList; // Manually updated list of items

    private int currentIndex = -1; // Tracks the currently active item in the list

    void Start()
    {
        // Ensure the item list is not empty before proceeding
        if (itemList == null || itemList.Count == 0)
        {
            Debug.LogError("ItemChanger: itemList is empty! Add items in the Inspector.");
            return;
        }

        //// Deactivate all items at the start
        //foreach (GameObject item in itemList)
        //{
        //    item.SetActive(false);
        //}

        // Activate the first item
        ChangeToNextItem();
    }

    public void ChangeToNextItem()
    {
        // Deactivate the current item, if any
        if (currentIndex >= 0 && currentIndex < itemList.Count)
        {
            itemList[currentIndex].SetActive(false);
        }

        // Increment the index and loop back if necessary
        currentIndex = (currentIndex + 1) % itemList.Count;

        // Activate the next item
        GameObject nextItem = itemList[currentIndex];
        nextItem.SetActive(true);
        Debug.Log($"Item switched to: {nextItem.name}");
    }

    public void ShowNextItem(Vector3 spawnPosition)
    {
        // Increment the index to get the next item
        currentIndex = (currentIndex + 1) % itemList.Count;

        // Activate the next item
        GameObject nextItem = itemList[currentIndex];
        nextItem.SetActive(true);

        // Ensure the next item appears at the correct starting position
        if (spawnPosition != Vector3.zero)
        {
            nextItem.transform.position = spawnPosition;
        }
        else
        {
            DraggableItem draggable = nextItem.GetComponent<DraggableItem>();
            if (draggable != null)
            {
                nextItem.transform.position = draggable.GetStartingPosition();
            }
            else
            {
                Debug.LogError($"DraggableItem component missing on {nextItem.name}");
            }
        }

        Debug.Log($"{nextItem.name} is now active at {nextItem.transform.position}");
    }
}
