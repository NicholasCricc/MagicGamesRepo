using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSceneLoader : MonoBehaviour
{
    [Tooltip("The name of the scene to load when this character is clicked.")]
    public string sceneToLoad;

    private void OnMouseDown()
    {
        // Ensure the scene name is not empty
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.Log("Loading scene: " + sceneToLoad);
            SceneManager.LoadScene(sceneToLoad); // Load the specified scene
        }
        else
        {
            Debug.LogError("Scene name is not set for character: " + gameObject.name);
        }
    }
}
