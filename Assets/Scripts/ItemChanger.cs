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
        ChangeToNextItem();
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
}
