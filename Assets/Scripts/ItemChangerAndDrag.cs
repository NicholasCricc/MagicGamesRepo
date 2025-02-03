using UnityEngine;
using System.Collections.Generic;

public class ItemChangerAndDrag : MonoBehaviour
{
    [Header("Item List for Clicking")]
    public List<GameObject> itemList; // List of items to cycle through
    private int currentIndex = -1; // Tracks which item is currently active

    private bool isDragging = false;
    private bool isOverDropZone = false;
    private Transform dropZone; // Stores the valid drop position
    private Vector3 startPosition; // Stores the item's original position

    private float clickTime = 0f; // Timer to check if the player is clicking or holding
    private float holdThreshold = 0.3f; // Time required to register a hold instead of a click

    void Start()
    {
        // Store the original position
        startPosition = transform.position;

        // Ensure all items are initially deactivated
        foreach (GameObject item in itemList)
        {
            item.SetActive(false);
        }

        // Activate the first item
        ChangeToNextItem();
    }

    void OnMouseDown()
    {
        clickTime = Time.time; // Register when the click starts
    }

    void OnMouseUp()
    {
        float holdDuration = Time.time - clickTime;

        if (holdDuration < holdThreshold)
        {
            // If the mouse was pressed for a short time, it's a click → change item
            ChangeToNextItem();
        }
        else
        {
            // If the mouse was held down, stop dragging
            isDragging = false;

            if (isOverDropZone)
            {
                // Snap to drop zone
                transform.position = dropZone.position;
            }
            else
            {
                // Return to start position if dropped outside
                transform.position = startPosition;
            }
        }
    }

    void OnMouseDrag()
    {
        float holdDuration = Time.time - clickTime;

        if (holdDuration >= holdThreshold)
        {
            isDragging = true;

            // Drag the item with the mouse
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector3(mousePosition.x, mousePosition.y, startPosition.z);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("DropZone"))
        {
            isOverDropZone = true;
            dropZone = collision.transform;
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("DropZone"))
        {
            isOverDropZone = false;
            dropZone = null;
        }
    }

    void ChangeToNextItem()
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
