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
        // 1) If we’re dropping a FullBody, first clear any Shirt zones
        var newCloth = newItem.GetComponent<ClothingItem>();
        if (newCloth != null && newCloth.clothingType == ClothingType.FullBody)
        {
            var allZones = FindObjectsOfType<DropZone>();
            foreach (var z in allZones)
            {
                if (z != this && z.AcceptsType(ClothingType.Shirt))
                    z.ClearZone();
            }
        }

        // 2) Now pull out the old one (if any)
        GameObject old = currentItem;
        if (old != null)
        {
            ResetItemForCycling(old);
        }

        // 3) Snap new into place
        currentItem  = newItem;
        isOccupied   = true;
        newItem.transform.position = transform.position;

        // 4) Mark placed so your ItemChanger skips it
        if (newCloth != null)
            newCloth.isPlaced = true;

        // 5) If it didn’t come from a rod, hide its hanger
        var drag = newItem.GetComponent<DraggableItem>();
        if (drag != null)
        {
            var rod = drag.originalParent?.GetComponent<ItemChanger>();
            if (rod == null)
            {
                drag.originalParent.gameObject.SetActive(false);
                Debug.Log($"🛑 {newItem.name}'s original parent hidden (accessory).");
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
        // bail if we can’t figure out what rod it belongs to
        if (drag == null || cloth == null || cloth.parentChanger == null) return;

        var changer = cloth.parentChanger;

        // 1) Send it home: re-parent, reset position/scale, clear drop-zone state
        item.transform.SetParent(drag.originalParent);
        item.transform.position = drag.GetStartingPosition();
        item.transform.localScale = drag.GetOriginalScale();
        drag.ResetDropZoneState();

        // 2) Un-mark so cycling will include it
        cloth.isPlaced = false;

        if (!changer.itemList.Contains(item))
        {
            changer.itemList.Add(item);
            Debug.Log($"📋 {item.name} re-added to {changer.name} list");
        }

        // 3) Is this truly one of this rod’s items?  (rather than an accessory)
        bool isRodItem = cloth.parentChanger == changer;

        if (isRodItem)
        {
            // 1) Send back to original rod, BUT deactivate it
            if (!changer.itemList.Contains(item))
            {
                changer.itemList.Add(item);
                Debug.Log($"📋 {item.name} re-added to {changer.name} list");
            }

            item.SetActive(false); // ✅ Hide immediately
            cloth.isPlaced = false;
            Debug.Log($"🛑 {item.name} returned to {changer.name} and hidden");
        }
        else
        {
            item.SetActive(false); // non-rod accessories
            Debug.Log($"🛑 {item.name} returned and hidden (not part of {changer.name})");
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

    /// <summary>
    /// Clears out whatever was here by resetting it and emptying this zone.
    /// </summary>
    public void ClearZone()
    {
        if (currentItem != null)
        {
            // send it back to its rod
            ResetItemForCycling(currentItem);
            Debug.Log($"🗑 Cleared {currentItem.name} from {name}");

            // now empty the slot
            currentItem = null;
            isOccupied = false;
        }
    }

}
