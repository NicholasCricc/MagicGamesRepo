using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager: MonoBehaviour
{

    public void LoadNextScene()
    {
        SceneManager.LoadScene("CharacterSelection");
    }



}
