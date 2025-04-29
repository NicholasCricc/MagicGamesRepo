using UnityEngine;
using System.Linq;       // for LINQ in the sibling cleanup

public class DropZone : MonoBehaviour
{
    public bool isOccupied = false;
    private GameObject currentItem;
    private ItemChanger itemChanger;
    public ClothingType[] acceptedTypes;

    private void Start()
    {
        // find your master ItemChanger so we can re‐add/reset items
        itemChanger = Object.FindFirstObjectByType<ItemChanger>();
    }

    /// <summary>
    /// Called by your DraggableItem when the user drops something here.
    /// Returns any previously‐placed item so you can cycle it back.
    /// </summary>
    public GameObject PlaceItem(GameObject newItem)
    {
        GameObject old = currentItem;

        // 1) Reset the old item if there was one
        if (old != null)
        {
            ResetItemForCycling(old);
        }

        // 2) Snap the new one in
        currentItem = newItem;
        isOccupied = true;
        newItem.transform.position = transform.position;

        var cloth = newItem.GetComponent<ClothingItem>();
        var drag = newItem.GetComponent<DraggableItem>();

        if (cloth != null)
        {
            cloth.isPlaced = true; // mark as placed so cycling skips it
        }

        if (drag != null)
        {
            bool cameFromRod = drag.originalParent.GetComponent<ItemChanger>() != null;

            if (!cameFromRod)
            {
                // ✅ If the newItem is not from a Rod (accessory items like glasses/headbands), deactivate after placing
                drag.originalParent.gameObject.SetActive(false);
                Debug.Log($"🛑 {newItem.name}'s original parent hidden after placement (not from rod)");
            }
        }

        return old;
    }


    /// <summary>
    /// Puts the newItem into this DropZone.
    /// </summary>
    private void SnapIntoZone(GameObject newItem)
    {
        // move into the exact drop‐zone position
        newItem.transform.position = transform.position;

        // mark as “placed” so your cycling code skips it
        var cloth = newItem.GetComponent<ClothingItem>();
        if (cloth != null) cloth.isPlaced = true;

        Debug.Log($"✅ {newItem.name} placed in {name}");
    }

    /// <summary>
    /// Swaps out whatever was here previously (sending it back to its own hanger)
    /// and immediately snaps the newItem into this zone.
    /// </summary>
    private void SwapItem(GameObject newItem)
    {
        // 1) pull out the old one
        GameObject old = currentItem;
        if (old != null)
        {
            ResetItemForCycling(old);
        }

        // 2) snap the new one in
        isOccupied = true;
        currentItem = newItem;
        newItem.transform.position = transform.position;

        var nc = newItem.GetComponent<ClothingItem>();
        if (nc != null) nc.isPlaced = true;

        Debug.Log($"✅ {newItem.name} placed in {name}");
    }

    /// <summary>
    /// Resets a just-removed item back onto its original “rod”
    /// so that it can be cycled again alongside its siblings.
    /// </summary>
    private void ResetItemForCycling(GameObject item)
    {
        var drag = item.GetComponent<DraggableItem>();
        var cloth = item.GetComponent<ClothingItem>();
        if (drag == null || cloth == null || cloth.parentChanger == null) return;

        var changer = cloth.parentChanger;

        // 1) send it home
        item.transform.SetParent(drag.originalParent);
        item.transform.position = drag.GetStartingPosition();
        item.transform.localScale = drag.GetOriginalScale();
        drag.ResetDropZoneState();

        // 2) un-mark
        cloth.isPlaced = false;

        // 3) decide by list membership
        bool isRodItem = changer.itemList.Contains(item);

        if (isRodItem)
        {
            // — rod items: re-enable & keep cycling
            item.SetActive(true);
            if (item.TryGetComponent<Collider2D>(out var col)) col.enabled = true;
            drag.enabled = true;

            // no need to re-add; it was already there
            changer.ResetIndex();
            changer.SetCurrentRodItem(item);
            changer.DeactivateConflictingClothingTypes(cloth.clothingType);
            Debug.Log($"♻️ {item.name} reset on rod");
        }
        else
        {
            // — accessories: hide immediately
            item.SetActive(false);
            Debug.Log($"🛑 {item.name} returned and hidden (not in rod list)");
        }
    }






    public void EnableItemInteraction(GameObject item)
    {
        if (item == null) return;
        if (item.TryGetComponent<Collider2D>(out var c)) c.enabled = true;
        if (item.TryGetComponent<DraggableItem>(out var d)) d.enabled = true;
        Debug.Log($"✅ Interaction re-enabled for {item.name}");
    }

    public bool AcceptsType(ClothingType type)
    {
        return acceptedTypes.Contains(type);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}
