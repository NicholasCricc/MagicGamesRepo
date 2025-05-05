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
        var newCloth = newItem.GetComponent<ClothingItem>();
        var allZones = FindObjectsOfType<DropZone>();

        // 1) If dropping FullBody → clear Shirt & Pants zones
        if (newCloth.clothingType == ClothingType.FullBody)
        {
            foreach (var z in allZones)
            {
                if (z != this
                 && (z.AcceptsType(ClothingType.Shirt)
                  || z.AcceptsType(ClothingType.Pants)))
                {
                    z.ClearZone();
                }
            }
        }
        // 2) If dropping Shirt or Pants → clear only the zone that actually holds a FullBody
        else if (newCloth.clothingType == ClothingType.Shirt
              || newCloth.clothingType == ClothingType.Pants)
        {
            foreach (var z in allZones)
            {
                if (z == this)
                    continue;

                // look at the item currently in that zone
                var occupant = z.currentItem;
                if (occupant != null)
                {
                    var occCloth = occupant.GetComponent<ClothingItem>();
                    if (occCloth != null
                     && occCloth.clothingType == ClothingType.FullBody)
                    {
                        z.ClearZone();
                    }
                }
            }
        }

        // ────────────────────────────────────────────────
        // 1) Your existing pull-out / snap-in / mark-placed…
        // ────────────────────────────────────────────────
        GameObject old = currentItem;
        if (old != null)
            ResetItemForCycling(old);

        currentItem = newItem;
        isOccupied = true;
        newItem.transform.position = transform.position;

        if (newCloth != null)
            newCloth.isPlaced = true;

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


    private void ResetItemForCycling(GameObject item)
    {
        var drag = item.GetComponent<DraggableItem>();
        var cloth = item.GetComponent<ClothingItem>();
        if (drag == null || cloth == null || cloth.parentChanger == null)
            return;

        var changer = cloth.parentChanger;

        // ── Special-case HeadBand: send it home, show it, then bail out ──
        if (cloth.clothingType == ClothingType.HeadBand)
        {
            // a) reparent & restore its world position/scale
            item.transform.SetParent(drag.originalParent, worldPositionStays: true);
            item.transform.position = drag.GetStartingPosition();
            item.transform.localScale = drag.GetOriginalScale();
            drag.ResetDropZoneState();

            // b) unmark and show
            cloth.isPlaced = false;
            item.SetActive(true);
            if (item.TryGetComponent<Collider2D>(out var col)) col.enabled = true;
            drag.enabled = true;

            // c) reset cycle pointer & slot to this
            changer.ResetIndex();
            changer.SetCurrentRodItem(item);
            changer.DeactivateConflictingClothingTypes(cloth.clothingType);

            Debug.Log($"♻️ {item.name} (HeadBand) returned to rod and shown");
            return;
        }

        // ── All other items follow your normal logic ──

        // 1) reparent & restore
        item.transform.SetParent(drag.originalParent, worldPositionStays: true);
        item.transform.position = drag.GetStartingPosition();
        item.transform.localScale = drag.GetOriginalScale();
        drag.ResetDropZoneState();

        // 2) unmark
        cloth.isPlaced = false;
        changer.ResetIndex();

        // 3) re-add if missing
        if (!changer.itemList.Contains(item))
        {
            changer.itemList.Add(item);
            Debug.Log($"📋 {item.name} re-added to {changer.name} list");
        }

        // 4) decide show vs hide
        bool isAccessory = cloth.clothingType == ClothingType.Glasses
                        || cloth.clothingType == ClothingType.Scarf
                        || cloth.clothingType == ClothingType.Hat; // hats stay hidden until cycled
        bool isRodItem = cloth.parentChanger == changer;

        if (isRodItem && !isAccessory)
        {
            item.SetActive(true);
            if (item.TryGetComponent<Collider2D>(out var c)) c.enabled = true;
            drag.enabled = true;
            changer.SetCurrentRodItem(item);
            changer.DeactivateConflictingClothingTypes(cloth.clothingType);
            Debug.Log($"♻️ {item.name} returned to {changer.name} and shown");
        }
        else
        {
            item.SetActive(false);
            Debug.Log($"🛑 {item.name} returned and hidden");
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