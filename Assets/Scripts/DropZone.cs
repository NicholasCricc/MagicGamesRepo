using UnityEngine;
using System.Linq;       // Use LINQ for sibling filtering

public class DropZone : MonoBehaviour
{
    public bool isOccupied = false;
    private GameObject currentItem;
    private ItemChanger itemChanger;
    public ClothingType[] acceptedTypes;

    private void Start()
    {
        // Grab the global ItemChanger for cycling
        itemChanger = Object.FindFirstObjectByType<ItemChanger>();
    }

    /// <summary>
    /// Drop handler—returns previously placed item if any.
    /// </summary>
    public GameObject PlaceItem(GameObject newItem)
    {
        var newCloth = newItem.GetComponent<ClothingItem>();
        var allZones = FindObjectsOfType<DropZone>();

        // FullBody clears Shirt & Pants
        if (newCloth.clothingType == ClothingType.FullBody)
        {
            foreach (var z in allZones)
                if (z != this && (z.AcceptsType(ClothingType.Shirt) || z.AcceptsType(ClothingType.Pants)))
                    z.ClearZone();
        }
        // Shirt/Pants clear any FullBody
        else if (newCloth.clothingType == ClothingType.Shirt || newCloth.clothingType == ClothingType.Pants)
        {
            foreach (var z in allZones)
            {
                if (z == this) continue;
                var placed = z.currentItem;
                if (placed != null)
                {
                    var placedCloth = placed.GetComponent<ClothingItem>();
                    if (placedCloth != null && placedCloth.clothingType == ClothingType.FullBody)
                        z.ClearZone();
                }
            }
        }

        // Snap-in & handle old item
        GameObject old = currentItem;
        if (old != null)
            ResetItemForCycling(old);

        currentItem = newItem;
        isOccupied = true;
        newItem.transform.position = transform.position;
        if (newCloth != null) newCloth.isPlaced = true;

        var drag = newItem.GetComponent<DraggableItem>();
        if (drag != null)
        {
            var rod = drag.originalParent?.GetComponent<ItemChanger>();
            if (rod == null)
            {
                drag.originalParent.gameObject.SetActive(false);
                Debug.Log($"🛑 Hiding accessory container for {newItem.name}");
            }
        }

        return old;
    }

    /// <summary>
    /// Remove item from this zone.
    /// </summary>
    public void ClearZone()
    {
        if (currentItem != null)
        {
            ResetItemForCycling(currentItem);
            Debug.Log($"🗑 Cleared {currentItem.name} from {name}");
            currentItem = null;
            isOccupied = false;
        }
    }

    private void ResetItemForCycling(GameObject item)
    {
        var drag = item.GetComponent<DraggableItem>();
        var cloth = item.GetComponent<ClothingItem>();
        if (drag == null || cloth == null || cloth.parentChanger == null)
            return;

        var changer = cloth.parentChanger;

        // Special-case accessories: return, show, and bail
        if (cloth.clothingType == ClothingType.HeadBand || cloth.clothingType == ClothingType.Glasses)
        {
            item.transform.SetParent(drag.originalParent, true);
            item.transform.position = drag.GetStartingPosition();
            item.transform.localScale = drag.GetOriginalScale();
            drag.ResetDropZoneState();

            cloth.isPlaced = false;
            item.SetActive(true);
            if (item.TryGetComponent<Collider2D>(out var col)) col.enabled = true;
            drag.enabled = true;

            changer.ResetIndex();
            changer.SetCurrentRodItem(item);
            changer.DeactivateConflictingClothingTypes(cloth.clothingType);
            Debug.Log($"♻️ Returned {item.name} to rod");
            return;
        }

        // Default return logic
        item.transform.SetParent(drag.originalParent, true);
        item.transform.position = drag.GetStartingPosition();
        item.transform.localScale = drag.GetOriginalScale();
        drag.ResetDropZoneState();

        cloth.isPlaced = false;
        changer.ResetIndex();

        if (!changer.itemList.Contains(item))
        {
            changer.itemList.Add(item);
            Debug.Log($"📋 Re-added {item.name}");
        }

        bool isAccessory = cloth.clothingType == ClothingType.Glasses
                        || cloth.clothingType == ClothingType.Scarf
                        || cloth.clothingType == ClothingType.Hat;
        bool isRodItem = cloth.parentChanger == changer;

        if (isRodItem && !isAccessory)
        {
            item.SetActive(true);
            if (item.TryGetComponent<Collider2D>(out var c)) c.enabled = true;
            drag.enabled = true;
            changer.SetCurrentRodItem(item);
            changer.DeactivateConflictingClothingTypes(cloth.clothingType);
            Debug.Log($"♻️ {item.name} shown on rod");
        }
        else
        {
            item.SetActive(false);
            Debug.Log($"🛑 {item.name} hidden on return");
        }
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
