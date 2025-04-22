using UnityEngine;

public class DropZone : MonoBehaviour
{
    public bool isOccupied = false;
    private GameObject currentItem;
    private ItemChanger itemChanger;

    private void Start()
    {
        itemChanger = Object.FindFirstObjectByType<ItemChanger>();
    }

    public GameObject PlaceItem(GameObject newItem)
    {
        Debug.Log($"📥 {newItem.name} is trying to be placed in {gameObject.name}");

        GameObject returnedItem = null;

        if (isOccupied)
        {
            returnedItem = currentItem;
            SwapItem(newItem);
        }
        else
        {
            isOccupied = true;
            currentItem = newItem;
            Debug.Log($"✅ {newItem.name} is now in {gameObject.name}");
        }

        return returnedItem;
    }


    private void SwapItem(GameObject newItem)
    {
        if (currentItem != null)
        {
            Debug.Log($"🔄 Swapping {currentItem.name} with {newItem.name}");

            DraggableItem draggable = currentItem.GetComponent<DraggableItem>();
            if (draggable != null)
            {
                Vector3 returnPosition = draggable.GetStartingPosition();

                // ✅ Ensure swapped item is fully reset
                currentItem.SetActive(false);
                currentItem.transform.position = returnPosition;

                // ✅ Remove drop zone reference to allow clicking
                DraggableItem swappedDraggable = currentItem.GetComponent<DraggableItem>();
                if (swappedDraggable != null)
                {
                    swappedDraggable.ResetDropZoneState(); // ✅ New function to reset drop zone state
                }

                ResetItemForCycling(currentItem);
                Debug.Log($"🔄 {currentItem.name} moved back to {returnPosition} and is now ready for cycling.");
            }
        }

        // ✅ Place new item in drop zone
        isOccupied = true;
        currentItem = newItem;
        newItem.transform.position = transform.position;
        Debug.Log($"✅ {newItem.name} placed in {gameObject.name}");
    }

    private void ResetItemForCycling(GameObject item)
    {
        if (item == null) return;

        Debug.Log($"♻️ Resetting {item.name} for cycling.");

        // ✅ Reset position & scale
        DraggableItem draggable = item.GetComponent<DraggableItem>();
        if (draggable != null)
        {
            item.transform.position = draggable.GetStartingPosition();
            item.transform.localScale = draggable.GetOriginalScale();
            draggable.ResetDropZoneState(); // ✅ Ensure future clicks work
        }

        // ✅ Reactivate and re-enable
        item.SetActive(true);
        EnableItemInteraction(item);

        // ✅ Re-register the item with ItemChanger
        if (itemChanger != null)
        {
            if (!itemChanger.itemList.Contains(item))
            {
                itemChanger.itemList.Add(item);
                Debug.Log($"✅ {item.name} re-added to itemList.");
            }

            itemChanger.ResetIndex(); // force update to avoid out-of-sync cycling

            // ✅ Track this as the current rod item and hide previous one if necessary
            itemChanger.SetCurrentRodItem(item);
        }

        Debug.Log($"✅ {item.name} is now fully reset and ready for interaction.");
    }



    public void EnableItemInteraction(GameObject item)
    {
        if (item == null) return;

        Collider2D collider = item.GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = true;
            Debug.Log($"✅ Collider enabled for {item.name}");
        }

        DraggableItem draggable = item.GetComponent<DraggableItem>();
        if (draggable != null)
        {
            draggable.enabled = true;
            Debug.Log($"✅ DraggableItem script enabled for {item.name}");
        }
    }
}
