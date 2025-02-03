using UnityEngine;
using System.Collections.Generic;

public class ItemChanger : MonoBehaviour
{
    [Header("List of Items")]
    public List<GameObject> itemList; // Manually updated list of items

    private int currentIndex = -1; // Tracks the currently active item in the list

    void Start()
    {
        // Ensure all items are initially deactivated
        foreach (GameObject item in itemList)
        {
            item.SetActive(false);
        }

        // Activate the first item
        //ChangeToNextItem();

        // Activate the first item
        ShowNextItem(Vector3.zero); // Initial call with a placeholder position
    }

    // Method to activate the next item in the list
    public void ChangeToNextItem()
    {
        // Deactivate the current item, if any
        if (currentIndex >= 0 && currentIndex < itemList.Count)
        {
            itemList[currentIndex].SetActive(false);
        }

        // Increment the index and loop back to the beginning if necessary
        currentIndex = (currentIndex + 1) % itemList.Count;

        // Activate the next item
        itemList[currentIndex].SetActive(true);
    }

    public void ShowNextItem(Vector3 spawnPosition)
    {
        // Increment the index
        currentIndex = (currentIndex + 1) % itemList.Count;

        // Activate the next item
        GameObject nextItem = itemList[currentIndex];
        nextItem.SetActive(true);

        // Reset the next item to the given spawn position
        if (spawnPosition != Vector3.zero)
        {
            nextItem.transform.position = spawnPosition;
        }
        else
        {
            // Default to initial item position if spawnPosition isn't provided
            //nextItem.transform.position = nextItem.GetComponent<DraggableItem>().GetStartingPosition();
        }
    }
}
