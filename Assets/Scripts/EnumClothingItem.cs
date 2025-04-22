using UnityEngine;

public enum ClothingType
{
    Hat,
    Glasses,
    Scarf,
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
}
