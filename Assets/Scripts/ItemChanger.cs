using UnityEngine;
using System.Collections.Generic;

public class ItemChanger : MonoBehaviour
{
    [Header("List of Items")]
    public List<GameObject> itemList;
    private int currentIndex = -1;

    void Start()
    {
        if (itemList == null || itemList.Count == 0)
        {
            Debug.LogError("❌ No items in ItemChanger");
            return;
        }

        // Hide all and set back-references
        foreach (var item in itemList)
        {
            item.SetActive(false);
            var cloth = item.GetComponent<ClothingItem>();
            if (cloth != null)
                cloth.parentChanger = this;
        }

        ChangeToNextItem();
    }

    public void ChangeToNextItem()
    {
        if (itemList == null || itemList.Count == 0)
        {
            Debug.LogError("❌ ChangeToNextItem: empty list");
            return;
        }

        if (currentIndex < 0 || currentIndex >= itemList.Count)
            currentIndex = -1;

        // Deactivate old item
        if (currentIndex >= 0)
        {
            var old = itemList[currentIndex];
            old.SetActive(false);
            if (old.TryGetComponent<Collider2D>(out var c)) c.enabled = false;
            if (old.TryGetComponent<DraggableItem>(out var d)) d.enabled = false;
            Debug.Log($"🛑 Deactivated {old.name}");
        }

        // Move pointer
        currentIndex = (currentIndex + 1) % itemList.Count;
        Debug.Log($"➡️ Next index {currentIndex}");

        // Skip placed items
        int safety = 0;
        while (itemList[currentIndex].GetComponent<ClothingItem>().isPlaced && safety < itemList.Count)
        {
            currentIndex = (currentIndex + 1) % itemList.Count;
            safety++;
        }

        if (safety >= itemList.Count)
        {
            Debug.LogWarning("⚠️ All items placed");
            return;
        }

        ActivateItem(currentIndex);
    }

    private void ActivateItem(int index)
    {
        if (index < 0 || index >= itemList.Count)
        {
            Debug.LogError("❌ Invalid ActivateItem index");
            return;
        }

        // Turn off others
        for (int i = 0; i < itemList.Count; i++)
        {
            if (i == index) continue;
            var other = itemList[i];
            var cloth = other.GetComponent<ClothingItem>();
            if (cloth != null && cloth.isPlaced) continue;
            other.SetActive(false);
            if (other.TryGetComponent<Collider2D>(out var c)) c.enabled = false;
            if (other.TryGetComponent<DraggableItem>(out var d)) d.enabled = false;
            Debug.Log($"🛑 Deactivated {other.name}");
        }

        // Show selected
        var current = itemList[index];
        current.SetActive(true);
        if (current.TryGetComponent<Collider2D>(out var cc)) cc.enabled = true;
        if (current.TryGetComponent<DraggableItem>(out var dd))
        {
            dd.enabled = true;
            dd.CacheStartPosition();
        }
        Debug.Log($"✅ Activated {current.name}");
    }

    public void ResetIndex()
    {
        currentIndex = -1;
        Debug.Log("🔄 Index reset");
    }

    public void SetCurrentRodItem(GameObject item)
    {
        int idx = itemList.IndexOf(item);
        if (idx >= 0)
        {
            currentIndex = idx;
            Debug.Log($"🎯 Index set to {idx}");
        }
        else
        {
            Debug.LogWarning($"⚠️ {item.name} not in list");
        }
    }

    public void DeactivateConflictingClothingTypes(ClothingType type)
    {
        foreach (var go in itemList)
        {
            var cloth = go.GetComponent<ClothingItem>();
            if (cloth != null && cloth.clothingType != type && !cloth.isPlaced)
            {
                go.SetActive(false);
                if (go.TryGetComponent<Collider2D>(out var c)) c.enabled = false;
                if (go.TryGetComponent<DraggableItem>(out var d)) d.enabled = false;
                Debug.Log($"🚫 Deactivated conflict {go.name}");
            }
        }
    }

    public bool CompareCurrentRodItem(GameObject item)
    {
        return currentIndex >= 0 && currentIndex < itemList.Count && itemList[currentIndex] == item;
    }

    public void ClearCurrentRodItem(GameObject item)
    {
        if (CompareCurrentRodItem(item))
        {
            currentIndex = -1;
            Debug.Log($"🧹 Cleared {item.name} pointer");
        }
    }

    public void SetCurrentIndex(int index)
    {
        if (index >= 0 && index < itemList.Count)
        {
            currentIndex = index;
            Debug.Log($"🎯 Index set to {index}");
        }
    }
}
