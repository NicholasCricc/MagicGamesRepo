using UnityEngine;

public class DropZone : MonoBehaviour
{
    public bool isOccupied = false;
    private GameObject currentItem; // ✅ Keeps track of the placed item
    private ItemChanger itemChanger; // ✅ Reference to the item changer

    private void Start()
    {
        itemChanger = Object.FindFirstObjectByType<ItemChanger>(); // ✅ Fixed CS0618 warning
    }

    public void PlaceItem(GameObject newItem)
    {
        if (isOccupied)
        {
            SwapItem(newItem);
        }
        else
        {
            isOccupied = true;
            currentItem = newItem;
            Debug.Log($"✅ {newItem.name} is now in {gameObject.name}");
        }
    }

    private void SwapItem(GameObject newItem)
    {
        if (currentItem != null)
        {
            Debug.Log($"🔄 Swapping {currentItem.name} with {newItem.name}");

            // ✅ Move the previous item back to its starting position
            DraggableItem draggable = currentItem.GetComponent<DraggableItem>();
            if (draggable != null)
            {
                currentItem.transform.position = draggable.GetStartingPosition();
                EnableItemInteraction(currentItem); // ✅ Reactivate collider and script
            }
            else
            {
                Debug.LogError($"❌ DraggableItem component missing on {currentItem.name}");
            }

            currentItem.SetActive(false); // ✅ Deactivate but allow cycling later
            ResetItemForCycling(currentItem); // ✅ Ensure it is recognized again
        }

        isOccupied = true;
        currentItem = newItem;
        newItem.transform.position = transform.position;
    }

    private void EnableItemInteraction(GameObject item)
    {
        item.SetActive(true);
        Collider2D collider = item.GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = true; // ✅ Reactivate collider
        }
        DraggableItem draggable = item.GetComponent<DraggableItem>();
        if (draggable != null)
        {
            draggable.enabled = true; // ✅ Reactivate script
        }
    }

    private void ResetItemForCycling(GameObject item)
    {
        if (itemChanger != null)
        {
            itemChanger.RegisterItem(item); // ✅ Ensure `ItemChanger` recognizes this item for cycling
            Debug.Log($"♻️ {item.name} is now re-added for cycling.");
        }
    }
}
