using UnityEngine;

public enum ClothingType
{
    Hat,
    Glasses,
    Scarf,
    HeadBand,
    Shirt,
    Pants,
    Shoes,
    FullBody,
    Luggage
}

public class ClothingItem : MonoBehaviour
{
    [Tooltip("Assign the type of clothing this item represents.")]
    public ClothingType clothingType;

    // mark when placed so rod‐cycler skips it
    [HideInInspector]
    public bool isPlaced = false;

    // back-pointer for DropZone.ResetItemForCycling
    [HideInInspector]
    public ItemChanger parentChanger;
}
