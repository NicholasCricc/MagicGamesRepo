using UnityEngine;
using System.Collections.Generic;

public class ItemChanger : MonoBehaviour
{
    [Header("List of Items")]
    public List<GameObject> itemList;
    private int currentIndex = -1;
    private Dictionary<ClothingType, GameObject> activeRodItems = new();


    void Start()
    {
        if (itemList == null || itemList.Count == 0)
        {
            Debug.LogError("ItemChanger: itemList is empty! Add items in the Inspector.");
            return;
        }

        foreach (GameObject item in itemList)
        {
            item.SetActive(false);
        }

        ChangeToNextItem();
    }

    public void ChangeToNextItem()
    {
        if (itemList == null || itemList.Count == 0)
        {
            Debug.LogError("❌ itemList is empty in ChangeToNextItem()");
            return;
        }

        GameObject previousItem = (currentIndex >= 0 && currentIndex < itemList.Count) ? itemList[currentIndex] : null;

        int attempts = itemList.Count;
        bool found = false;

        for (int i = 0; i < attempts; i++)
        {
            currentIndex = (currentIndex + 1) % itemList.Count;

            if (!IsItemInDropZone(itemList[currentIndex]))
            {
                found = true;
                break;
            }
        }

        if (!found)
        {
            Debug.LogWarning("⚠️ No available items to cycle through.");
            return;
        }

        // 💡 Register the previous item if it was never tracked
        if (previousItem != null && previousItem.activeSelf)
        {
            ClothingItem prevClothing = previousItem.GetComponent<ClothingItem>();
            if (prevClothing != null && !activeRodItems.ContainsKey(prevClothing.clothingType))
            {
                activeRodItems[prevClothing.clothingType] = previousItem;
                Debug.Log($"📌 [Fallback Register] {previousItem.name} of type {prevClothing.clothingType} added to activeRodItems before cycling.");
            }
        }

        // 🔻 Disable the previous item if needed
        if (previousItem != null && !IsItemInDropZone(previousItem))
        {
            previousItem.SetActive(false);

            Collider2D col = previousItem.GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            DraggableItem drag = previousItem.GetComponent<DraggableItem>();
            if (drag != null) drag.enabled = false;

            ClothingItem clothing = previousItem.GetComponent<ClothingItem>();
            if (clothing != null && activeRodItems.ContainsKey(clothing.clothingType))
            {
                activeRodItems.Remove(clothing.clothingType);
                Debug.Log($"🗑 Removed {previousItem.name} of type {clothing.clothingType} from activeRodItems.");
            }
        }

        GameObject nextItem = itemList[currentIndex];
        EnableItemInteraction(nextItem);

        // 🚨 Absolute failsafe: disable ANY active object in the scene of the same type
        ClothingItem nextClothing = nextItem.GetComponent<ClothingItem>();
        if (nextClothing != null)
        {
            ClothingItem[] allItems = GameObject.FindObjectsOfType<ClothingItem>();
            foreach (ClothingItem c in allItems)
            {
                GameObject item = c.gameObject;
                if (c.clothingType == nextClothing.clothingType && item != nextItem)
                {
                    item.SetActive(false);

                    var col = item.GetComponent<Collider2D>();
                    if (col != null) col.enabled = false;

                    var drag = item.GetComponent<DraggableItem>();
                    if (drag != null) drag.enabled = false;

                    SpriteRenderer sr = item.GetComponent<SpriteRenderer>();
                    if (sr != null) sr.sortingOrder = 2;

                    Debug.Log($"🧹 [Scene-wide cleanup] Disabled {item.name} of type {c.clothingType}");
                }
            }
        }


        Debug.Log("🧭 activeRodItems before SetCurrentRodItem:");
        foreach (var kvp in activeRodItems)
        {
            Debug.Log($"   - {kvp.Key}: {kvp.Value.name} | Active: {kvp.Value.activeSelf}");
        }

        SetCurrentRodItem(nextItem);

        Debug.Log("✅ activeRodItems AFTER SetCurrentRodItem:");
        foreach (var kvp in activeRodItems)
        {
            Debug.Log($"   - {kvp.Key}: {kvp.Value.name} | Active: {kvp.Value.activeSelf}");
        }

        DraggableItem draggable = nextItem.GetComponent<DraggableItem>();
        if (draggable != null)
        {
            draggable.CacheStartPosition();
        }

        Debug.Log($"🔹 {nextItem.name} is now active at {nextItem.transform.position}");
    }



    public void EnableItemInteraction(GameObject item)
    {
        if (item == null) return;

        item.SetActive(true);
        Collider2D collider = item.GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
            collider.enabled = true;
        }

        DraggableItem draggable = item.GetComponent<DraggableItem>();
        if (draggable != null)
        {
            draggable.CacheStartPosition();
            draggable.enabled = false;
            draggable.enabled = true;
        }
    }

    private bool IsItemInDropZone(GameObject item)
    {
        ClothingItem clothing = item.GetComponent<ClothingItem>();
        if (clothing == null) return false;

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(item.transform.position, 0.5f);
        foreach (var hitCollider in hitColliders)
        {
            DropZone zone = hitCollider.GetComponent<DropZone>();
            if (zone != null && zone.AcceptsType(clothing.clothingType))
            {
                return true;
            }
        }

        return false;
    }


    public void MarkItemAsPlaced(GameObject placedItem)
    {
        ClothingItem placedClothing = placedItem.GetComponent<ClothingItem>();
        if (itemList.Contains(placedItem) && placedClothing != null)
        {
            itemList.Remove(placedItem);
            Debug.Log($"✅ {placedItem.name} marked as placed and removed from itemList.");
        }

    }

    public bool HasAvailableItems()
    {
        return itemList != null && itemList.Count > 0;
    }

    public void ResetIndex()
    {
        currentIndex = -1;
    }

    public void ShowNextAvailableItem()
    {
        ChangeToNextItem();
    }

    public bool CompareCurrentRodItem(GameObject item)
    {
        ClothingItem clothing = item.GetComponent<ClothingItem>();
        if (clothing == null) return false;

        return activeRodItems.TryGetValue(clothing.clothingType, out GameObject currentItem) && currentItem == item;
    }


    public void ClearCurrentRodItem(GameObject item)
    {
        ClothingItem clothing = item.GetComponent<ClothingItem>();
        if (clothing == null) return;

        if (activeRodItems.ContainsKey(clothing.clothingType))
        {
            activeRodItems.Remove(clothing.clothingType);
            Debug.Log($"🗑 Cleared rod item for type: {clothing.clothingType}");
        }
    }


    public void SetCurrentRodItem(GameObject item)
    {
        ClothingItem clothing = item.GetComponent<ClothingItem>();
        if (clothing == null) return;

        // If a previous item of the same type exists, disable it
        if (activeRodItems.TryGetValue(clothing.clothingType, out GameObject previousItem) && previousItem != null && previousItem != item)
        {
            if (!IsItemInDropZone(previousItem))
            {
                previousItem.SetActive(false);

                var col = previousItem.GetComponent<Collider2D>();
                if (col != null) col.enabled = false;

                var drag = previousItem.GetComponent<DraggableItem>();
                if (drag != null) drag.enabled = false;

                Debug.Log($"🧹 Previous rod item of type {clothing.clothingType} ({previousItem.name}) fully disabled.");
            }
        }

        // 🔄 Ensure this item is now active and interactable
        if (!item.activeSelf)
        {
            item.SetActive(true);
            Debug.Log($"🔄 {item.name} was re-activated before tracking.");
        }

        activeRodItems[clothing.clothingType] = item;
        Debug.Log($"📌 New rod item set: {item.name} (Type: {clothing.clothingType})");
    }

    public void DeactivateConflictingClothingTypes(ClothingType activeType)
    {
        List<ClothingType> conflictingTypes = new();

        // Define mutually exclusive categories
        if (activeType == ClothingType.FullBody)
        {
            conflictingTypes.Add(ClothingType.Shirt);
            conflictingTypes.Add(ClothingType.Pants);
        }
        else if (activeType == ClothingType.Shirt || activeType == ClothingType.Pants)
        {
            conflictingTypes.Add(ClothingType.FullBody);
        }

        foreach (GameObject item in itemList)
        {
            if (item == null || !item.activeSelf) continue;

            ClothingItem c = item.GetComponent<ClothingItem>();
            if (c != null && conflictingTypes.Contains(c.clothingType))
            {
                item.SetActive(false);

                var col = item.GetComponent<Collider2D>();
                if (col != null) col.enabled = false;

                var drag = item.GetComponent<DraggableItem>();
                if (drag != null) drag.enabled = false;

                Debug.Log($"🛑 [Conflict Cleanup] Deactivated {item.name} of type {c.clothingType} due to conflict with {activeType}");
            }
        }
    }



}
